/* ContinuousTouchGesture.cs
 * Copyright Grasshopper 2012 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * Base for all gestures that have a start, update, and end
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ContinuousGestureInfo : GestureInfo
{
    public ContinuousGestureInfo() : base()
    {
    }
    
    public abstract void NotifyRecognised();
    public abstract void NotifyUpdate();
}

/// <summary>
/// NOTE: continuous gestures are responsible for calling RaiseEvent() while State == InProgress in order to raise 
/// an event with Phase.Updated
/// </summary>
public abstract class ContinuousGestureRecogniser<T> : TouchGestureRecogniser<T> where T : ContinuousGestureInfo, new()
{
    protected abstract TouchGestureEvent<T> recognisedGestureEvent { get; }
    protected abstract TouchGestureEvent<T> updatedGestureEvent { get; }

    protected override void OnBegin( T gesture )
    {
        if ( recognisedGestureEvent != null )
            recognisedGestureEvent( gesture );

        gesture.NotifyRecognised();
    }

    protected override void GestureChanged( T gesture )
    {
        base.GestureChanged( gesture );

        if ( gesture.state == GestureState.InProgress )
        {
            if ( updatedGestureEvent != null )
                updatedGestureEvent( gesture );

            gesture.NotifyUpdate();
        }
    }
}
