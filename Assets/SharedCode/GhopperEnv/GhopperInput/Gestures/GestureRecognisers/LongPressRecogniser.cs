/* LongPressRecogniser.cs
 * Copyright Grasshopper 2013 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * A gesture of a held press.
 * Discrete (sent when held for given time)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Holds information about a long press
/// </summary>
public class LongPressGestureInfo : DiscreteGestureInfo
{
    ILongTouchable[] startedTouching;

    public override void Init( TouchGestureRecogniserBase recogniser, TouchGroup touches, int clusterID )
    {
        base.Init( recogniser, touches, clusterID );

        if ( startedOver )
            startedTouching = startedOver.GetComponentsWithInterface<ILongTouchable>();
        else
            startedTouching = new ILongTouchable[0];
    }

    public override void NotifyComplete()
    {
        foreach ( var touching in startedTouching )
            touching.CompletedLongPress( this );
    }

    public override void NotifyFailed()
    {
        foreach ( var touching in startedTouching )
            touching.FailedLongPress( this );
    }
}

public interface ILongTouchable
{
    void CompletedLongPress( LongPressGestureInfo longPressInfo );      // sent when touch held for given time
    void FailedLongPress( LongPressGestureInfo longPressInfo );     // snt when touch moves too much or is released too soon
}

/// <summary>
/// Long-Press gesture: detects when the finger is held down without moving, for a specific duration
/// </summary>
public class LongPressRecogniser : DiscreteGestureRecogniser<LongPressGestureInfo>
{
#if UNITY_IOS
	Dictionary<int, LongPressGestureInfo> dummy = new Dictionary<int, LongPressGestureInfo>( new IntComparer() );
#endif

    /// <summary>
    /// How long the finger must stay down without moving in order to validate the gesture
    /// </summary>
    public float duration = 1.0f;

    /// <summary>
    /// How far the finger is allowed (in pixels) to move around its starting position without breaking the gesture
    /// Scren space
    /// </summary>
    public float moveTolerance = 5f;

    public static event TouchGestureEvent<LongPressGestureInfo> LongPressCompleted;
    public static event TouchGestureEvent<LongPressGestureInfo> LongPressFailed;

    protected override TouchGestureEvent<LongPressGestureInfo> completedGestureEvent { get { return LongPressCompleted; } }
    protected override TouchGestureEvent<LongPressGestureInfo> failedGestureEvent { get { return LongPressFailed; } }

    protected override bool CanBegin( LongPressGestureInfo gesture )
    {
        if ( !base.CanBegin(gesture) )
            return false;

        return true;
    }

    protected override void OnBegin(LongPressGestureInfo gesture)
    {

    }

    protected override GestureState OnRecognise(LongPressGestureInfo gesture)
    {
        if( gesture.touchCount != requiredFingerCount )
            return GestureState.Failed;

        if( gesture.elapsedTime >= duration )
            return GestureState.Ended;

        // check if we moved too far from initial position
        if( gesture.GetTouches().GetAvgDistFromStart().sqrMagnitude > moveTolerance * moveTolerance )
            return GestureState.Failed;

        return GestureState.InProgress;
    }
}
