using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Class: GhopperBezelTemplateBase
//		Base class for bezel templates to extend. 
//		Provides basic/shared functionality, like loading, receiving events, and forwarding configuration commands to the bezel.
//		
//		*Should not be instantiated on its own.*
public class GhopperBezelTemplateBase {
	// Constant: TEMPLATE_NAME
	//		Private. Just stores a default template name, in case the extending class forgets to give its own.
	private const string TEMPLATE_NAME = "NO_TEMPLATE_NAME_GIVEN";
	
	// Variable: templateName
	//		Internal. The <GhopperBezel> uses this value to specify which template to load.
	public string templateName = TEMPLATE_NAME;
	
	// Variable: instanceId
	//		Internal. The instance ID is used to route communication to/from template instances.
	public int instanceId = -1;
	
	// Variable: pendingMessages
	//		Internal. If the template has not loaded yet, outgoing commands are stored here.
	public List<GhopperMessage> pendingMessages = new List<GhopperMessage>();
	
	// Variable: ourBezel
	//		Internal. Reference to the <GhopperBezel> object.
	GhopperBezel ourBezel = null;
	bool isLoaded = false;
	
	// Function: OnLoad
	//		Can be overrided per template, if it's useful to receive a callback when done loading.
	virtual public void OnLoad() {
		// override per template? (if it's useful to receive the callback)
	}
	
	// Function: OnReceiveEvent
	//		Can be overrided per template, if the template sends events (e.g. button-pressing, etc.)
	virtual public void OnReceiveEvent(string eventAddress, ArrayList values) {
//		DebugExtras.Log("Template received event!");
		// override per template? (if it has events)
		DebugExtras.LogWarning(string.Format("Warning: received event, but the template has not overrided OnReceiveEvent"));
	}
	
	// Function: FinishLoading
	//		Internal. Called after the bezel (outside of Unity) has finished loading the template.
	//		Sends any waiting outgoing messages, triggers <OnLoad>, establishes bezel reference.
	public void FinishLoading(GhopperBezel bezel) {
//		DebugExtras.Log("Template finished loading!");
		if (bezel != null && !isLoaded) {
			ourBezel = bezel;
			isLoaded = true;
			MakePendingCalls();
			OnLoad();
		}
	}
	
	// Function: TellBezel
	//		Internal. Sends address & values to <GhopperBezel>, so it can pass on the info the <GhopperDaemon> & out of Unity.
	//		Formats the message address to include the template's id & the template address root.
	//		If the template hasn't loaded yet, it stores the message to send in <FinishLoading>.
	protected void TellBezel(string callAddress, ArrayList values) {
//		DebugExtras.Log("Tell Bezel " + callAddress);
		if (isLoaded) {
			ourBezel.Send("template/" + this.instanceId + GhopperDaemon.ADDRESS_SEPARATOR + callAddress, values);
		} else {
			AddPendingCall(callAddress, values);
		}
	}
	
	// Function: AddPendingCall
	//		Internal. Stores an outgoing message to send it later. Called by <TellBezel> if the template hasn't finished loading.
	void AddPendingCall(string callAddress, ArrayList values) {
//		DebugExtras.Log("Add Pending Call");
		GhopperMessage message = new GhopperMessage();
		message.address = callAddress;
		message.values = values;
		pendingMessages.Add(message);
	}
	
	// Function: MakePendingCalls
	//		Internal. Sends all messages stored by <AddPendingCall>. Called by <FinishLoading>.
	void MakePendingCalls() {
//		DebugExtras.Log("Make Pending Calls");
		int i;
		GhopperMessage[] messagesOut = pendingMessages.ToArray();
		int max = pendingMessages.Count;
		
		if (IsLoaded() && max > 0) {
			DebugExtras.Log(string.Format("sending pending messages for template {0}", this.instanceId));
			pendingMessages.Clear();
			for (i = 0; i < max; i++) {
				GhopperMessage message = messagesOut[i];
				TellBezel(message.address, message.values);
			}
		}
	}
	
	// Function: SetContent
	//		Internal. Sends message to set content, given the content address and values. Both the address and the value set vary per template.
	//		Called by templates that extend this base class. (For an example, see: <PlayerScoreBezel>.)
	protected void SetContent(string contentAddress, ArrayList values) {
//		DebugExtras.Log("Template: set content");
		string callAddress = "content/" + contentAddress;
		TellBezel(callAddress, values);
	}
	
	// Function: Hide
	//		Tells the bezel (outside of Unity) to hide this template, if it's currently showing.
	//		Can be overrided per template if necessary. (For different hide animations, maybe?)
	virtual public void Hide() { // override if so inclined, but not necessary
//		DebugExtras.Log("Template: hide");
		string callAddress = "hide";
		TellBezel(callAddress, new ArrayList());
	}
	
	// Function: Show
	//		Tells the bezel (outside of Unity) to show this template, if it's currently hidden.
	//		Can be overrided per template if necessary. (For different show animations, maybe?)
	//
	// *Note that only one template can be visible at a time.*
	virtual public void Show() {
//		DebugExtras.Log("Template: show");
		string callAddress = "show";
		TellBezel(callAddress, new ArrayList());
	}
	
	public bool IsLoaded() {
		return isLoaded;
	}
	
}
