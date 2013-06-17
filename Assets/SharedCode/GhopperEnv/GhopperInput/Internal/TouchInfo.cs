/* TouchInfo.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Holds information about a touch
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchInfo
{
    /// <summary>
    /// Time that touch started
    /// </summary>
    public readonly float timeDown;

    /// <summary>
    /// ID of touch. Stays the same while touch is down
    /// </summary>
    public readonly int touchID;

    /// <summary>
    /// Whether touch started on an object on a GUI camera
    /// </summary>
    public readonly bool guiTouch;

    /// <summary>
    /// Holds the raycaster info that this touch is using (cameras & their layermasks)
    /// </summary>
    public readonly RaycasterInfo raycastInfo;

    /// <summary>
    /// Camera that touch started in
    /// </summary>
    public readonly Camera startCam;

    /// <summary>
    /// Screen position that touch started at
    /// </summary>
    public readonly Vector2 startScreenPos;

    /// <summary>
    /// World position that touch started at (Vector3.zero if didn't start on anything)
    /// </summary>
    public readonly Vector3 startWorldPos;

    /// <summary>
    /// Gesture recognisers that this touch is being used by
    /// </summary>
    // TODO
    public readonly List<TouchGestureRecogniserBase> gestureRecognisers = new List<TouchGestureRecogniserBase>();

    /// <summary>
    /// Whether touch is currently down. This will be true for all touches sent from InputMaster in that frame
    /// </summary>
    public bool isDown { get { return Time.frameCount - lastUpdateFrame < 2; } } // if touch expires mark it as up

    /// <summary>
    /// Current screen position of touch
    /// </summary>
    public Vector2 screenPos { get; private set; }

    /// <summary>
    /// Width of touch (for TUIO only)
    /// </summary>
    public float width { get; private set; }

    /// <summary>
    /// Height of touch (for TUIO only)
    /// </summary>
    public float height { get; private set; }

    /// <summary>
    /// Angle of touch to table (for TUIO only)
    /// </summary>
    public float angle { get; private set; }

    /// <summary>
    /// Ray of touch from current camera. (If not touching anything, screenRay is from the start camera. If no start camera, will be a zero ray)
    /// </summary>
    public Ray screenRay { get; private set; }

    /// <summary>
    /// Hit info from most recent raycast
    /// </summary>
    public RaycastHit hitInfo { get; private set; }

    /// <summary>
    /// Gets the current world position of touch
    /// </summary>
    public Vector3 worldPos { get { return hitInfo.point; } }

    /// <summary>
    /// Object that is currently being touched
    /// </summary>
    public GameObject touching { get { return hitInfo.collider ? hitInfo.collider.gameObject : null; } }

    /// <summary>
    /// Whether touch is over an object right now
    /// </summary>
    public bool isTouchingSomething { get { return hitInfo.collider; } }

    /// <summary>
    /// Time since touch started
    /// </summary>
    public float timeHeld { get { return Time.time - timeDown; } }

    /// <summary>
    /// Change in screen position since last frame
    /// </summary>
    public Vector2 deltaScreenPos { get { return screenPos - lastScreenPos; } }

    /// <summary>
    /// Change in world position since last frame
    /// </summary>
    public Vector3 deltaWorldPos { get { return worldPos - lastWorldPos; } }
       
    readonly List<ITouchable> startedOver;
    Vector2 lastScreenPos;
    Vector3 lastWorldPos;
    int lastUpdateFrame;
    
    public TouchInfo( int touchID, RaycasterInfo raycastInfo, Vector2 screenPos, float width, float height, float angle )
    {
        this.timeDown = Time.time;
        this.touchID = touchID;
        
        this.raycastInfo = raycastInfo;
        this.startCam = UpdateTouch( screenPos, width, height, angle );
        this.guiTouch = startCam && startCam.GetComponent<InputCamera>().guiCam;
        
        this.startScreenPos = screenPos;
        this.startWorldPos = hitInfo.point;

        this.startedOver = new List<ITouchable>();

        GatherTouchables();
    }

    public static implicit operator bool( TouchInfo touch )
    {
        return touch != null;
    }
    
    /// <summary>
    /// Updates the touch based on new pos/width/height info
    /// </summary>
    /// <returns>
    /// The camera the touch hit from (if any).
    /// </returns>
    /// <param name='screenPos'>
    /// Screen position.
    /// </param>
    /// <param name='width'>
    /// Width.
    /// </param>
    /// <param name='height'>
    /// Height.
    /// </param>
    /// <param name='angle'>
    /// Angle.
    /// </param>
    public Camera UpdateTouch( Vector2 screenPos, float width, float height, float angle )
    {
        lastUpdateFrame = Time.frameCount;

        this.lastScreenPos = this.screenPos;
        this.screenPos = screenPos;
        this.width = width;
        this.height = height;
        this.angle = angle;

        this.lastWorldPos = worldPos;

        RaycastHit hit;
        int hitCamID = CastTouch( out hit );
        
        if ( hitCamID >= 0 )
        {
            Camera hitCamera = raycastInfo.cameras[hitCamID];
            screenRay = hitCamera.ScreenPointToRay( screenPos );
            hitInfo = hit;
            
            return hitCamera;
        }
        
        // hit nothing in world, just a screen touch
        screenRay = startCam ? startCam.ScreenPointToRay( screenPos ) : new Ray( screenPos, Vector3.zero );
        hitInfo = new RaycastHit();

        return null;
    }
    
    /// <summary>
    /// Gets the position of the touch on a plane at zDist dist from the camera
    /// </summary>
    /// <returns>
    /// The touch at zDist from cam
    /// </returns>
    /// <param name='zDist'>
    /// Distance from camera along its Z axis.
    /// </param>
    public Vector3 GetTouchAtZDistFromCam (float zDist, Camera cam = null )
    {
        if ( !cam )
            cam = startCam;

        if ( !cam )
        {
            DebugExtras.LogWarning ("Can't get touch position at z-dist from cam when there was no starting camera for touch");
            return Vector3.zero;
        }

        float rayDistFromCam = zDist / Mathf.Cos( Mathf.Deg2Rad * Vector3.Angle ( startCam.transform.forward, screenRay.direction ) );
        return screenRay.GetPoint( rayDistFromCam );
    }

    /// <summary>
    /// Gets the touch on the camera facing plane containing <param name="point">
    /// </summary>
    /// <returns>
    /// The point position
    /// </returns>
    /// <param name='point'>
    /// Point in plane
    /// </param>
    [System.Obsolete( "Use GetTouchOnPlane() instead" )]
    public Vector3 GetTouchOnPlaneContaining( Vector3 point )
    {
        return GetTouchOnPlane( point );
    }
    
    /// <summary>
    /// Gets the touch on a plane containing "point"
    /// </summary>
    /// <returns>
    /// The point position
    /// </returns>
    /// <param name='planePoint'>
    /// Point in plane
    /// </param>
    /// <param name="planeNormal">
    /// Normal in plane. If zero (default), plane faces camera.
    /// </param>
    public Vector3 GetTouchOnPlane( Vector3 planePoint, Vector3 planeNormal = default( Vector3 ) ) 
    {
        if ( planeNormal == Vector3.zero )
        {
            if ( !startCam )
            {
                DebugExtras.LogWarning( "GetTouchOnPlane needs a normal provided, or a startCam to use" );
                return Vector3.zero;
            }

            planeNormal = -startCam.transform.forward;
        }

        float raydist = Vector3.Dot( planePoint - screenRay.origin, planeNormal ) / Vector3.Dot( screenRay.direction, planeNormal );
        return screenRay.GetPoint( raydist );
    }

    /// <summary>
    /// Re-cast a touch onto the specified layer
    /// </summary>
    /// <param name='layer'>
    /// Layer.
    /// </param>
    public int CastTouch( LayerMask layer, out RaycastHit hit )
    {
        return raycastInfo.Raycast( screenPos, out hit, layer );
    }

    /// <summary>
    /// Cast the touch on the already defined raycastInfo
    /// </summary>
    /// <returns>The touch.</returns>
    /// <param name="hit">Hit.</param>
    public int CastTouch( out RaycastHit hit )
    {
        return raycastInfo.Raycast( screenPos, out hit );
    }

    /// <summary>
    /// Return enumerable collection of all ITouchables that touch started over
    /// Access with foreach
    /// </summary>
    public IEnumerable<ITouchable> GetTouchables()
    {
        foreach ( var touchable in startedOver )
            yield return touchable;
    }
    
    void GatherTouchables()
    {
        startedOver.Clear();
        if ( touching )
            startedOver.AddRange( touching.GetComponentsWithInterface<ITouchable>() );
    }

    public override string ToString()
    {
        return string.Format("[Touch {0}: screenPos={1}, worldPos={2}, touching={3}, timeHeld={4}]", touchID, screenPos, worldPos, touching ? touching.name : "null", timeHeld );
    }
}