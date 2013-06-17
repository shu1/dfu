/* DragRecogniser.cs
 * Copyright Grasshopper 2013 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * A drag gesture for one or more fingers.
 * Continuous (Press -> Move -> Release)
 */

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Holds information about a drag
/// </summary>
public class DragGestureInfo : ContinuousGestureInfo
{
    /// <summary>
    /// Last screen position of gesture
    /// </summary>
    public Vector2 lastPos;

    /// <summary>
    /// Screen distance moved by gesture since last frame
    /// </summary>
    public Vector2 deltaMove { get { return position - lastPos; } }

    IDraggable[] startDragging; // IDraggables that gesture started over

    public DragGestureInfo() : base()
    {
    }

    public override void Init( TouchGestureRecogniserBase recogniser, TouchGroup touches, int clusterID )
    {
        base.Init(recogniser, touches, clusterID);
        lastPos = position;

        if ( startedOver )
            startDragging = startedOver.GetComponentsWithInterface<IDraggable>();
        else
            startDragging = new IDraggable[0];
    }

    public override void Update( TouchGroup touches )
    {
        lastPos = position;

        base.Update( touches );
    }

    public override void NotifyRecognised()
    {
        foreach ( var draggin in startDragging )
            draggin.StartDrag( this );
    }

    public override void NotifyUpdate()
    {
        foreach ( var dragging in startDragging )
            dragging.UpdateDrag( this );
    }

    public override void NotifyComplete()
    {
        foreach ( var dragging in startDragging )
            dragging.EndDrag( this );
    }
}

/// <summary>
/// OBjects with classes iplementing IDraggable will have these methods called when a drag starts over them
/// </summary>
public interface IDraggable
{
    void StartDrag( DragGestureInfo dragInfo );
    void UpdateDrag( DragGestureInfo dragInfo );
    void EndDrag( DragGestureInfo dragInfo );
}

/// <summary>
/// Drag Gesture Recognizer
///   A full finger press > move > release sequence
/// </summary>
public class DragRecogniser : ContinuousGestureRecogniser<DragGestureInfo>
{
#if UNITY_IOS
	Dictionary<int, DragGestureInfo> dummy = new Dictionary<int, DragGestureInfo>( new IntComparer() );
#endif

    /// <summary>
    /// How far the finger is allowed to move from its initial position without making the gesture fail
    /// In screenspace
    /// 0 - no movement, 2 - full screen movement
    /// </summary>
    [Range( 0, 2 )]
    public float moveTolerance = 1f;

    /// <summary>
    /// Applies for multi-finger drag gestures only:
    /// Check if the gesture should fail when the fingers do not move in the same direction
    /// </summary>
    public bool applySameDirectionConstraint = false;

    public static event TouchGestureEvent<DragGestureInfo> StartDrag;   // drag events
    public static event TouchGestureEvent<DragGestureInfo> UpdatedDrag;
    public static event TouchGestureEvent<DragGestureInfo> EndDrag;

    protected override TouchGestureEvent<DragGestureInfo> recognisedGestureEvent { get { return StartDrag; } }
    protected override TouchGestureEvent<DragGestureInfo> updatedGestureEvent { get { return UpdatedDrag; } }
    protected override TouchGestureEvent<DragGestureInfo> completedGestureEvent { get { return EndDrag; } }
     
    protected override bool CanBegin( DragGestureInfo gesture )
    {
        if( !base.CanBegin( gesture ) )
            return false;

        TouchGroup touches = gesture.GetTouches();
        // must have moved beyond move tolerance threshold
        if( touches.GetAvgDistFromStart().sqrMagnitude * InputMaster.pixelToScreenRatio * InputMaster.pixelToScreenRatio > moveTolerance * moveTolerance )
            return false;

        // all touches must be moving
        if( !touches.AllMoving() )
            return false;

        // if multiple touches, make sure they're all going in roughly the same direction
        if( requiredFingerCount >= 2 && applySameDirectionConstraint && !touches.AreMovingInSameDirection( 0.35f ) )
            return false;

        return true;
    }

    protected override GestureState OnRecognise( DragGestureInfo gesture )
    {

        if( gesture.touchCount != requiredFingerCount )
        {
            // fingers were lifted off / added
            return GestureState.Ended;
        }

        TouchGroup touches = gesture.GetTouches();
        if( requiredFingerCount >= 2 && applySameDirectionConstraint && touches.AllMoving() && !touches.AreMovingInSameDirection( 0.35f ) )
            return GestureState.Ended;

        return GestureState.Recognised;
    }
}


