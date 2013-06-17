/* DiscreteGesture.cs
 * Copyright Grasshopper 2012 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * Base for all gestures that have a single success or failure
 */

using UnityEngine;
using System.Collections;

public abstract class DiscreteGestureInfo : GestureInfo 
{ 
    public DiscreteGestureInfo() : base()
    {
    }

    public abstract void NotifyFailed();
}

public abstract class DiscreteGestureRecogniser<T> : TouchGestureRecogniser<T> where T : DiscreteGestureInfo, new()
{
    protected abstract TouchGestureEvent<T> failedGestureEvent { get; }

    protected override void GestureChanged( T gesture )
    {
        base.GestureChanged( gesture );

        if ( gesture.state == GestureState.Failed && gesture.prevState != GestureState.Failed )
        {
            if ( failedGestureEvent != null )
                failedGestureEvent( gesture );

            gesture.NotifyFailed();
        }
    }
}
