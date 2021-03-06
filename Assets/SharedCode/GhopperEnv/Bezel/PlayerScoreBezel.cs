using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class: PlayerScoreBezel
//		A basic template with four text slots, a background color, and active/inactive indicator, for each player. All of said properties are editable. 
//		
//		*Note: <SetNumberOfPlayers> _must_ be called before setting any other values, otherwise they will be reset.*
//		
//		*Note: Currently seems to assume that players are distributed evenly around the table.*
public class PlayerScoreBezel: GhopperBezelTemplateBase {
	
	private const string TEMPLATE_NAME = "PlayerScore";
	
	const string COMMAND_SET_NUM_PLAYERS = "players";
	const string COMMAND_SET_PLAYER_COLOR = "{0}/color";
	const string COMMAND_SET_PLAYER_NAME = "{0}/name";
	const string COMMAND_SET_PLAYER_SCORE = "{0}/score";
	const string COMMAND_SET_PLAYER_INFO = "{0}/info";
	const string COMMAND_SET_PLAYER_DETAIL = "{0}/detail";
	const string COMMAND_SET_PLAYER_ACTIVE = "{0}/active";
	
	// Constructor: PlayerScoreBezel
	public PlayerScoreBezel() {
		this.templateName = TEMPLATE_NAME;
	}
	public PlayerScoreBezel(int numPlayers) {
		this.templateName = TEMPLATE_NAME;
		SetNumberOfPlayers(numPlayers);
	}
	
	// Function: SetNumberOfPlayers
	//		Sets the number of players to display.
	//		
	//		*Note: Calling this will reset all player properties.*
	//
	// Parameters:
	//		- numPlayers - the number of players to display in the template.
	public void SetNumberOfPlayers(int numPlayers) {
//		DebugExtras.Log("PlayerScore: Set Number players");
		string command = COMMAND_SET_NUM_PLAYERS;
		ArrayList values = new ArrayList();
		
		values.Add(numPlayers);
		
		SetContent(command, values);
	}
	
	// Function: SetPlayerColor
	//		Sets the color for a specific player/wedge.
	//		
	//		*Note: Alpha values may not be supported.*
	//
	// Parameters:
	//		- playerNum - the number of the player that that color should be associated with.
	//		- playerColor - the color to use.
	public void SetPlayerColor(int playerNum, Color32 playerColor) {
//		DebugExtras.Log("PlayerScore: Set player color");
		string command = string.Format(COMMAND_SET_PLAYER_COLOR, playerNum);
		int colorVal = ConversionUtil.ColorToInt(playerColor);
		
		ArrayList values = new ArrayList();
		
		values.Add(colorVal);
		
		SetContent(command, values);
	}
	
	// Function: SetPlayerName
	//		Sets the name text (the upper left field) for a specific player/wedge.
	//
	// Parameters:
	//		- playerNum - the number of the player whose name text should be changed.
	//		- playerName - the string to put in the field.
	public void SetPlayerName(int playerNum, string playerName) {
//		DebugExtras.Log("PlayerScore: Set player name");
		string command = string.Format(COMMAND_SET_PLAYER_NAME, playerNum);
		
		ArrayList values = new ArrayList();
		
		values.Add(playerName);
		
		SetContent(command, values);
	}
	
	// Function: SetPlayerScore
	//		Sets the score text (the upper right field) for a specific player/wedge.
	//
	// Parameters:
	//		- playerNum - the number of the player whose score text should be changed.
	//		- playerScore - the string (or numeric value) to put in the field.
	public void SetPlayerScore(int playerNum, string playerScore) {
//		DebugExtras.Log("PlayerScore: Set player score");
		string command = string.Format(COMMAND_SET_PLAYER_SCORE, playerNum);
		
		ArrayList values = new ArrayList();
		
		values.Add(playerScore);
		
		SetContent(command, values);
	}
	public void SetPlayerScore(int playerNum, float playerScore) {
		SetPlayerScore(playerNum, playerScore.ToString());
	}
	public void SetPlayerScore(int playerNum, int playerScore) {
		SetPlayerScore(playerNum, playerScore.ToString());
	}
	
	// Function: SetPlayerInfo
	//		Sets the info text (the lower left field?) for a specific player/wedge.
	//
	// Parameters:
	//		- playerNum - the number of the player whose info text should be changed.
	//		- playerInfo - the string to put in the field.
	public void SetPlayerInfo(int playerNum, string playerInfo) {
//		DebugExtras.Log("PlayerScore: Set player info");
		string command = string.Format(COMMAND_SET_PLAYER_INFO, playerNum);
		
		ArrayList values = new ArrayList();
		
		values.Add(playerInfo);
		
		SetContent(command, values);
	}
	
	// Function: SetPlayerDetail
	//		Sets the detail text (the lower right field?) for a specific player/wedge.
	//
	// Parameters:
	//		- playerNum - the number of the player whose detail text should be changed.
	//		- playerDetail - the string to put in the field.
	public void SetPlayerDetail(int playerNum, string playerDetail) {
//		DebugExtras.Log("PlayerScore: Set player detail");
		string command = string.Format(COMMAND_SET_PLAYER_DETAIL, playerNum);
		
		ArrayList values = new ArrayList();
		
		values.Add(playerDetail);
		
		SetContent(command, values);
	}
	
	// Function: SetPlayerActive
	//		Sets active/inactive indicator for the player.
	//
	// Parameters:
	//		- playerNum - the number of the player whose "active" status should be changed.
	//		- isActive - whether or not the player is active.
	public void SetPlayerActive(int playerNum, bool isActive) {
//		DebugExtras.Log("PlayerScore: Set player active");
		string command = string.Format(COMMAND_SET_PLAYER_ACTIVE, playerNum);
		
		ArrayList values = new ArrayList();
		
		if (isActive) {
			values.Add(1);
		} else {
			values.Add(0);
		}
		
		SetContent(command, values);
	}
	
}
/*
	Title: Notes
	
  	About: Template Communication details
 	(Current as of 01/15/2013)
 	
 	|	Action				Sender	Address		Option(s)				Value(s)
 	|
	| Specify # of players	Game	players	
	| Set player color		Game	{}/color	position (0 index)		color (int)
	| Set player name		Game	{}/name		position (0 index)		name (string)
	| Set player score		Game	{}/score	position (0 index)		score (string)
	| Set player info		Game	{}/info		position (0 index)		info (string)
	| Set player detail		Game	{}/detail	position (0 index)		detail (string)
	|
*/