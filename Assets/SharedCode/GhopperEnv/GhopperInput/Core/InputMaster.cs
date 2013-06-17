/* InputMaster.cs
 * Copyright Eddie Cameron 2012 / Grasshopper 2013
 * ----------------------------
 * Gathers and distributes Input info.
 * Sets up an InputProvider, and gathers any input info from it each frame.
 * - Provides access for direct access to touches by index
 * - Sends events when touches start and end, and an update each frame between
 * - Raycasts under each touch, and will call StartTouch, UpdateTouch and EndTouch on any 
 * ITouchables on the object that the touch starts over
 * 
 * Touches keep the same ID while down
 * Start and End events and calls are guaranteed to happen. Update is guaranteed to happen each frame while touch is down
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InputMaster : StaticBehaviour<InputMaster> 
{
    public InputProvider mouseInputProvider;
    public InputProvider mobileTouchInputProvider;
    public InputProvider tuioInputProvider;
    public bool seperateGUIInput = true;    // if true, touches on InputCameras marked as 'GUI' won't raise WorldTouch events

    public string editorTuioIpAddress = ""; // fill out to receive TUIO input in editor (from this address only). Set to 0 or blank to take input from anywhere
    public bool useTuioInEditor = false;    // set to true to test TUIO input in editor (try TuioPad for iOS)
    public bool useTuioInBuild = true;          
    public bool detailedDebug = false;      // get a whole bunch of annoying messages about touches

    // send events if touch didn't start over touchable object (straight onto world)
	public static event Action<TouchInfo> WorldTouchStarted;
	public static event Action<TouchInfo> WorldTouchUpdate;
	public static event Action<TouchInfo> WorldTouchEnded;

    public static Camera[] guiCams { get { return _guiCams.ToArray(); } }
    static List<Camera> _guiCams = new List<Camera>();
    public static Camera[] worldCams { get { return _worldCams.ToArray( ); } }	// first world cam
    static List<Camera> _worldCams = new List<Camera>();

    public static System.Net.IPAddress allowedTuioSender { get; private set; }

	public static float pixelToScreenRatio { get; private set; }
	
    /// <summary>
    /// Gets enumerable collection of all current touches (use with foreach)
    /// </summary>
    public static IEnumerable<TouchInfo> touches    // access with "foreach ( TouchInfo touch in InputMaster.touches ) {...}"
    {
        get
        {
            foreach ( var touch in currentTouches )
                yield return touch.Value;
        }
    }

	static InputProvider inputProvider;
	static Dictionary<int, TouchInfo> currentTouches = new Dictionary<int, TouchInfo>( new IntComparer() );
    static RaycasterInfo inputRaycaster;    // looks for cameras with InputCamera attached
    static TouchDebugPoint touchDebugPointPrefab;
    
    #region Accessors
    /// <summary>
    /// Gets current touches as a TouchGroup
    /// </summary>
    public static TouchGroup GetTouches()
    {
        return new TouchGroup( currentTouches.Values );
    }

    /// <summary>
    /// Get a specific touch with an ID. Returns null if no touches with given ID
    /// </summary>
    /// <returns>The touch.</returns>
    public static TouchInfo GetTouch( int touchID )
    {
        TouchInfo touch;
        if ( currentTouches.TryGetValue( touchID, out touch ) )
            return touch;
        
        return null;
    }
    
    /// <summary>
    /// Converts a screenPos (in pixels) to normalised screen space
    /// </summary>
    /// <returns>The normalised screenPos.</returns>
    /// <param name="screenPos">Screen position in pixels.</param>
    public static Vector2 PixelToScreenPos( Vector2 pixelPos )
    {
        pixelPos.x /= Screen.width;
        pixelPos.y /= Screen.height;
        return pixelPos;
    }
    #endregion

	void OnLevelWasLoaded()
	{
        _guiCams.Clear();
        _worldCams.Clear();

		var cameras = new LinkedList<InputCamera>();
		foreach ( var newCamera in FindObjectsOfType( typeof( InputCamera ) ) as InputCamera[] )
		{
            Camera camComponent = newCamera.camera;

            if ( newCamera.guiCam )
                _guiCams.Add( camComponent );
            else
                _worldCams.Add( camComponent );

			bool added = false;
			LinkedListNode<InputCamera> camNode = cameras.First;
			while ( camNode != null && ( camNode = camNode.Next ) != null )
			{
				if ( camComponent.depth > camNode.Value.camera.depth )
				{
					cameras.AddBefore( camNode, newCamera );
					added = true;
					break;
				}
			}

			if ( !added )
				cameras.AddLast( newCamera );
		}

		InputCamera[] raycastCameras = new InputCamera[cameras.Count];
		cameras.CopyTo ( raycastCameras, 0 );

        DebugExtras.Log( "Input initialised with " + raycastCameras.Length + " cameras" );
		inputRaycaster = new RaycasterInfo( raycastCameras );
	}
	
	protected override void Awake()
    {
        base.Awake( );

        System.Net.IPAddress _allowedSender;
        if ( !System.Net.IPAddress.TryParse( editorTuioIpAddress, out _allowedSender ) )
        {
#if UNITY_EDITOR
            DebugExtras.LogWarning( "No tuio sender specified, all tuio messages will be interpreted" );
#endif
            allowedTuioSender = System.Net.IPAddress.Any;
        }
        else
            allowedTuioSender = _allowedSender;

		pixelToScreenRatio = 1f / Screen.height;

        if ( detailedDebug )
            touchDebugPointPrefab = Resources.Load( "TouchDebugPoint", typeof( TouchDebugPoint ) ) as TouchDebugPoint;

		DontDestroyOnLoad( gameObject );
        OnLevelWasLoaded();

        SetUpInputProvider();
	}

	protected override void Update()
	{
		base.Update();

        inputProvider.FetchFrameInputs();

		// go thru all possible inputs, if new, udpate as began, if changed, update as Moved, if was present in currentTouches but touch is no longer down, update as Ended
		for ( int touchID = 0; touchID < inputProvider.maxSupportedInputs; touchID++ )
		{
			InputProvider.InputInfo inputInfo = inputProvider.GetInputState( touchID );

			if ( inputInfo.isDown )
			{
				if ( currentTouches.ContainsKey( touchID ) )
					UpdateTouch( touchID, inputInfo.screenPos, inputInfo.width, inputInfo.height, inputInfo.angle, TouchPhase.Moved );
				else
					UpdateTouch( touchID, inputInfo.screenPos, inputInfo.width, inputInfo.height, inputInfo.angle, TouchPhase.Began );
			}
			else if ( currentTouches.ContainsKey( touchID ) )
            {
                TouchInfo lastTouch = currentTouches[touchID];
				UpdateTouch( touchID, lastTouch.screenPos, lastTouch.width, lastTouch.height, lastTouch.angle, TouchPhase.Ended );
            }
		}
	}

	/// <summary>
	/// Updates a touch of known phase (like from iOS or mouse)
	/// </summary>
	/// <param name='touchID'>
	/// Touch ID
	/// </param>
	/// <param name='touchPos'>
	/// Touch screen position.
	/// </param>
	/// <param name='phase'>
	/// Phase ( for each ID, need to be in order Begin -> Moved/Stationary -> Ended/Cancelled )
	/// </param>
	static void UpdateTouch( int touchID, Vector2 touchPos, float width, float height, float angle, TouchPhase phase )
	{
        TouchInfo touch;
		if ( phase == TouchPhase.Began )
			touch = new TouchInfo( touchID, inputRaycaster, touchPos, width, height, angle );
		else if ( currentTouches.TryGetValue( touchID, out touch ) )
			touch.UpdateTouch( touchPos, width, height, angle );
		else
		{
            DebugExtras.LogWarning( "Touch with ID " + touchID + " updated with phase " + phase + " before phase 'Begin'. You should send a touch with phase 'Begin' before any other phases with the same ID" );

			if ( phase == TouchPhase.Canceled || phase == TouchPhase.Ended )
				return;	// skip touch if only Ended or Cancelled
           
            phase = TouchPhase.Began;
			touch = new TouchInfo( touchID, inputRaycaster, touchPos, width, height, angle );
		}

		// send touch start to any objects under new touch
		if ( phase == TouchPhase.Began )
			BeginTouch( touch );
		else if ( phase == TouchPhase.Moved || phase == TouchPhase.Stationary )
			MoveTouch( touch );
		else if ( phase == TouchPhase.Ended || phase == TouchPhase.Canceled )
			EndTouch( touch );
	}

	static void BeginTouch( TouchInfo touchInfo )
	{
        if ( instance.detailedDebug )
        {
            DebugExtras.Log( "Started: " + touchInfo );
            if ( touchDebugPointPrefab )
                ( (TouchDebugPoint)Instantiate( touchDebugPointPrefab, Vector3.zero, Quaternion.identity ) ).Init( touchInfo );
        }

		// make sure touch index isn't already determined in list
		if ( currentTouches.ContainsKey( touchInfo.touchID ) )
		{
            DebugExtras.LogWarning( "Touch " + touchInfo.touchID + " began before last touch of same ID Ended or Cancelled" );
			
			EndTouch( currentTouches[touchInfo.touchID] );	// End the previous touch
		}
		
		currentTouches.Add( touchInfo.touchID, touchInfo );

		// send touch start to touchables
		foreach ( var touchable in touchInfo.GetTouchables() )
        {
            try
            {
			    touchable.StartTouch( touchInfo );
            }
            catch ( Exception e )
            {
                DebugExtras.LogError( e );
            }
        }

		if ( !instance.seperateGUIInput || !touchInfo.guiTouch )	// only non-gui touches send WorldTouch... events
            SafeEvents.SafeRaise( WorldTouchStarted, touchInfo );
	}

	static void MoveTouch( TouchInfo touchInfo )
    {
		// update touched objects
		foreach ( var touched in touchInfo.GetTouchables() )
        {
            try
            {
			    touched.UpdateTouch ( touchInfo );
            }
            catch ( Exception e )
            {
                DebugExtras.LogError( e );
            }
        }
		
		if ( !instance.seperateGUIInput || !touchInfo.guiTouch )
			SafeEvents.SafeRaise( WorldTouchUpdate, touchInfo );
	}

	static void EndTouch( TouchInfo touchInfo )
	{
        if ( instance.detailedDebug )
            DebugExtras.Log( "Ended: " + touchInfo );

		// stop tracking, send end touch
		foreach ( var touched in touchInfo.GetTouchables() )
        {
            try
            {
			    touched.EndTouch( touchInfo );
            }
            catch ( Exception e )
            {
                DebugExtras.LogError( e );
            }
        }
		
		if ( !instance.seperateGUIInput || !touchInfo.guiTouch )
			SafeEvents.SafeRaise( WorldTouchEnded, touchInfo );

		currentTouches.Remove( touchInfo.touchID );
	}

	static void SetUpInputProvider()
	{
		InputProvider providerToUse = null;
#if UNITY_EDITOR
		if ( instance.useTuioInEditor ) 
			providerToUse = instance.tuioInputProvider;
		else 
			providerToUse = instance.mouseInputProvider;
#elif UNITY_IPHONE || UNITY_ANDROID
        providerToUse = instance.mobileTouchInputProvider;
#else
		if ( instance.useTuioInBuild ) 
			providerToUse = instance.tuioInputProvider;
		else 
			providerToUse = instance.mouseInputProvider;
#endif

		inputProvider = (InputProvider)Instantiate( providerToUse, Vector3.zero, Quaternion.identity );
        inputProvider.transform.parent = instance.transform;
	}
}

/// <summary>
/// ITouchable methods will be called on each implementing class on objects taht are touched
/// To touch an object, it must have a collider on a layer defined under an InputCamera
/// </summary>
public interface ITouchable
{
	void StartTouch( TouchInfo info );
	void UpdateTouch( TouchInfo info );
	void EndTouch( TouchInfo info );
}
