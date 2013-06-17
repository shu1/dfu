using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Linq;
using OSC;
using Tuio;

/// <summary>
/// Receives OSC data, then routes incoming messages to registered delegates.
/// Portions of this were previously in TuioTracker.
/// </summary>
namespace OSC
{
	// delegates for receiving OSC bundles & messages
	public delegate void OSCBundleDelegate(OSCBundle bundle);
	public delegate void OSCMessageDelegate(OSCMessage message);
	
	// sets up UDP receiver and routes incoming OSC messages to registered callbacks
	public class OSCReceiver
	{
		UdpClient udpReceiver;
		TuioConfiguration config;
	    Thread thr;
	    bool isrunning;
		
	    OSCBundleDelegate allBundleHandler = null; // called for every incoming bundle (if we wish to revert to)
		OSCMessageDelegate allMessageHandler = allMessageHandlerDefault; // called for every incoming message
	    Dictionary<string, List<OSCMessageDelegate>> addressListeners = new Dictionary<string, List<OSCMessageDelegate>>(); // delegates are called for matching addresses
		
		// Moved out from TuioTracker to accompany setup.
		// Probably vestigial. Remove? (Seems kinda silly, as it just dictates the port number, and requires 'using Tuio'.)
	    public void ConfigureFramework( TuioConfiguration config )
	    {
	        this.config = config as TuioConfiguration;
	    }
		
		// registers an OSC Message delegate for a given address (or pattern with wildcards, in future)
	    public void AddCallback( string pattern, OSCMessageDelegate handler )
	    {
			List<OSCMessageDelegate> existingHandlers = null;
	    	if ( addressListeners.ContainsKey( pattern ) ) {
			 	existingHandlers = addressListeners[pattern];
			} else {
				existingHandlers = new List<OSCMessageDelegate>();
	        	addressListeners.Add( pattern, existingHandlers );
			}
			
			if ( !existingHandlers.Contains( handler ) ) {
		        existingHandlers.Add( handler );
			}
	    }
		
		// unregisters an OSC Message delegate for a given address (or pattern with wildcards, in future)
	    public void RemoveCallback( string pattern, OSCMessageDelegate handler )
	    {
	        List<OSCMessageDelegate> existingHandlers = addressListeners[pattern];
	        if (existingHandlers != null) {
	        	existingHandlers.Remove(handler);
	        }
	    }
		
		// registers an OSC bundle delegate to be called every time a bundle is received
		public void SetAllBundleHandler( OSCBundleDelegate handler )
		{
			allBundleHandler = handler;
		}
		
		// registers an OSC bundle delegate to be called every time a bundle is received
		public void SetAllMessageHandler( OSCMessageDelegate handler )
		{
			allMessageHandler = handler;
		}
		
		public void Start()
	    {
	        if (!isrunning)
	        {
        		IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, this.config.Port);
	            this.udpReceiver = new UdpClient(endpoint);
	
	            this.isrunning = true;
	            this.thr = new Thread (new ThreadStart( this.receive ) );
	            this.thr.Start();
				DebugExtras.Log("Started udp thread");
	        }
	    }
		
		public void Stop()
	    {
	        if (isrunning)
	        {
	            isrunning = false;
	            close();
	        }
	    }
		
		void close()
	    {
	        try
	        {
	            //Might throw an exception which is meaningless when we are shutting down
	            this.udpReceiver.Close();
	        }
	        catch
	        {
	
	        }
	    }

	    void receive()
	    {
	        try
	        {
	            receiveData();
	        }
	        catch
	        {				
	        }
	        finally
	        {
	            // Try to stop cleanly on termination of the blocking receivedata function
	            this.Stop();
	        }
	    }
		
		void receiveData()
	    {
	        while (isrunning)
	        {
				IPEndPoint ip = null;
	
	            byte[] buffer = this.udpReceiver.Receive(ref ip);
				
#if UNITY_EDITOR
                if ( !fromAllowedHost( ip.Address ) )
                    continue;
#endif

				switch ( (char)buffer[0] ) 
				{
					case '/':
						receiveMessageData(buffer);
						break;
					case '#':
						receiveBundleData(buffer);
						break;
				}
	        }
	    }
		
		void receiveMessageData( byte[] data ) {
			OSCMessage message = OSCPacket.Unpack( data ) as OSCMessage;
	
            if ( message != null )
            {
//				DebugExtras.Log("got a message.");
				routeMessage( message ) ;
            }
		}
		
		void receiveBundleData( byte[] data ) 
        {
			OSCBundle bundle = OSCPacket.Unpack( data ) as OSCBundle;
	
            if ( bundle != null )
            {
//				DebugExtras.Log("got a bundle.");
				routeMessages( bundle ) ;
				if ( allBundleHandler != null ) 
				{
            		allBundleHandler( bundle );
            	}
            }
		}
		
		void routeMessages( OSCBundle bundle ) 
		{
			List<OSCMessageDelegate> handlers = null;
			
			foreach (OSCMessage msg in bundle.Values) {
//				DebugExtras.Log(msg.ToString());

                if ( GhopperDaemon.detailedDebug )
                    DebugExtras.Log( "Received OSC message: " + msg.ToString() );

				if ( addressListeners.ContainsKey( msg.Address ) ) 
                {
					handlers = addressListeners[msg.Address];
					foreach ( OSCMessageDelegate handler in handlers ) 
                    {
						handler( msg );
					}
				}
				allMessageHandler( msg );
			}
		}
		
		void routeMessage( OSCMessage msg ) {
			List<OSCMessageDelegate> handlers = null;
			
//			DebugExtras.Log(msg.ToString());
			if ( addressListeners.ContainsKey( msg.Address ) ) {
				handlers = addressListeners[msg.Address];
				foreach ( OSCMessageDelegate handler in handlers ) {
					handler( msg );
				}
			}
			
			allMessageHandler( msg );
		}

        bool fromAllowedHost( IPAddress hostIP )
        {
            return hostIP.Equals( IPAddress.Loopback ) || hostIP.Equals( InputMaster.allowedTuioSender ) || InputMaster.allowedTuioSender.Equals( IPAddress.Any );
        }
		
		static void allMessageHandlerDefault( OSCMessage message ) {
		}

	}
}