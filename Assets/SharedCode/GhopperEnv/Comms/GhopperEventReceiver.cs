using UnityEngine;
using System.Collections;

// Class: GhopperEventReceiver
// 		This class receives game-related events from the GhopperDaemon. 
//		You can receive these events in your game by: 
//			- extending this class and overriding its methods, then
//			- assigning an instance of the extending class to <GhopperDaemon>'s public gameEventReceiver property.

// Note: if we make this an interface, Unity (4.0) does not list it as a public variable of the GhopperDaemon script in the IDE.
// An alternative may be to use delegates that could be assigned by the games; this may be more flexible for game creators.
// (Another alternative may be to have the game edit a stub of GhopperEventReceiver, which we provide in a separate file? ...But this seems messy.)
public class GhopperEventReceiver : MonoBehaviour {
	
	// Function: OnAddPlayer
	// 		This event occurs several times at game start in order to list the initial set of players, but may also occur mid-game.
	//
	//		All games should override this method so that they can handle getting new players.
	//
	//		*Todo: convert playerColor to a Unity 'Color' object. (If it's permanent?)*
	//
	// Parameters:
	//		- playerId - The player ID used by the CMS.
	//		- playerName - The name of the player. (May not match the player entry in the CMS, currently?)
	//		- playerAngle - The angle/position of the player at the circular table.
	//		- playerColor - A color associated with the player. (temporary? --also, note that this is currently an int value)
	public virtual void OnAddPlayer(int playerId, int playerAngle, string playerFirstName, string playerLastName, int playerColor) {
		WarnUnhandledEvent("OnAddPlayer");
	}
	
	// Function: OnRemovePlayer
	// 		This event may occur mid-game if a player drops out.
	//
	//		All games should override this method so that they can handle players that have to leave.
	//
	// Parameters:
	//		- playerId - The player ID used by the CMS
	public virtual void OnRemovePlayer(int playerId) {
		WarnUnhandledEvent("OnRemovePlayer");
	}
	
	// Function: OnReady
	// 		This event occurs at start, and indicates the Daemon has finished sending all initial settings (players, offset, etc).
	//
	//		When the game has finished setup, it should finish things up by calling Show().
	//
	//		All games should override this method so the game can start at the appropriate time.
	public virtual void OnReady() {
		WarnUnhandledEvent("OnGo");
	}
	
	// Function: OnPause
	// 		This event  will occur when a player pauses the game.
	//
	//		All games should override this method so the game responds to players pressing pause.
	//
	//		*Note: as of 1/15/2013, there is no input to pause/unpause yet.*
	public virtual void OnPause() {
		WarnUnhandledEvent("OnPause");
	}
	
	// Function: OnUnpause
	// 		This event  will occur when a player unpauses the game.
	//
	//		All games should override this method so the game responds to players attempting to unpause.
	//
	//		*Note: as of 1/15/2013, there is no input to pause/unpause yet.* (Presumably the paused/unpaused state will be tracked by the Daemon?)
	public virtual void OnUnpause() {
		WarnUnhandledEvent("OnUnpause");
	}
	
	// Function: WarnUnhandledEvent
	// 		Private Function. Formats & logs warning if a required method is not overrided.
	private void WarnUnhandledEvent(string eventName) {
		DebugExtras.LogWarning(string.Format("GhopperEventReceiver tried to call '{0}', but it looks like this method hasn't been overrided yet.", eventName));
	}
}
