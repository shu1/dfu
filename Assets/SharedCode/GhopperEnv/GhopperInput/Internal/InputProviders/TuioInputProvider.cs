/* TuioInputProvider.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Translates TUIO state into generic input info
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TuioInputProvider : InputProvider 
{
    Dictionary<int, int> touchToTuio = new Dictionary<int, int>( new IntComparer() );
    Dictionary<int, int> tuioToTouch = new Dictionary<int, int>( new IntComparer() );

    Dictionary<int, Tuio.Touch> currentTouches;

    public override int maxSupportedInputs {
        get {
            return 60;  
        }
    }

    public override void FetchFrameInputs()
    {
        TuioTrackingComponent.GetTouches( out currentTouches );
    }

    public override InputInfo GetInputState( int touchID )
    {
        Tuio.Touch tuioTouch = GetTouchWithID( touchID );
        if ( tuioTouch != null )
            return new InputInfo( tuioTouch.IsCurrent && tuioTouch.Status != Tuio.TouchStatus.Ended,
                                  tuioTouch.TouchPoint, 
                                  (float)tuioTouch.Properties.Width, (float)tuioTouch.Properties.Height, (float)tuioTouch.Properties.Angle );

        return new InputInfo( false, Vector2.zero );
    }

    Tuio.Touch GetTouchWithID( int touchID )
    {
        int tuioID = -1;
        if ( touchToTuio.TryGetValue( touchID, out tuioID ) )
        {
            // touch ID assigned to certain tuio touch
            // check that tuio touch still exists
            Tuio.Touch tuioTouch;
            if ( currentTouches.TryGetValue( tuioID, out tuioTouch ) )
                return tuioTouch;

            // touch doesn't exist
            // de assign
            touchToTuio.Remove( touchID );
            tuioToTouch.Remove( tuioID );
        }
        else
        {
            // no tuio input assigned to this touchID yet
            // assign touchID to first nonassigned tuioID
            foreach( var touch in currentTouches )
            {
                if ( !tuioToTouch.ContainsKey( touch.Key ) )
                {
                    touchToTuio.Add( touchID, touch.Key );
                    tuioToTouch.Add( touch.Key, touchID );

                    return touch.Value;
                }
            }
        }

        return null;
    }
}
