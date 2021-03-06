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

public class TouchLinker {
	
	Dictionary<int, MonoBehaviour[]> touchLinks = new Dictionary<int, MonoBehaviour[]>( new IntComparer() );
	List<int> linksToRemove = new List<int>();
	
	public bool DoRayCastAll = false;
	
	class GestureHit
	{
		public List<MonoBehaviour> HitHandlers;
		public RaycastHit Hit;
	}
	
	public LayerMask RaycastLayerMask
	{
		get;
		set;
	}	
	
	List<GestureHit> innerCast(Tuio.Touch t, Ray targetRay)
	{
		List<GestureHit> hitBehaviours = new List<GestureHit>();
		
		RaycastHit innerHit = new RaycastHit();
		
		if (Physics.Raycast(targetRay, out innerHit, 100f, RaycastLayerMask))
		{
			GestureHit h = new GestureHit();
			h.HitHandlers = GetComponentsByInterfaceType<IGestureHandler>(innerHit.collider.transform);
			h.Hit = innerHit;
			hitBehaviours.Add(h);
		}
		return hitBehaviours;
	}
	
	List<GestureHit> innerCastAll(Tuio.Touch t, Ray targetRay)
	{
		List<GestureHit> hitBehaviours = new List<GestureHit>();
		
		RaycastHit[] innerHits = Physics.RaycastAll(targetRay, 100f, RaycastLayerMask);
		
		foreach(RaycastHit innerHit in innerHits)
		{
			GestureHit h = new GestureHit();
			h.HitHandlers = GetComponentsByInterfaceType<IGestureHandler>(innerHit.transform);
			h.Hit = innerHit;
			hitBehaviours.Add(h);
		}
		return hitBehaviours;
	}
	
	public void AddTouch(Tuio.Touch t, Ray toCast)
	{	
		// Add it to the touchlinks
		touchLinks.Add(t.TouchId, new MonoBehaviour[0]);
		
		// Raycast the touch, see what we hit
		List<GestureHit> lh = null;
		if (DoRayCastAll)
		{
			lh = innerCastAll(t, toCast);
		}
		else
		{
			lh = innerCast(t, toCast);
		}
		
		// Update the touch link with the found handlers
		MonoBehaviour[] allHanders = lh.SelectMany(m => m.HitHandlers).ToArray();
		touchLinks[t.TouchId] = allHanders;

		// Notify all handlers
		foreach (GestureHit gh in lh)
        {
			foreach (MonoBehaviour mb in gh.HitHandlers)
			{
            	((IGestureHandler)mb).AddTouch(t, gh.Hit);
			}
        }
	}
	
	public void RemoveTouch(Tuio.Touch t)
	{
		if (!touchLinks.ContainsKey(t.TouchId)) return;
		
		MonoBehaviour[] gestureHandlers = touchLinks[t.TouchId];
		linksToRemove.Add(t.TouchId);
		
		// Notify all enabled handlers
		foreach (MonoBehaviour h in gestureHandlers)
        {
            IGestureHandler handler = (IGestureHandler) h;
            handler.RemoveTouch(t);
        }
	}
	
	public void UpdateTouch(Tuio.Touch t)
	{
		if (!touchLinks.ContainsKey(t.TouchId)) return;
		
		MonoBehaviour[] gestureHandlers = touchLinks[t.TouchId];
		
		// Notify all enabled handlers
		foreach (MonoBehaviour h in gestureHandlers)
        {
            IGestureHandler handler = (IGestureHandler) h;
            handler.UpdateTouch(t);
        }
	}
	
	public void FinishNotification()
	{
		var distinctHandlers = touchLinks
			.SelectMany(g => g.Value)
			.Distinct();
		
		foreach (MonoBehaviour h in distinctHandlers)
		{
			IGestureHandler handler = (IGestureHandler) h;
			handler.FinishNotification();
		}
		RemoveDeadLinks();
	}
	
	void RemoveDeadLinks()
	{
		foreach (int i in linksToRemove)
		{
			touchLinks.Remove(i);
		}
		linksToRemove.Clear();
	}
	
	public List<MonoBehaviour> GetComponentsByInterfaceType<T>(Transform transform)	
	{
		List<MonoBehaviour> finalList = new List<MonoBehaviour>();
		finalList = transform.gameObject.GetComponents(typeof(T)).Select(o => (MonoBehaviour)o).ToList();
		return finalList;
	}
}