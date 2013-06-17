/* TouchVisualizer.cs
 * Copyright Grasshopper 2012 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * Simple script to show where touches are being detected
 */

using UnityEngine;
using System.Collections;

public class TouchVisualizer : MonoBehaviour 
{
	
	void OnGUI() {
		int screenHeight = Screen.height;
		Rect posRect = new Rect(0,0,50,50);
		foreach ( var touch in InputMaster.touches )
        {
			posRect.x = touch.screenPos.x - 25;
			posRect.y = screenHeight - (touch.screenPos.y + 25);
			GUI.Box(posRect, touch.touchID.ToString());
		}
	}
}
