/* InputCamera.cs
 * Copyright Grasshopper 2013
 * ----------------------------
 * Declares that a camera should be used for raycasting input
 * Each touch is raycasted on all InputCameras, in depth order, until a collider is hit (ie, a touch on a GUI cam won't raycast on camera underneath)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputCamera : BehaviourBase 
{
    /// <summary>
    /// If set to true, touches that hit objects under this camera will be treated as 'GUI' touches, and won't raise WorldTouch events
    /// </summary>
    public bool guiCam;

    /// <summary>
    /// Layers that touches on this camera can hit
    /// </summary>
    public LayerMask inputRaycastLayers = ~0;
}
