/* TouchEventSender.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Drag onto any collider, and the object will be touched under any InputCameras with a layermask including the collider layer.
 * Events are sent for each ITouchable method
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TouchEventSender : BehaviourBase, ITouchable
{
	public event Action<TouchInfo> OnStartTouch;
	public event Action<TouchInfo> OnUpdateTouch;
	public event Action<TouchInfo> OnEndTouch;

	public void StartTouch( TouchInfo touch )
	{
		if ( OnStartTouch != null ) 
            OnStartTouch( touch );
    }

	public void UpdateTouch( TouchInfo touch )
	{
		if ( OnUpdateTouch != null ) 
            OnUpdateTouch( touch );
	}

	public void EndTouch( TouchInfo touch )
	{
		if ( OnEndTouch != null ) 
            OnEndTouch( touch );
	}
}
