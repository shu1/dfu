/* TapRecogniser.cs
 * Copyright Grasshopper 2013 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * A gesture of a tap
 * Discrete (sent when touch has tapped a given number of times without moving too far)
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TapGestureInfo : DiscreteGestureInfo
{
    /// <summary>
    /// Number of taps performed
    /// </summary>
    public int taps { get; private set; }

    /// <summary>
    /// Time when finger was last down
    /// </summary>
    public float lastDown { get; private set; }

    /// <summary>
    /// Time when last tap occured (when finger last went up)
    /// </summary>
    public float lastTap { get; private set; }

    /// <summary>
    /// Whether touch is currently down (right number of fingers down)
    /// </summary>
    public bool down { get { return touchCount == numFingers; } }
   
    int numFingers = 1;
    bool wasDown = false;
    ITappable[] tapping;

    public override void Init( TouchGestureRecogniserBase recogniser, TouchGroup touches, int clusterID )
    {
        base.Init(recogniser, touches, clusterID);

        taps = 0;
        lastDown = lastTap = Time.time;
        numFingers = recogniser.requiredFingerCount;
        if ( startedOver )
            tapping = startedOver.GetComponentsWithInterface<ITappable>();
        else
            tapping = new ITappable[0];
    }

    public override void Update( TouchGroup usingTouches )
    {
        wasDown = down;
        base.Update( usingTouches );

        if ( down )
            lastDown = Time.time;
        else if ( wasDown && touchCount == 0 )
        {
            lastTap = Time.time; 
            taps++;
        }
    }

    public override void NotifyComplete()
    {
        foreach ( var tappable in tapping )
            tappable.CompletedTap( this );
    }

    public override void NotifyFailed()
    {
        foreach ( var tappable in tapping )
            tappable.FailedTap( this );
    }
}

public interface ITappable
{
    void CompletedTap( TapGestureInfo tapInfo );    // sent when given number of taps happen within given time and distance
    void FailedTap( TapGestureInfo tapInfo );   
}

/// <summary>
/// Tap Gesture Recognizer
///   A press and release sequence at the same location
/// </summary>
public class TapRecogniser : DiscreteGestureRecogniser<TapGestureInfo>
{
#if UNITY_IOS
	Dictionary<int, TapGestureInfo> dummy = new Dictionary<int, TapGestureInfo>( new IntComparer() );
#endif

    /// <summary>
    /// Exact number of taps required to succesfully recognize the tap gesture. Must be greater or equal to 1.
    /// </summary>
    /// <seealso cref="Taps"/>
    public int requiredTaps = 1;

    /// <summary>
    /// How far the finger can move from its initial position without making the gesture fail
    /// In screen space
    /// </summary>
    public float moveTolerance = 0.02f;

    /// <summary>
    /// Maximum amount of time the fingers can be held down without failing the gesture. Set to 0 for infinite duration.
    /// </summary>
    public float maxDuration = 0.5f;

    /// <summary>
    /// The maximum amount of the time that can elapse between two consecutive taps without causing the recognizer to reset.
    /// Set to 0 to ignore this setting.
    /// </summary>
    public float maxDelayBetweenTaps = 0.2f;

    public static event TouchGestureEvent<TapGestureInfo> OnCompletedTaps;
    public static event TouchGestureEvent<TapGestureInfo> OnFailedTaps;

    protected override TouchGestureEvent<TapGestureInfo> completedGestureEvent { get { return OnCompletedTaps; } }
    protected override TouchGestureEvent<TapGestureInfo> failedGestureEvent { get { return OnFailedTaps; } }

    bool HasTimedOut( TapGestureInfo gesture )
    {
        // check elapsed time since beginning of gesture
        if( maxDuration > 0 && ( gesture.elapsedTime > maxDuration ) )
            return true;

        // check elapsed time since last tap
        if( requiredTaps > 1 && maxDelayBetweenTaps > 0 && ( Time.time - gesture.lastTap > maxDelayBetweenTaps ) )
            return true;

        return false;
    }

    protected override void OnBegin(TapGestureInfo gesture)
    {
    }

    protected override void ProcessUntrackedCluster( TouchCluster cluster )
    {
        // look for existing tap gestures nearby
        TapGestureInfo nearestGesture = null;
        float distToNearest = float.MaxValue;
        foreach ( var gesture in gestures )
        {
            if ( gesture.Value.state == GestureState.InProgress && !gesture.Value.down )
            {
                float distToGesture = ( gesture.Value.position - cluster.touches.GetAvgScreenPos() ).sqrMagnitude;
                if ( distToGesture < distToNearest && distToGesture < moveTolerance * moveTolerance )
                {
                    nearestGesture = gesture.Value;
                    distToNearest = distToGesture;
                }
            }
        }

        if ( nearestGesture == null )
        {
            TapGestureInfo newGesture = CreateGesture( cluster.touches, cluster.id );
            if ( newGesture != null && CanBegin( newGesture ) )
                Begin( newGesture, cluster.id, cluster.touches );
        }
        else
            nearestGesture.clusterID = cluster.id;
    }

    GestureState RecogniseSingleTap( TapGestureInfo gesture )
    {
        if ( gesture.touchCount != requiredFingerCount )
        {
            // all fingers lifted - fire the tap event
            if( gesture.touchCount == 0 )
                return GestureState.Ended;

            // either lifted off some fingers or added some new ones
            return GestureState.Failed;
        }

        if( HasTimedOut( gesture ) )
            return GestureState.Failed;

        // check if finger moved too far from start position
        float sqrDist = ( gesture.GetTouches().GetAvgScreenPos() - gesture.startPosition ).sqrMagnitude;
        if( sqrDist >= moveTolerance * moveTolerance )
            return GestureState.Failed;

        return GestureState.InProgress;
    }

    GestureState RecogniseMultiTap( TapGestureInfo gesture )
    {
        if ( !gesture.down && gesture.touchCount > 0 && Time.time - gesture.lastDown > 0.25f )
            return GestureState.Failed;
        DebugExtras.Log( "Touch lasted to here" );

        if( HasTimedOut( gesture ) )
            return GestureState.Failed;

        if( gesture.down )
        {
            // check if finger moved too far from start position
            float sqrDist = Vector3.SqrMagnitude( gesture.GetTouches().GetAvgScreenPos() - gesture.startPosition );
            if( sqrDist >= moveTolerance * moveTolerance )
                return GestureState.Failed;
        }
        else if ( gesture.taps >= requiredTaps )
            return GestureState.Ended;

        return GestureState.InProgress;
    }

    protected override GestureState OnRecognise(TapGestureInfo gesture)
    {
        return requiredTaps > 1 ? RecogniseMultiTap( gesture ) : RecogniseSingleTap( gesture );
    }
}
