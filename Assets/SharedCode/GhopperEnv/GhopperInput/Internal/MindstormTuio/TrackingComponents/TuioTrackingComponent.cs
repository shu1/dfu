/*
Unity3d-TUIO connects touch tracking from a TUIO to objects in Unity3d.

Copyright 2011 - Mindstorm Limited (reg. 05071596)

Author - Simon Lerpiniere

This file is part of Unity3d-TUIO.

Unity3d-TUIO is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Unity3d-TUIO is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser Public License for more details.

You should have received a copy of the GNU Lesser Public License
along with Unity3d-TUIO.  If not, see <http://www.gnu.org/licenses/>.

If you have any questions regarding this library, or would like to purchase 
a commercial licence, please contact Mindstorm via www.mindstorm.com.
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using OSC;
using Tuio;
using System.Linq;

public class TuioTrackingComponent : StaticBehaviour<TuioTrackingComponent>
{   
    static object touchesLock = new object();
    static Dictionary<int, Tuio.Touch> touches = new Dictionary<int, Tuio.Touch>( new IntComparer() );

    static int screenWidth, screenHeight;
	
	static bool isTracking = false;
    
    protected override void Awake ()
    {       
        base.Awake();

        ResetScreenScale();
    }
	
	public static void ResetScreenScale() {
		screenWidth = Screen.width;
        screenHeight = Screen.height;
	}
	
//	public static void StartTracking() {
//		if (!isTracking) 
//		{
//			DebugExtras.Log("tuioTracking.startTracking()");
//	        
//	        TuioTracking tuioTracking = new TuioTracking();
//			OSCReceiver oscReceiver = new OSCReceiver();
//			
//			oscReceiver.ConfigureFramework( new TuioConfiguration() );
//			
//			StartTracking(oscReceiver, tuioTracking);
//	        
//			osc.Start();
//		}
//	}
	
	public static void StartTracking(OSCReceiver oscReceiver, TuioTracking tuioTracking) {
		if (!isTracking) {
			tuioTracking.SetOscIn( oscReceiver );
			isTracking = true;
		}
	}
	
	/// <summary>
	/// Updates all touches with the latest TUIO received data 
	/// </summary>
	public static void NewTouch( Tuio2DCursor cursor )
    {
        lock( touchesLock )
        {
            if ( !touches.ContainsKey( cursor.SessionID ) )
                touches.Add( cursor.SessionID, buildTouch( cursor ) );
        }
    }

    public static void UpdateTouch( Tuio2DCursor cursor )
    {
        lock ( touchesLock )
        {
            Tuio.Touch touch;
            if ( touches.TryGetValue( cursor.SessionID, out touch ) )
                touch.SetNewTouchPoint( getScreenPoint( cursor ), getRawPoint( cursor ) );
        }
    }

    public static void RemoveTouch( int sessionID )
    {
        lock ( touchesLock )
        {
            if ( touches.ContainsKey( sessionID ) )
                touches.Remove( sessionID );
        }
    }

    public static bool TryGetTouch( int tuioID, out Tuio.Touch touch )
    {
        lock ( touchesLock )
        {
            if ( touches.TryGetValue( tuioID, out touch ) )
                return true;
        }

        return false;
    }

    public static void GetTouches( out Dictionary<int, Tuio.Touch> toCopyTo )
    {
        lock ( touchesLock )
        {
            toCopyTo = new Dictionary<int, Tuio.Touch>( touches, new IntComparer() );
        }
    }
	
	static Tuio.Touch buildTouch(Tuio2DCursor cursor)
    {
        TouchProperties prop;
        prop.Acceleration = cursor.Acceleration;
        prop.VelocityX = cursor.VelocityX;
        prop.VelocityY = cursor.VelocityY;
        prop.Height = cursor.Height;
        prop.Width = cursor.Width;
        prop.Angle = cursor.Angle;
		
        Vector2 p = getScreenPoint(cursor);
		Vector2 raw = getRawPoint(cursor);

        Tuio.Touch t = new Tuio.Touch(cursor.SessionID, p, raw);
        t.Properties = prop;

        return t;
    }

    static Vector2 getRawPoint(Tuio2DCursor data)
    {
		Vector2 position = new Vector2(data.PositionX, data.PositionY);
        return position;
    }
	
	static Vector2 getScreenPoint(Tuio2DCursor data)
    {
		Vector2 position = new Vector2(data.PositionX, data.PositionY);
		
		float x1 = getScreenPoint(position.x,
            screenWidth, false);
        float y1 = getScreenPoint(position.y,
            screenHeight, true);

        Vector2 t = new Vector2(x1, y1);
        return t;
    }
	
	static float getScreenPoint(float xOrY, int screenDimension, bool flip)
    {
		// Flip it the get in screen space
		if (flip) xOrY = 0.5f + (0.5f - xOrY);
        xOrY *= (float)screenDimension;
        return xOrY;
    }
}