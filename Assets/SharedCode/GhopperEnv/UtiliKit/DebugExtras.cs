/* DebugExtras.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Extends the Unity Debug class to compile and execute conditionally. Stack traces eat up performance!
 */

// comment out to hide debugs in releases
#define DEBUG
// ----------

#if UNITY_EDITOR
#define DEBUG   // always debug mode in editor
#endif

using UnityEngine;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

public static class DebugExtras 
{
	public static bool debugBuild
	{
		get
		{
			bool debug = false;
#if DEBUG
    		debug = true;
#endif
			return debug;
		}
	}

    [Conditional( "DEBUG" )]
    public static void Assert( bool isTrue, string assertMsg = null )
    {
        if ( !isTrue )
            UnityEngine.Debug.LogError( "Assert failed " + assertMsg ?? "" );
    }
    
    [Conditional( "DEBUG" )]
    public static void Log( object toLog, Object context = null)
    {
        UnityEngine.Debug.Log( toLog + "\n-----------------------", context );
    }
    
    [Conditional( "DEBUG" )]
    public static void LogWarning( object toLog, Object context = null)
    {
        UnityEngine.Debug.LogWarning( toLog + "\n-----------------------", context );
    }
    
    [Conditional( "DEBUG" )]
    public static void LogError( object toLog, Object context = null)
    {
        UnityEngine.Debug.LogError( toLog + "\n-----------------------", context );
    }
}
