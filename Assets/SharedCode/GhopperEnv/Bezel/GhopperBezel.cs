using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class: GhopperBezel
//		Manages bezel-related communications & basic functionality. (This may just be used for loading).
//
//		Communication:
//		Incoming: daemon > bezel (> template instance)
//			- template 'loaded'
//			- template-specific events
//			- "alert" message close event
//
//		Outgoing: (template instance >) bezel > daemon
//			- load template (from bezel)
//			- hide/show template (from template)
//			- configure template (from template)
//			- trigger text "splat" or "tell"
//			- trigger "alert" message

public class GhopperBezel : MonoBehaviour {
	// Variables: Internal
	//	daemon 		- Private. Reference to <GhopperDaemon> instance, for comms.
	//	templates 	- Private. List of loading & loaded bezel template instances.
	GhopperDaemon daemon = null; // all comms for the bezel go through the daemon
	List<GhopperBezelTemplateBase> templates = new List<GhopperBezelTemplateBase>();
	
	public delegate void AlertDelegate();
	Queue<AlertDelegate> alertCallbacks = new Queue<AlertDelegate>();
	
	void Start () {
		daemon = gameObject.GetComponent<GhopperDaemon>();
		if (daemon == null) {
			DebugExtras.LogWarning("Warning: Hey! GhopperBezel and GhopperDaemon should be attached to the same GameObject. Otherwise, errors may result.");
		}
	}
	
	// Function: LoadTemplate
	//		Starts loading a new template instance in the Grasshopper shell (outside of Unity).
	//
	//		Parameters:
	//		- newTemplate - An instance of a <GhopperBezelTemplateBase> subclass, representing the bezel template we wish to start loading.
	public void LoadTemplate(GhopperBezelTemplateBase newTemplate) {
		string messageAddress = GhopperDaemon.BEZEL_API_ADDRESS + "load";
		ArrayList values = new ArrayList();
		
		newTemplate.instanceId = templates.Count;
		
		values.Add(newTemplate.templateName);
		values.Add(newTemplate.instanceId);
		
		templates.Add(newTemplate);
		
		daemon.Send(messageAddress, values);
	}

	// Function: Splat
	// 		Displays large text in the center of the screen, oriented to an angle.
	// 		Multiple Splat calls at the same time will queue & be shown in sequence, separately.
	//
	//		Parameters:
	//		- text - The text to display. Can include newlines ( "\n" ).
	//		- angle - The angle in degrees to orient the text to. (0 is "right side up" for projector / player 0.)
	//		- seconds - Duration to show the text. Note that initial tween/animation time is included in this value, but exit animation time is not.
	public void Splat( string text, float angle, float seconds ) {
		ArrayList vals = new ArrayList();
		vals.Add(text);
		vals.Add(angle);
		vals.Add(seconds);
		Send("splat", vals);
	}
	
	// Function: Tell
	// 		Displays medium-sized text that is oriented to an angle in degrees (0 is "right side up" for projector / first player).
	// 		Multiple Tells calls at the same time will all be shown at once (so be mindful of overlaying multiple messages).
	//
	//		NOTE: the horizontal dimensions do not minimize overlap between players' messages -- you should test sending the same message to 6 players at once, if applicable, and check for overlap. Newlines may be a fallback to address any horizontal overlap you find.
	//
	//		Parameters:
	//		- text - The text to display. Can include newlines ( "\n" ).
	//		- angle - The angle in degrees to orient the text to. (0 is "right side up" for projector / player 0.)
	//		- seconds - Duration to show the text. Note that initial tween/animation time is included in this value, but exit animation time is not.
	public void Tell( string text, float angle, float seconds ) {
		ArrayList vals = new ArrayList();
		vals.Add(text);
		vals.Add(angle);
		vals.Add(seconds);
		Send("tell", vals);
	}
	
	// Function: Alert
	// 		Displays a pop-up alert message that is oriented to an angle & must be tapped to close. Closing the the alert can trigger a callback, if one is provided.
	// 		Multiple Alert calls at once will queue & show in sequence.
	//
	//		Parameters:
	//		- text - The text to display. Can include newlines ( "\n" ).
	//		- angle - The angle in degrees to orient the text to. (0 is "right side up" for projector / player 0.)
	//		- onClose - (optional) A callback to trigger when the alert is closed.
	public void Alert( string text, float angle ) {
		Alert(text, angle, null);
	}
	public void Alert( string text, float angle, AlertDelegate onClose ) {
		ArrayList vals = new ArrayList();
		vals.Add(text);
		vals.Add(angle);
		alertCallbacks.Enqueue(onClose);
		Send("alert", vals);
	}
	
	// Function: Send
	//		(Internal) Sends a message (or address+values) to the daemon, with the bezel address prepended
	public void Send(GhopperMessage message) {
		message.address = GhopperDaemon.BEZEL_API_ADDRESS + message.address;
		daemon.Send(message.address, message.values);
	}
	public void Send(string address, ArrayList values) {
		address = GhopperDaemon.BEZEL_API_ADDRESS + address;
		daemon.Send(address, values);
	}
	
	// Function: OnTemplateLoaded
	//		Private. Called internally when a 'template loaded' message is received. Triggers <GhopperBezelTemplateBase>'s <FinishLoading> method.
	void OnTemplateLoaded(int templateId) {
		GhopperBezelTemplateBase template = GetTemplate(templateId);
		if (template != null) {
			template.FinishLoading(this);
		} else {
			DebugExtras.LogWarning(string.Format("WARNING: Received loaded message for template {0}, but template {0} does not exist!", templateId));
		}
	}
	
	// Function: OnTemplateEvent
	//		Private. Called internally when a template event message is received. Passes the event address to the <GhopperBezelTemplateBase>.
	void OnTemplateEvent(int templateId, string eventAddress, ArrayList values) {
        DebugExtras.Log( "OnTemplateEvent " + eventAddress );
		GhopperBezelTemplateBase template = GetTemplate(templateId);
		if (template != null) {
			template.OnReceiveEvent(eventAddress, values);
		} else {
			DebugExtras.LogWarning(string.Format("WARNING: Received event for template {0}, but template {0} does not exist!", templateId));
		}
	}
	
	// Function: OnAlertClose
	//		Private. Called internally when an alert-closed message is received. Calls the next callback in the queue.
	void OnAlertClose() {
		if (alertCallbacks.Count > 0) {
			AlertDelegate callback = alertCallbacks.Dequeue();
			if (callback == null) {
				OnAlertCloseDefault();
			} else {
				callback();
			}
		}
	}
	
	// Function: OnAlertCloseDefault
	//		Private. Default alert-close callback (if none specified in <Alert> call).
	void OnAlertCloseDefault() {
		// DebugExtras.Log("alert closed (default callback)");
	}
	
	// Function: OnReceiveBezelMessage
	//		Internal. Called by <GhopperDaemon> on receiving a message for the bezel
	public void OnReceiveBezelMessage(string messageAddress, ArrayList values) {
       // if ( GhopperDaemon.detailedDebug )
            DebugExtras.Log("Bezel: received message: " + messageAddress);
		
        string[] path = messageAddress.Split(GhopperDaemon.ADDRESS_SEPARATOR);
		if (path.Length >= 2) {
			if (path[0] == "template") 
            {
				string message = path[2];
				int templateId = int.Parse(path[1]);
				switch (message) {
					case "ready":
						OnTemplateLoaded(templateId);
					break;
					case "event":
						int eventAddressStart = 2;
						int eventAddressEnd = path.Length - eventAddressStart;
						string eventAddress = string.Join(GhopperDaemon.ADDRESS_SEPARATOR.ToString(), path, eventAddressStart, eventAddressEnd);
						OnTemplateEvent(templateId, eventAddress, values);
					break;
					default:
						DebugExtras.LogWarning(string.Format("Warning: GhopperBezel does not recognize the following message: {0}", messageAddress));
					break;
				}
			} else if (messageAddress == "alert/close") {
				OnAlertClose();
			}
		}
	}
	
	// Function: GetTemplate
	//		Private. Used in case template storage is changed.
	GhopperBezelTemplateBase GetTemplate(int templateId) {
		return templates[templateId];
	}
	
}
/*
	Title: Notes
	
  	About: Grasshopper protocol for bezel
 	(Dated 01/15/2013 -- missing tell, splat, alert)
 	
 	|	Action						Sender		address								option(s)				value(s)
 	|
	| Load bezel template			Game	/ghpr/bezel/load					template name				id (int)
	| Loaded template is ready		Shell	/ghpr/bezel/template/{}/ready		id of template instance		
	| Show a template instance		Game	/ghpr/bezel/template/{}/show		id of template instance		
	| Hide a template instance		Game	/ghpr/bezel/template/{}/hide		id of template instance		
	| Set bezel template setting	Game	/ghpr/bezel/template/{}/content/*	id of template instance		value of setting (*)	
	| Bezel-based event				Shell	/ghpr/bezel/template/{}/event		id of template instance		name of event (string)
	|
*/