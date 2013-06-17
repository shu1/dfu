using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// internal representation of players; should retain salient cms 'player' properties
public class GhopperPlayer {
	
	// Note: player info vars may change
	
	int id = -1;
    int angle = 0;
	string firstName = string.Empty;
	string lastName = string.Empty;
	Color32 color = Color.white;
	
	public GhopperPlayer() {
		
	}
	
	public GhopperPlayer(GhopperCmsResponse cmsResponse) {
		Dictionary<string, object> data = cmsResponse.data;
		
		if ( data.ContainsKey( "id" ) ) {
			id = (int)data["id"];
		}
		if ( data.ContainsKey( "first_name" ) ) {
			firstName = (string)data["first_name"];
		}
		if ( data.ContainsKey( "last_name" ) ) {
			lastName = (string)data["last_name"];
		}
		if ( data.ContainsKey( "color" ) ) {
			color = ConversionUtil.HexToColor( (string)data["color"] );
		}
	}
	
	public GhopperPlayer(int pid, int pangle, string pfname, string plname, Color pcolor) {
		// Note: player info vars may change
		id = pid;
        angle = pangle;
		firstName = pfname;
		lastName = plname;
		color = pcolor;
	}
	
	public int GetId() {
		return id;
	}

    public int GetAngle()
    {
        return angle;
    }
	
	public string GetFirstName() {
		return firstName;
	}
	
	public string GetLastName() {
		return lastName;
	}
	
	public string GetFullName() {
		return firstName + " " + lastName;
	}
	
	public Color GetColor() {
		return color;
	}
	
}
