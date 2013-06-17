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
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Linq;
using OSC;
using System.Collections;

/// <summary>
/// Tracking implementation for receiving TUIO data and acting upon the events with WPF.
/// </summary>
namespace Tuio
{
	public class TuioTracking
	{
		#region Fields
		public Dictionary<int, Tuio2DCursor> current = new Dictionary<int, Tuio2DCursor>( new IntComparer() );

		List<int> nextAliveCursors = new List<int>();
		Dictionary<int, Tuio2DCursor> nextNewCursors = new Dictionary<int, Tuio2DCursor>( new IntComparer() );
		
		OSCReceiver oscReceiver = null;

		object m_lock = new object(); // for thread safety
		#endregion
		
		// Start tracking for a given receiver -- In progress
		public void SetOscIn( OSCReceiver oscIn ) 
		{
			oscReceiver = oscIn;
			oscReceiver.AddCallback("/tuio/2Dcur", this.ReceiveTuio);
			oscReceiver.AddCallback("/tuio/2Dblb", this.ReceiveTuio);
		}
		
		public void ForceRefresh()
		{
			// For tuio this is only useful to remove stuck points after a TUIO server restart
			lock (m_lock)
			{
				this.current.Clear();
			}
		}
		
		public Tuio2DCursor[] GetTouchArray()
		{
			// For tuio this is only useful to remove stuck points after a TUIO server restart
			lock (m_lock)
			{
				Tuio2DCursor[] ts = this.current.Values.ToArray();
				return ts;
			}
		}

		// OLD
		// Mindstorm's original OSC->Cursors (the actual tracking step), split out of receiveData (above),
		// with the actual update moved into updateCursors
		// -- this assumes we're receiving bundles, which seems to be reasonable for TUIO,
		// but not necessarily for OSC
//		public void ReceiveTuio(OSCBundle bundle) 
//		{
//			//Not currently checked, we probably should!
//			//int fseq = TuioParser.GetSequenceNumber(bundle);
//
//			List<int> alivecursors = TuioParser.GetAliveCursors(bundle);
//			Dictionary<int, Tuio2DCursor> newcursors = TuioParser.GetCursors(bundle);
//			
//			updateCursors(newcursors, alivecursors);
//		}

		public void ReceiveTuio(OSCMessage message) 
        {
			ArrayList values = message.Values;
			string command = "";

			if (TuioParser.IsCursor(message.Address) && values.Count > 0) {
				command = (string)values[0];

				switch ( command ) {
					case "alive": // first message in bundle (aside from "source", which is optional) -- indicates active touches
//					DebugExtras.Log("alive msg");
						nextAliveCursors = TuioParser.GetAliveCursors(message);
						break;

					case "set": // occurs for every touch that changed/moved
//						DebugExtras.Log("set msg");
						Tuio2DCursor newCursor = TuioParser.GetCursor(message);
						nextNewCursors.Add(newCursor.SessionID, newCursor);

						break;

					case "fseq": // last message in bundle -- indicates message's frame sequence (to check if message is in order)
//						DebugExtras.Log("fseq msg");
						// since this is always at the end of the bundle in TUIO 1.1, this cues the update
						
						//Not currently checked, we probably should! (in case the bundle arrived out of sequence)
						//int fseq = TuioParser.GetSequenceNumber(message);

						updateCursors(nextNewCursors, nextAliveCursors);

						nextNewCursors.Clear();

						break;
				}
			}
		}

		void updateCursors(Dictionary<int, Tuio2DCursor> newCursors, List<int> aliveCursors) 
		{
			// Remove the deleted ones
			removeNotAlive(aliveCursors);

			// Process held/updated items
			updateSetCursors(newCursors, aliveCursors);

			// Process new items
			addNewCursors(newCursors);
		}
	
		void addNewCursor(int id, Tuio2DCursor cursor)
		{
			if (!this.current.ContainsKey(id)) {
				this.TouchAdded(cursor);
			}
		}
	
		void addNewCursors(Dictionary<int, Tuio2DCursor> sets)
		{
//			DebugExtras.Log("try to add "+sets.Count.ToString()+" new cursors to the current "+current.Count.ToString());
			// Get all the cursors we've not got
			var result = (from entry in sets
						  where (!this.current.ContainsKey(entry.Key))
						  select entry.Value);
	
			// Add them
			foreach (Tuio2DCursor cur in result)
				this.TouchAdded(cur);
		}
	
		void updateSetCursors(Dictionary<int, Tuio2DCursor> sets, List<int> alive)
		{
			foreach (int curid in alive)
			{
				//Held cursor
				if (!sets.ContainsKey(curid) && this.current.ContainsKey(curid))
				{
					this.TouchHeld(curid);
				}
				else
				{
					if (sets.ContainsKey(curid) && this.current.ContainsKey(curid))
					{
						Tuio2DCursor cur = sets[curid];
						if (cur.IsEqual(this.current[curid]))
						{
							//Call touchheld if same value
							this.TouchHeld(curid);
						}
						else
						{
							this.TouchUpdated(cur);
						}
					}
				}
			}
		}
	
		void removeNotAlive(List<int> alive)
		{
			// Get all the ones to delete
			var result = (from entry in this.current
						  where (!alive.Contains(entry.Key))
						  select entry.Key).ToArray<int>();
	
			// Delete them
			foreach (int i in result)
				this.TouchRemoved(i);
		}
	
		protected void TouchHeld(int touchId)
		{
		}
		
		protected void TouchUpdated(Tuio2DCursor cur)
		{
			TuioTrackingComponent.UpdateTouch( cur );

			lock(m_lock)
			{
				this.current[cur.SessionID] = cur;
			}
		}
	
		protected void TouchAdded(Tuio2DCursor cur)
		{
			TuioTrackingComponent.NewTouch( cur );

			lock(m_lock)
			{
				this.current.Add(cur.SessionID, cur);
			}
		}
	
		protected void TouchRemoved(int touchId)
		{
			TuioTrackingComponent.RemoveTouch( touchId );

			lock(m_lock)
			{
				this.current.Remove(touchId);
			}
		}
	}
}