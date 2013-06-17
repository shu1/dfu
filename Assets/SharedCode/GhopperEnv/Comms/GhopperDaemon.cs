using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Tuio;
using OSC;

// Class: GhopperDaemon
// This class handles communication to/from the Grasshopper daemon (which runs outside of Unity).
// 
// Some examples of communication include:
// - receiving raw TUIO data or player add/remove messages, and 
// - sending bezel template-related and 'game over' messages, etc.
public class GhopperDaemon : MonoBehaviour {
	
	public const char ADDRESS_SEPARATOR = '/';
	
	public const string GRASSHOPPER_API_ADDRESS = "/ghpr/";
	public const string GAME_API_ADDRESS = GRASSHOPPER_API_ADDRESS + "game/";
	public const string PLAYER_API_ADDRESS = GRASSHOPPER_API_ADDRESS + "player/";
	public const string BEZEL_API_ADDRESS = GRASSHOPPER_API_ADDRESS + "bezel/";
	
	const string TOUCH_INPUT_ADDRESS = "/tuio/";
	
	// Constants: BacklogConstants
	// RETRY_WAIT 			- The default time to wait before making a second attempt to send messages. 
	//						(The first attempt may fail if the socket isn't open yet, or if we tried to send too many at once.)
	// MAX_BACKLOG_SALVO 	- The maximum number of "backlogged" messages to send at once. 
	//						(A message may enter the backlog if the first attempt to send it failed.) 
	private const float RETRY_WAIT = 0.5f;
	private const int MAX_BACKLOG_SALVO = 20; // max number of messages that we should send at once
	
	// Variables: UdpConnections
	// hostIP 		- Private. The Daemon's IP address. This is the local address.
	// remotePort 	- Private. The port number that we're sending to, in order to reach the Grasshopper daemon.
	// localPort 	- Private. The port number that we're reading from, in order to receive messages from the Grasshopper daemon.
	// 				*Note: The 'default' udp port for TUIO seems to be 3333. We might switch the local port to this, so TUIO testing is easier for inexperienced devs.*
	// private string hostIP = "127.0.0.1";
	// private int sendPort = 3335;
	private int listenPort = 3336; // this is the default value -- the value may also be set by a launch option "--daemonspeak"
	
	// Variables: BacklogVars
	// backlogOutgoing 		- Private. A collection of messages that we haven't been able to send to the daemon yet (most likely because the socket wasn't ready).
	// retryWait 			- Private. The time we'll wait before trying to send the backlog.
	// consecutiveFails 	- Private. The number of times in a row that we weren't able to send from the backlog, stored in case we encounter other UDP issues.
	private ArrayList backlogOutgoing = new ArrayList();
	private float retryWait = RETRY_WAIT;
	private int consecutiveFails = 0;
	
	private bool didInit = false;
	
    public static bool detailedDebug { get; private set; } 
	
	// Variables: OscStack
	// osc 		- Reference to the OSC parser/tracker for sending/receiving OSC messages. (All TUIO & daemon messages are sent in the OSC format).
	//			*Note: this reference will likely change when switching to a different TUIO implementation.* 
	//			(Mindstorm uses a different OSC lib.)
	// udp 		- Reference to the UDP IO class for sending/receiving OSC messages. (All OSC messages are sent via UDP.)
	//			*Note: this reference may change or disappear altogether when switching to a different TUIO implementation.*
	//			(Mindstorm grouped UDP input & tuio-tracking together.)
//	public Osc osc;//old
//	public UDPPacketIO udp;//old
	public OSCReceiver oscReceiver;
	public OSCSender oscSender;
	
	// Variables: DaemonMessageHandling
	// tuioReceiver 		- Reference to the TUIO input handler.
	// gameEventReceiver 	- Reference to the game event (e.g. new players, "go" message, etc.) handler.
	// bezel 				- Reference to the bezel communication (e.g. load/show/populate template) handler.
	// ghAddressTable 		- Private. Contains the set of root addresses where we expect grasshopper (non-tuio) messages, and associates them with their handlers (delegates).
//	public TouchReader touchReader = null;
	public GhopperEnv gameEventReceiver = null;
	public GhopperBezel bezel = null;
//	private bool usingTouchReader = false;
	
	public TuioTracking tuioTracker = null;
	
	Hashtable ghAddressTable = new Hashtable(); // set of addresses where we expect grasshopper (non-tuio) messages
	List<GhopperMessage> incomingMessages = new List<GhopperMessage>();
	object ghMessageLock = new object(); // for thread safety
	
	// Variables: LocalTestingVars
	// fakeStartupInfo 		- A flag to tell _builds_ to generate initial "player-add" & "go" messages, in lieu of receiving them from the Grasshopper daemon.
	// 						*Make sure this is set to false for production builds.*
	// maxFakePlayers 		- The maximum number of fake players to generate, if applicable. The minimum is currently set to 2.
	// alwaysFakeInEditor 	- A flag to generate initial "player-add" & "go" messages when testing from within the Unity Editor. Does not affect builds.
	public bool fakeStartupInfo = false;
	public int maxFakePlayers = 6;
	public bool alwaysFakeInEditor = true;   
    public bool _detailedDebug = false; 
	
	// Function: Init
	// 		Call this when you're ready to start daemon-based communications.
	// 		*(Actually, this can probably be moved into the MonoBehaviour's Start method, so devs just need to call <SendGameInit> when they're ready.)*
	public void Init() 
    {
		DebugExtras.Log("GhopperDaemon.Init");
        detailedDebug = _detailedDebug;

		if (!didInit) {
			
			GhopperEnv attachedEventReceiver = gameObject.GetComponent<GhopperEnv>();
			if (gameEventReceiver == null) {
				if (attachedEventReceiver != null) {
				gameEventReceiver = attachedEventReceiver;
				} else {
					DebugExtras.LogError("Please attach your GhopperEnv subclass to the same prefab as the GhopperDaemon, or assign it manually to GhopperDaemon.gameEventReceiver before init.");
				}
			}
			
//			usingTouchReader = (touchReader != null);
			
			// recognized (daemon) message address setup
			SetDaemonMessageReceiver(PLAYER_API_ADDRESS, OnReceivePlayerEvent);
			SetDaemonMessageReceiver(GAME_API_ADDRESS, OnReceiveGameEvent);
			SetDaemonMessageReceiver(BEZEL_API_ADDRESS, OnReceiveBezelEvent);
			
			// udp/osc setup -- PENDING UPDATE -- will need to transition to reactivision + mindstorm OSC
//			udp.init(hostIP, remotePort, localPort);
//			osc.init(udp);
//			
//			osc.SetAllMessageHandler(OnReceiveOsc);
			
			// osc (&tuio) setup
			TuioConfiguration receiverConfig = new TuioConfiguration();
			
			oscReceiver = new OSCReceiver();
			oscSender = new OSCSender();
			tuioTracker = new TuioTracking();
			
			oscSender.Start();
			
			receiverConfig.Port = GetLaunchOption("-daemonspeak", listenPort);
			oscReceiver.ConfigureFramework( receiverConfig );
			oscReceiver.SetAllMessageHandler( OnReceiveOsc );
			TuioTrackingComponent.StartTracking(oscReceiver, tuioTracker);
			
			oscReceiver.Start();
			
			didInit = true;
		}
	}

    void OnApplicationQuit()
    {
        if ( oscReceiver != null )
            oscReceiver.Stop();
    }
	
	int GetLaunchOption( string optionName, int defaultValue )
    {
        string[] allParams = GetLaunchParams( );
        bool gotVal = false;
        int parsedVal = -1;
		
        if ( allParams != null )
        {
            for ( int i = 0; i < allParams.Length; i++) 
            {
    			if ( allParams[i].Equals( optionName ) ) 
                {
    				DebugExtras.Log("found option "+optionName);
    				if ( ( i + 1 < allParams.Length ) && int.TryParse( allParams[ i + 1 ], out parsedVal ) ) 
                    {
    					gotVal = true;
    				}
    			}
    		}
        }

		if ( gotVal ) {
			DebugExtras.Log("parsed val "+parsedVal.ToString());
			return parsedVal;
		} else {
			DebugExtras.Log("default val used");
			return defaultValue;
		}
	}
	
	void Update() {
		CheckIncomingMessages();
	}
	
	void CheckIncomingMessages() {
		lock(ghMessageLock) {
			foreach (GhopperMessage msg in incomingMessages) 
            {
                if ( detailedDebug )
    				DebugExtras.Log( "Received: " + msg.ToFormattedString());
				RouteDaemonMessage(msg);
			}
			
			incomingMessages.Clear();
		}
	}
	
	// Function: SetDaemonMessageReceiver
	//		 Private. Associates a particular incoming message address with a <GhopperMessageHandler>.
	void SetDaemonMessageReceiver(string key, GhopperMessageHandler handler) {
		Hashtable.Synchronized(ghAddressTable).Add(key, handler);
	}
	
	// Function: OnReceiveOsc
	// 		Private. Takes in osc messages and routes them to internal sorting for /tuio/ (as OSC) or /ghpr/ (converted to <GhopperMessage>).
	void OnReceiveOsc(OSCMessage message) {
//		DebugExtras.Log("OnReceiveOsc!");
//		if (message.Address.StartsWith(TOUCH_INPUT_ADDRESS)) {
//			OnReceiveTuio(message);
//		} else 
        if (message.Address.StartsWith(GRASSHOPPER_API_ADDRESS)) {
			GhopperMessage ghMessage = new GhopperMessage(message.Address, message.Values);
			OnReceiveDaemonMessage(ghMessage);
		} else {
			OnReceiveOtherOsc(message);
		}
	}
	
	// Function: OnReceiveTuio
	// 		(Internal) Takes in TUIO OSC messages received from the daemon and sends them along to the TUIO tracker.
	//		*May change with TUIO different implementation--currently sends a MakingThings' OscMessage to an OscMessageHandler*
	public void OnReceiveTuio(OSCMessage message) {
//		DebugExtras.Log("daemon received tuio");
//		DebugLogOsc(message);
//		if (usingTouchReader) {
//			touchReader.tuioHandler( message );
//		}
//		OSCMessage convertedMessage = new OSCMessage( message.Address );
//		convertedMessage.SetValues( message.Values );
//		tuioTracker.ReceiveTuio( convertedMessage );
	}
	
	// Function: OnReceiveDaemonMessage
	// 		(Internal) Takes in <GhopperMessage>s received from the daemon and routes them to the handler associated with their address.
	public void OnReceiveDaemonMessage(GhopperMessage message) {
		lock ( ghMessageLock ) {
			incomingMessages.Add(message);
		}
	}
	
	public void RouteDaemonMessage(GhopperMessage message) {
		GhopperMessageHandler handler = null;
		int longestAddressLength = 0;
		
		// find best-matching address, and send the message to the handler for that address
		foreach (string address in ghAddressTable.Keys) { 
			// find the longest partial match for address (could stand to be optimized)
			if (message.address.StartsWith(address) && address.Length > longestAddressLength) {
				longestAddressLength = address.Length;
				handler = (GhopperMessageHandler)ghAddressTable[address];
			}
		}
		if (handler != null) {
            handler(message);
		} else {
			DebugExtras.LogWarning("No handler found for this address: "+message.address);
		}
	}
	
	// Function: OnReceiveOtherOsc
	// 		(Internal) Takes in OscMessages received from the daemon that couldn't be matched to a known root address & logs them.
	public void OnReceiveOtherOsc( OSCMessage message )
    {
        if ( detailedDebug )
            DebugLogOsc( message );
	}
	
	// Function: OnReceivePlayerEvent
	// 		(Internal) Receives /ghpr/player/ messages and triggers the appropriate <gameEventReceiver> method, passing along the salient values.
	public void OnReceivePlayerEvent(GhopperMessage message) {
		string[] signature = message.address.Substring(PLAYER_API_ADDRESS.Length).Split(ADDRESS_SEPARATOR);
		int playerId = int.Parse(signature[0]);
		string method = signature[1];
		ArrayList values = message.values;
		
		if (method == "add") { // vars: (message sender--ignore this), id (int), angle (int), first name (string), last name (string), color (int)
			GhopperEnv.OnAddPlayer(playerId, (int)values[1], values[2].ToString(), values[3].ToString(), (int)values[4]);
//			gameEventReceiver.OnAddPlayer(playerId, (int)values[1], values[2].ToString(), values[3].ToString(), (int)values[4]);
		} else if (method == "remove") { // vars: (none)
			GhopperEnv.OnRemovePlayer(playerId);
//			gameEventReceiver.OnRemovePlayer(playerId);
		} else {
			DebugExtras.LogWarning("Unrecognized player api method: " + method);
		}
	}
	
	// Function: OnReceiveGameEvent
	// 		(Internal) Receives /ghpr/game/ messages and triggers the appropriate <gameEventReceiver> method, passing along the salient values.
	//		(Also handles offset internally. Maybe this message should be moved to a different address? '/ghpr/screen/', perhaps?)
	public void OnReceiveGameEvent(GhopperMessage message) {
		string[] signature = message.address.Substring(GAME_API_ADDRESS.Length).Split(ADDRESS_SEPARATOR);
		string method = signature[0];
		ArrayList values = message.values;
		if (method == "ready") { // vars: (none)
			TuioTrackingComponent.ResetScreenScale();
			gameEventReceiver.OnReady();
		} else if (method == "pause") { // vars: (none)
			gameEventReceiver.OnPause();
		} else if (method == "unpause") { // vars: (none)
			gameEventReceiver.OnUnpause();
		} else if (method == "offset") { // vars: (x offset, y offset) -- NOTE: the daemon will handle this on its own
			OnReceiveScreenOffset((int)values[1], (int)values[2]);
		} else {
			DebugExtras.LogWarning("Unrecognized game api method: " + method);
		}
	}
	
	// Function: OnReceiveBezelEvent
	// 		(Internal) Receives /ghpr/bezel/ messages and sends them to the bezel instance for further parsing.
	public void OnReceiveBezelEvent(GhopperMessage message) 
    {
		string eventAddress = message.address.Substring(BEZEL_API_ADDRESS.Length);
		bezel.OnReceiveBezelMessage(eventAddress, message.values);
	}
	
	// Function: OnReceiveScreenOffset
	// 		(Internal) Receives screen offset amounts & triggers the windowpusher
	public void OnReceiveScreenOffset(int x, int y) {
		// todo: send pixel offset to tuio / input
	}
	
	// Function: SendGameInit
	// 		Sends the 'game/init' message to Grasshopper daemon, informing it that the game is initialized and 
	//		ready to receive initial setup information (e.g. players & offset), then start.
	public void SendGameInit() {
//		DebugExtras.Log("ghDaemon: SendInit()");
		Send(GAME_API_ADDRESS+"init", new ArrayList());
#if UNITY_EDITOR || UNITY_IPHONE || UNITY_ANDROID
		if (alwaysFakeInEditor || fakeStartupInfo) { // test in editor without affecting build
			Invoke("TriggerSampleStartup", RETRY_WAIT);
		}
#else
		if (fakeStartupInfo) {
			DebugExtras.LogWarning("GhopperDaemon's fakeStartupInfo bool is true. Make sure you set that to false before making a release build.");
			Invoke("TriggerSampleStartup",RETRY_WAIT);
		}
#endif
	}
	
	public void SendShowGame() {
		Send(GAME_API_ADDRESS+"show", new ArrayList());
	}
	
	// Function: SendGameEnd
	// 		Sends the 'game/end' message to Grasshopper daemon, informing it that the game is finished and can be closed.
	//		*This should make the daemon (outside of Unity) quit the game.*
	public void SendGameEnd() {
//		DebugExtras.Log("ghDaemon: SendEnd()");
		Send(GAME_API_ADDRESS+"end", new ArrayList());
	}
	
	// Function: SendBigMoment
	// 		(stub) Sends the 'big moment' message to the Grasshopper daemon.
	//		*This is just a stub. This method is neither finalized (signature-wise) nor implemented.*
	public void SendBigMoment() 
    {
        if ( detailedDebug )
		    DebugExtras.Log("ghDaemon: SendBigMoment()");

		DebugExtras.LogWarning("SendBigMoment is not yet implemented.");
	}
	
	// Function: DebugLogOsc
	// 		(Internal) Logs OSC message, if necessary.
	public void DebugLogOsc(OSCMessage oM) 
    {
		DebugExtras.Log(oM.ToString());
	}
	
	// Function: Send
	//		(Internal) Sends an address and values to the Grasshopper daemon (outside of Unity).
	public void Send (string address, ArrayList values) {
//		DebugExtras.Log("ghDaemon: Send()");
		
		OSCMessage msg = new OSCMessage(address);
		values.Insert( 0, "game"); // for message routing, the first value tells the daemon who the sender is (in all of our cases, it's the game!)
		msg.SetValues(values);
		
		if (oscSender.IsRunning()) {
			if (backlogOutgoing.Count > 0) {
				SendBacklog();	
			}
			oscSender.Send(msg);
		} else {
			DebugExtras.Log("socket ain't open yet.");
			consecutiveFails++;
			backlogOutgoing.Add(msg);
			Invoke("SendBacklog", retryWait);
		}
	}
	
	// Function: SendBacklog
	//		(Internal) Sends backlogged OscMessages to the Grasshopper daemon (outside of Unity).
	// 		(A message can be backlogged if communication has not yet been achieved.)
	void SendBacklog() {
//		DebugExtras.Log("SendBacklog");
		int count = 0;
		if (oscSender.IsRunning()) {
			consecutiveFails = 0;
			while (backlogOutgoing.Count > 0 && count < MAX_BACKLOG_SALVO) {
				oscSender.Send( (OSCMessage)backlogOutgoing[0] );
				backlogOutgoing.RemoveAt(0);
				count++;
			}
			// if we still have messages to send, wait for a bit
			if (backlogOutgoing.Count > 0) {
				DebugExtras.Log("we've still got messages in the backlog. we'll send those in a bit.");
				Invoke("SendBacklog", retryWait);
			}
		} else {
			DebugExtras.Log("can't send daemon message backlog -- socket ain't open yet.");
			consecutiveFails++;
			Invoke("SendBacklog", retryWait);
		}
	}
	
	// Function: GetLaunchParams
	//		(Internal) Gets any options passed by the Grasshopper daemon (outside of Unity) to the game executable, via command line.
	//		Not currently used.
	string[] GetLaunchParams() {
		return System.Environment.GetCommandLineArgs();
	}
	
	// Function: TriggerSampleStartup
	//		(Internal) Simulates the on-table startup process by generating & "sending" fake player-add & go messages to the <GhopperDaemon>. 
	//		Useful for testing without the Grasshopper daemon (outside of Unity).
	public void TriggerSampleStartup() {
		// generate random# players, then send go

		int numSamplePlayers = Mathf.CeilToInt( Random.value * (maxFakePlayers - 1) ) + 1;
		int i = 0;
		GhopperMessage message = null;
		
		DebugExtras.LogWarning("\"Fake\" Grasshopper startup process has started. Make sure GhopperDaemon.fakeStartupInfo isn't enabled in your final build.");
		
		// generate & send addPlayer messages
		for (i = 0; i < numSamplePlayers; i++) {
			message = GeneratePlayerAddMessage(i);
			OnReceivePlayerEvent(message);
		}
		
		// generate & send Go message
		message = GenerateReadyMessage();
		OnReceiveGameEvent(message);
	}
	
	// Function: GeneratePlayerAddMessage
	//		(Internal) Used by <TriggerSampleStartup>.
	GhopperMessage GeneratePlayerAddMessage(int playerNum) {
		int playerPosition = Mathf.CeilToInt( playerNum * (360 / 6) );
		int playerColor = (int)Mathf.Floor((Random.value * 2 - 1) * int.MaxValue);
		string playerFirstName = "Figment";
		string playerLastName = "#" + playerNum.ToString();
		
		GhopperMessage message = new GhopperMessage();
		ArrayList messageValues = message.values;
		string address = PLAYER_API_ADDRESS + playerNum.ToString() + "/add";
		
		message.address = address;
		messageValues.Add("daemon");
		messageValues.Add(playerPosition);
		messageValues.Add(playerFirstName);
		messageValues.Add(playerLastName);
		messageValues.Add(playerColor);
		
		return message;
	}
	
	// Function: GenerateReadyMessage
	//		(Internal) Used by <TriggerSampleStartup>.
	GhopperMessage GenerateReadyMessage() {
		GhopperMessage message = new GhopperMessage();
		message.address = GAME_API_ADDRESS + "ready";
		message.values.Add("daemon");
		return message;
	}
}
