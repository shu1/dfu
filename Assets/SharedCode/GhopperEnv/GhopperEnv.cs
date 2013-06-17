using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OSC;
using Tuio;

public abstract class GhopperEnv : StaticBehaviour<GhopperEnv> 
{
    public static GhopperDaemon daemon { get; private set; }
    public static GhopperCms cms { get; private set; }
    public static GhopperBezel bezel { get; private set; }
    
//  static OSCSender oscSender = null;
        
    static List<GhopperPlayer> players = new List<GhopperPlayer>();
    
    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad( gameObject );
    }

    void OnApplicationQuit()
    {
        End();
    }

    public static void Init() 
    {
        // starts ghopper init & tells daemon that game is now running.
        GhopperDaemon attachedDaemon = instance.GetComponent<GhopperDaemon>();
        if (attachedDaemon == null) {
            DebugExtras.LogError("GhopperDaemon must be attached to the same game object as your GhopperEnv subclass. (this should change in future revisions)");
        } else {
            daemon = attachedDaemon;
            daemon.Init();
            daemon.SendGameInit();
        }
        
        GhopperCms attachedCms = instance.GetComponent<GhopperCms>();
        if (attachedCms == null) {
            DebugExtras.LogError("GhopperCms must be attached to the same game object as your GhopperEnv subclass. (this should change in future revisions)");
        } else {
            cms = attachedCms;
        }
        
        GhopperBezel attachedBezel = instance.GetComponent<GhopperBezel>();
        if (attachedBezel == null) {
            DebugExtras.LogError("GhopperBezel must be attached to the same game object as your GhopperEnv subclass. (this should change in future revisions)");
        } else {
            bezel = attachedBezel;
        }
    }
    
    public static void Show() {
        // tells loader to show the game after you're done setting up
        daemon.SendShowGame();
    }
    
    public static void End() 
    {
        if ( daemon != null )
            daemon.SendGameEnd();
    }
    
    public static GhopperPlayer[] GetPlayers() {
        return players.ToArray();
    }

    public static GhopperPlayer GetPlayer( int playerID )
    {
        if ( playerID < 0 || playerID >= players.Count )
        {
            DebugExtras.LogError( "Requested a nonexistant player: " + playerID );
            return null;
        }

        return players[playerID];
    }

    public static void OnAddPlayer(int pid, int pangle, string pfirstname, string plastname, int pColor) {
        GhopperPlayer newPlayer = new GhopperPlayer( pid, pangle, pfirstname, plastname, ConversionUtil.IntToColor(pColor) );
        players.Add(newPlayer);
    }
    
    public static void OnRemovePlayer (int pid)
        {
                foreach (GhopperPlayer player in players) {
                        if (player.GetId () == pid) {
                                players.Remove (player);
                                break;
                        }
                }
        }
    
    public abstract void OnReady();
        // MUST be overrided -- that's how the daemon tells your game that it's ready to start. (and your game should respond via GhopperEnv.Show())
    
    public virtual void OnPause() {
        // should be overrided
    }
    
    public virtual void OnUnpause() {
        // should be overrided
    }
}