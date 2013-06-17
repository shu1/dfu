/* TouchGestureRecogniser.cs
 * Copyright Grasshopper 2012 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * Base class for all gesture recognisers, organises gathering relavent touches or touch clusters
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public delegate void TouchGestureEvent<T>( T gestureInfo ) where T : GestureInfo;

public abstract class TouchGestureRecogniserBase : BehaviourBase
{
    public int requiredFingerCount = 1; // fingers needed for gesture
	public bool detailedDebug = false;
}

public abstract class TouchGestureRecogniser<T> : TouchGestureRecogniserBase where T : GestureInfo, new()
{
    public int maxSimultaneousGestures = 1;

    protected Dictionary<int,T> gestures = new Dictionary<int, T>( new IntComparer() );
    List<int> endedGestures = new List<int>();

    /// <summary>
    /// Does this type of recognizer support finger clustering to track simultaneous multi-finger gestures?
    /// </summary>
    public virtual bool supportFingerClustering
    {
        get { return true; }
    }

    protected abstract TouchGestureEvent<T> completedGestureEvent { get; }
    
    protected override void Awake()
    {   
        base.Awake();
    }
    
    protected override void Start()
    {
        base.Start( );

        if ( !supportFingerClustering || requiredFingerCount < 2 )
        {
            InputMaster.WorldTouchStarted += OnTouchStarted;
        }
    }

    void OnTouchStarted( TouchInfo touch )
    {
        if ( !gestures.ContainsKey( touch.touchID ) )
            CreateGesture( new TouchGroup( touch ), touch.touchID );
    }
    
    #region Updates
    protected override void Update()
    {
        foreach ( var gesture in endedGestures )
            gestures.Remove( gesture );

        endedGestures.Clear();

        if( requiredFingerCount == 1 )
            UpdatePerFinger();
        else if ( supportFingerClustering )
            UpdateUsingClusters();
        else
            UpdateWithAllTouches();
    }

    void UpdateWithAllTouches()
    {
        foreach ( var gesture in gestures )
            UpdateGesture( gesture.Value, InputMaster.GetTouches() );
    }

    void UpdateUsingClusters()
    {
        foreach( var cluster in TouchClusterManager.GetClusters() )
            ProcessCluster( cluster );
        foreach( var g in gestures )
        {
            TouchCluster cluster = TouchClusterManager.GetCluster( g.Key );
            TouchGroup touches = cluster == null ? new TouchGroup() : cluster.touches;

            UpdateGesture( g.Value, touches );
        }
    }

    void UpdatePerFinger()
    {
        foreach ( var gesture in gestures )
        {
            TouchGroup touchAsGroup = new TouchGroup();
            TouchInfo thisTouch = InputMaster.GetTouch( gesture.Key );
            if ( thisTouch && thisTouch.isDown )
                touchAsGroup.Add( thisTouch );
            UpdateGesture( gesture.Value, touchAsGroup );
        }
    }
    
    void ProcessCluster( TouchCluster cluster )
    {
        // this cluster already has a gesture associated to it
        if( gestures.ContainsKey( cluster.id ) )
            return;
        
        // only consider clusters that match our gesture's required finger count
        if( cluster.touches.Count != requiredFingerCount )
            return;

        ProcessUntrackedCluster( cluster );
    }

    protected virtual void ProcessUntrackedCluster( TouchCluster cluster )
    {
        // no claims - find an unactive gesture
        T gesture = CreateGesture( cluster.touches, cluster.id );

        if ( gesture == null )
            return; // Can't track any more gestures for some reason
        
        // did we recognize the beginning a valid gesture?
        if( !CanBegin( gesture ) )
            return;
        
        Begin( gesture, cluster.id, cluster.touches );
    }
#endregion

    #region Get/release touches
    protected void Acquire( TouchInfo touch )
    {
        if( !touch.gestureRecognisers.Contains( this ) )
            touch.gestureRecognisers.Add( this );
    }
    
    protected bool Release( TouchInfo touch )
    {
        return touch.gestureRecognisers.Remove( this );
    }

    void ReleaseTouches( GestureInfo gesture )
    {
        foreach( var touch in gesture.GetTouches() )
            Release( touch );
    }
    #endregion

    /// <summary>
    /// This controls whether or not the gesture recognition should begin
    /// </summary>
    /// <param name="touches">The active touches</param>
    protected virtual bool CanBegin( T gesture )
    {
        if( gesture.GetTouches().Count != requiredFingerCount )
            return false;
        
        return true;
    }
    
    /// <summary>
    /// Method called when the gesture recognizer has just started recognizing a valid gesture
    /// </summary>
    /// <param name="touches">The active touches</param>
    protected abstract void OnBegin( T gesture );
    
    /// <summary>
    /// Method called on each frame that the gesture recognizer is in an active state
    /// </summary>
    /// <param name="touches">The active touches</param>
    /// <returns>The new state the gesture recognizer should be in</returns>
    protected abstract GestureState OnRecognise( T gesture );
    
    protected void Begin( T gesture, int clusterId, TouchGroup touches )
    {
        foreach( var touch in touches )
            Acquire( touch );

        gesture.state = GestureState.Started;
        OnBegin( gesture );
    }
    
    void UpdateGesture( T gesture, TouchGroup touches )
    {
		if ( detailedDebug )
        	DebugExtras.Log( "Updating gesture " + gesture.GetType() + " with " + touches.Count + " touches" );
        
		gesture.Update( touches );

        if( gesture.state == GestureState.Ready )
        {
            if( CanBegin( gesture ) )
                Begin( gesture, gesture.clusterID, touches );
            else if ( touches.Count == 0 )
            {
                endedGestures.Add( gesture.clusterID );
                return;
            }
            else
                return;
        }

        if( gesture.state == GestureState.Started )
            gesture.state = GestureState.InProgress;

        GestureState newState = OnRecognise( gesture );
        gesture.state = newState;
        GestureChanged( gesture );
        
        switch( gesture.state )
        {
        case GestureState.InProgress:
        case GestureState.Recognised:
            break;
            
        case GestureState.Ended: // Ended
        case GestureState.Failed:

            ReleaseTouches( gesture );
            endedGestures.Add( gesture.clusterID );
            break;
            
        default:
            DebugExtras.LogError( this + " - Unhandled state: " + gesture.state + ". Failing gesture." );
            gesture.state = GestureState.Failed;
            break;
        }
    }

    #region Changed gestures
    protected virtual void GestureChanged( T gesture )
    {
        if ( gesture.state == GestureState.Ended && gesture.prevState != GestureState.Ended )
        {
            if ( completedGestureEvent != null )
                completedGestureEvent( gesture );

            gesture.NotifyComplete();
        }
    }
    #endregion

    #region Utils
    /// <summary>
    /// Check if all the touches in the list started recently
    /// </summary>
    /// <param name="touches">The touches to evaluate</param>
    /// <returns>True if the age of each touch in the list is under a set threshold</returns>
    protected bool Young( TouchGroup touches )
    {
        TouchInfo oldestTouch = touches.GetOldest();
        
        if( oldestTouch == null )
            return false;

        return Time.time - oldestTouch.timeDown < 0.25f;
    }

    /// <summary>
    /// Instantiate a new gesture object
    /// </summary>
    protected T CreateGesture( TouchGroup touches, int clusterID = -1 )
    {
        if ( clusterID == -1 )
            clusterID = touches[0].touchID;
        
        DebugExtras.Assert( !gestures.ContainsKey( clusterID ) );
        
        T newGesture = new T();
        newGesture.Init( this, touches, clusterID );

        gestures.Add( clusterID, newGesture );
        
        return newGesture;
    }
#endregion
}

public abstract class GestureInfo
{   
    // finger cluster
    public int clusterID { get; set; }
    public float startTime { get; private set; }
    public TouchGestureRecogniserBase recogniser { get; private set; }
    public Vector2 startPosition { get; private set; }
    public GameObject startedOver { get; private set; }

    public Vector2 position { get; private set; }
    public GameObject currentlyOver { get; private set; }
    public GestureState prevState{ get; private set; }

    public float elapsedTime { get { return Time.time - startTime; } }
    public int touchCount { get { return _touches.Count; } }

    protected GestureState _state = GestureState.Ready;
    protected TouchGroup _touches = new TouchGroup();

    /// <summary>
    /// Get or set the current gesture state
    /// </summary>
    public GestureState state
    {
        get { return _state; }
        set
        {
            if ( _state != value || prevState != value )
            {
				if ( recogniser.detailedDebug )
                	DebugExtras.Log( "Setting gesture state to : " + value );
                prevState = _state;
                _state = value;
            }
        }
    }

    public GestureInfo()
    {
    }

    public virtual void Init( TouchGestureRecogniserBase recogniser, TouchGroup touches, int clusterID )
    {
        this.startTime = Time.time;

        this.recogniser = recogniser;
        Update( touches );
        this.startPosition = position;
        this.startedOver = currentlyOver;

        this.clusterID = clusterID;
    }

    public virtual void Update( TouchGroup usingTouches )
    {
        _touches.Clear();
        _touches.AddRange( usingTouches );
        TouchInfo origin = GetOriginTouch();
        if ( origin )
        {
            position = origin.screenPos;
            currentlyOver = origin.touching;
        }
    }
    
    /// <summary>
    /// The fingers that began the gesture
    /// </summary>
    public TouchGroup GetTouches()
    {
        return new TouchGroup( _touches ); 
    }

    /// <summary>
    /// Replace with relevent interface calls (eg: IDraggable.EndGesture( this ) )
    /// </summary>
    public abstract void NotifyComplete();

    /// <summary>
    /// By default, gesture is 'over' the object that oldest touch is over
    /// </summary>
    protected virtual TouchInfo GetOriginTouch()
    {
        return _touches.GetOldest();
    }
}

public enum GestureState
{
    /// <summary>
    /// Gesture is ready to start (not used currently)
    /// </summary>
    Ready,

    /// <summary>
    /// Gesture has begun, but not yet analysed
    /// </summary>
    Started,

    /// <summary>
    /// Gesture is being analysed, still ongoing
    /// </summary>
    InProgress,

    /// <summary>
    /// Gesture has been recognised, but still ongoing (continous only)
    /// </summary>
    Recognised,

    /// <summary>
    /// Gesture has ended successfully
    /// </summary>
    Ended,

    /// <summary>
    /// Gesture failed
    /// </summary>
    Failed
}