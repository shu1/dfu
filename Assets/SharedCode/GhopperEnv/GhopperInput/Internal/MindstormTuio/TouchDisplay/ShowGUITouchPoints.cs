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

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Tuio;

public class ShowGUITouchPoints : MonoBehaviour 
{
	Dictionary<int, GUITexture> touchIcons = new Dictionary<int, GUITexture>( new IntComparer() );
	public GameObject GUITouchIcon;
	
	void Update()
	{
		if (Input.GetKey("escape")) 
		{
			Application.Quit();
		}
	}

	void HandleTouches(Tuio.Touch t)
	{
		switch (t.Status)
		{
		case TouchStatus.Began:
			addTouch(t);
			break;
		case TouchStatus.Ended:
			removeTouch(t);
			break;
		case TouchStatus.Moved:
			updateTouch(t);
			break;
		case TouchStatus.Stationary:
		default:
			break;
		}
	}
	
	void addTouch(Tuio.Touch t)
	{
		addTouchIcon(t);
	}
	
	void removeTouch(Tuio.Touch t)
	{
		removeTouchIcon(t);
	}
	
	void updateTouch(Tuio.Touch t)
	{
		updateTouchIcon(t);
	}
	
	GUITexture addTouchIcon(Tuio.Touch t)
	{
		GameObject touchIcon = (GameObject)Instantiate(GUITouchIcon);
		GUITexture texture = touchIcon.GetComponent<GUITexture>();
		
		setTouchIconPos(texture, t.TouchPoint);
		
		touchIcons.Add(t.TouchId, texture);
		return texture;
	}
	
	void removeTouchIcon(Tuio.Touch t)
	{
		if (!touchIcons.ContainsKey(t.TouchId)) return;
		GUITexture go = touchIcons[t.TouchId];
		touchIcons.Remove(t.TouchId);
		Destroy(go.gameObject);
	}
	
	void updateTouchIcon(Tuio.Touch t)
	{
		if (!touchIcons.ContainsKey(t.TouchId)) return;
		GUITexture go = touchIcons[t.TouchId];
		setTouchIconPos(go, t.TouchPoint);
	}
	
	void setTouchIconPos(GUITexture touchIcon, Vector2 position)
	{
		touchIcon.pixelInset = new Rect(position.x - 16, position.y - 16, 32, 32);
	}
}
