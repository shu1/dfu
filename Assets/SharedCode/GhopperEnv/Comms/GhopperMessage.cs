using System;
using System.Collections;
using UnityEngine;
using System.Text;

// Class: GhopperMessage
// This class represents our <GhopperDaemon> messages internally.
//
// (This more or less just wraps OscMessages, but if OSC implementation changes, our internal comms needn't also change.)
public class GhopperMessage {
	// Variable: address
	// 		The address that the message is/was bound for. Resembles a URL pattern. 
	public string address;
	// Variable: values
	// 		The values (if any) associated with this message.
	public ArrayList values;
	
	// Constructor: GhopperMessage
	public GhopperMessage() {
		values = new ArrayList();
	}
	
	// Constructor: GhopperMessage
	public GhopperMessage(string messageAddress, ArrayList messageValues) {
		this.address = messageAddress;
		if (messageValues != null) {
			this.values = messageValues; 
		} else {
			this.values = new ArrayList(); 
		}
	}
	
	// Function: Log
	// This is a convenience method for logging the message contents.
	public void Log() {
		DebugExtras.Log( this.ToFormattedString() );
	}
	
	// Function: ToFormattedString
	// This is a convenience method for getting the message contents in a human-readable format.
	public string ToFormattedString() {
		int i, max;
		StringBuilder msg = new StringBuilder();
		msg.AppendFormat("address: '{0}'; arguments:",address);
		max = values.Count;
		for (i = 0; i < max; i++) {
			msg.AppendFormat(" '{0}'", values[i]);
		}
		return msg.ToString();
	}
	
	// add OSC conversion? (or provide a separate class for that?)
	// if adding converion, add unhandled OSC type conversion?
}

// Topic: GhopperMessageHandler
// 		This is a delegate for receiving/handling internal daemon/shell messages (<GhopperMessage> instances).
//
// 		Each address root for incoming messages (e.g. regarding bezel, game, or player) is associated with a <GhopperMessageHandler> that is registered with the <GhopperDaemon>.
public delegate void GhopperMessageHandler ( GhopperMessage message );