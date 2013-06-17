/* PinchRecogniser.cs
 * Copyright Grasshopper 2013 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * A gesture of two (TODO : or more) fingers moving away or towards each other
 * Continuous (Moving -> Lifted/Move wrong direction)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Information about a pinch gesture
/// </summary>
public class PinchGestureInfo : ContinuousGestureInfo
{
    /// <summary>
    /// Gap between fingers when gesture started
    /// </summary>
    /// <value>The start gap.</value>
    public float startGap{ get; private set; }  // in pixel space

    /// <summary>
    /// Current gap between fingers
    /// </summary>
    public float gap { get; private set; }

    /// <summary>
    /// How much gap changed since last frame
    /// </summary>
    public float deltaGap { get { return gap - lastGap; } }

    float lastGap;
    IPinchable[] startPinching;

    public PinchGestureInfo() : base()
    {

    }

    public override void Init( TouchGestureRecogniserBase recogniser, TouchGroup touches, int clusterID )
    {
        base.Init(recogniser, touches, clusterID);

        startGap = lastGap = gap;
        
        if ( startedOver )
            startPinching = startedOver.GetComponentsWithInterface<IPinchable>();
        else
            startPinching = new IPinchable[0];
    }

    public override void Update( TouchGroup touches )
    {
        lastGap = gap;

        base.Update( touches );

        if ( touchCount > 1 )
            gap = ( _touches[1].screenPos - _touches[0].screenPos ).magnitude;
    }

    protected override TouchInfo GetOriginTouch()
    {
        Vector2 avgScreenPos = _touches.GetAvgScreenPos();
        return new TouchInfo( -1, touchCount == 0 ? new RaycasterInfo() : _touches[0].raycastInfo, avgScreenPos, 0, 0, 0 );
    }

    public override void NotifyRecognised()
    {
        foreach ( var pinchable in startPinching )
            pinchable.StartPinch( this );
    }

    public override void NotifyUpdate()
    {
        foreach ( var pinchable in startPinching )
            pinchable.UpdatePinch( this );
    }

    public override void NotifyComplete()
    {
        foreach ( var pinchable in startPinching )
            pinchable.EndPinch( this );
    }
}

/// <summary>
/// IPinchable objects will be sent these messages when pinches start, move, and fail/end
/// </summary>
public interface IPinchable
{
    void StartPinch( PinchGestureInfo pinchInfo );
    void UpdatePinch( PinchGestureInfo pinchInfo );
    void EndPinch( PinchGestureInfo pinchInfo );
}

/// <summary>
/// Pinch Gesture Recognizer
///   Two fingers moving closer or further away from each other
/// </summary>
public class PinchRecogniser : ContinuousGestureRecogniser<PinchGestureInfo>
{
#if UNITY_IOS
	Dictionary<int, PinchGestureInfo> dummy = new Dictionary<int, PinchGestureInfo>( new IntComparer() );
#endif
    /// <summary>
    /// this controls how tolerant the pinch gesture detector is to the two fingers
    /// moving in opposite directions.
    /// Setting this to 0 means the fingers have to move in exactly opposite directions to each other.
    /// this value should be kept between 0 and 1 excluded.
    /// </summary>    
    [Range( 0, 1 )]
    public float directionTolerance = 0.3f;

    /// <summary>
    /// Minimum pinch distance (in screen space) required to trigger the pinch gesture
    /// </summary>
    [Range( 0, 1 )]
    public float minDistance = 0.2f;

    public static event TouchGestureEvent<PinchGestureInfo> StartPinch;
    public static event TouchGestureEvent<PinchGestureInfo> UpdatePinch;
    public static event TouchGestureEvent<PinchGestureInfo> EndPinch;

    protected override TouchGestureEvent<PinchGestureInfo> recognisedGestureEvent { get { return StartPinch; } }
    protected override TouchGestureEvent<PinchGestureInfo> updatedGestureEvent { get { return UpdatePinch; } }
    protected override TouchGestureEvent<PinchGestureInfo> completedGestureEvent { get { return EndPinch; } }

    protected override void Awake()
    {
        base.Awake( );

        if ( requiredFingerCount != 2 )
        {
            DebugExtras.LogWarning( "Pinch gesture only supports two fingers at the moment" );
            requiredFingerCount = 2;
        }
    }

    public override bool supportFingerClustering
    {
        get { return true; }
    }

    protected override bool CanBegin( PinchGestureInfo gesture )
    {
		if ( detailedDebug )
      	  DebugExtras.Log( "Checking pinch can begin" );
        if( !base.CanBegin( gesture ) )
            return false;

        TouchGroup touches = gesture.GetTouches();

        if( !touches.AllMoving() )
            return false;

		if ( detailedDebug )
       		DebugExtras.Log( "Pinch touches are all moving" );
        if( !touches.AreMovingInOppositeDirection( directionTolerance ) )
            return false;
        
		if ( detailedDebug )
        	DebugExtras.Log( "All moving in opposite direction. Gap: " + gesture.gap + " Start gap: " + gesture.startGap );

        if( Mathf.Abs( ( gesture.gap - gesture.startGap ) * InputMaster.pixelToScreenRatio ) < minDistance )
            return false;

        return true;
    }

    protected override GestureState OnRecognise( PinchGestureInfo gesture )
    {
        int numTouches = gesture.touchCount;
        if( numTouches != requiredFingerCount )
        {
            if ( numTouches <= 0 )
                return GestureState.Ended;

            // fingers were lifted, but some left
            if( numTouches < requiredFingerCount )
                return GestureState.Recognised;

            // more fingers added, gesture failed
            return GestureState.Failed;
        }

        TouchGroup touches = gesture.GetTouches();
        // dont do anything if both fingers arent moving
        if( !touches.AllMoving() )
            return GestureState.Recognised;

        if( Mathf.Abs( gesture.deltaGap ) > 0.001f )
        {
            if( !touches.AreMovingInOppositeDirection( directionTolerance ) )
            {
                // skip without firing event
                return GestureState.Ended; 
            }
        }

        return GestureState.InProgress;
    }
}
