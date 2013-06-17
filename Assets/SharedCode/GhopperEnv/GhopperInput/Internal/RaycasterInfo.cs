/* RaycasterInfo.cs
 * Copyright Grasshopper 2013 (adapted with permission from FingerGestures' RaycastCamera.cs, copyright William Ravaine)
 * ----------------------------
 * Defines and implements a 'group' raycast across multiple cameras
 */

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RaycasterInfo
{
    /// <summary>
    /// List of cameras to use for each raycast. Each camera will be considered in the order specified in this list,
    /// and the Raycast method will continue until a hit is detected.
    /// </summary>
    public Camera[] cameras;
    
    /// <summary>
    /// Layers to use when raycasting
    /// </summary>
    public LayerMask[] layerMasks;
    
    /// <summary>
    /// Thickness of the ray. 
    /// Setting rayThickness to 0 will use a normal Physics.Raycast()
    /// Setting rayThickness to > 0 will use Physics.SphereCast() of radius equal to half rayThickness
    ///  ** IMPORTANT NOTE ** According to Unity's documentation, Physics.SphereCast() doesn't work on colliders setup as triggers
    /// </summary>
    public float rayThickness = 0;
    
    /// <summary>
    /// Property used while in the editor only. 
    /// Toggles the visualization of the raycasts as red lines for misses, and green lines for hits (visible in scene view only)
    /// </summary>
    public bool visualizeRaycasts = false;
    
    public RaycasterInfo()
    {
        cameras = new Camera[0];
        layerMasks = new LayerMask[0];
    }
    
    public RaycasterInfo( InputCamera[] camerasInfo )
    {
		cameras = new Camera[camerasInfo.Length];
		layerMasks = new LayerMask[camerasInfo.Length];
		for( int i = 0; i < camerasInfo.Length; i++ )
		{
			cameras[i] = camerasInfo[i].camera;
			layerMasks[i] = camerasInfo[i].inputRaycastLayers;
		}
    }

    /// <summary>
    /// Raycast the specified screenPos and layerMask.
    /// Returns index of camera that was hit, or -1 if none
    /// </summary>
    /// <param name="screenPos">Screen position. (Pixel space)</param>
    /// <param name="hit">Hit.</param>
    /// <param name="layerMask">Layer mask.</param>
	public int Raycast( Vector2 screenPos, out RaycastHit hit, LayerMask layerMask )
	{
		for ( int i = 0; i < cameras.Length; i++ )
		{
			if ( Raycast( cameras[i], screenPos, out hit, layerMask ) )
			    return i;
	    }
			    
	    hit = new RaycastHit();
	    return -1;	    
    }
			    
    /// <summary>
    /// Raycast the specified screenPos, using the layermasks already defined in the raycaster
    /// Returns index of camera that hit was under, or -1 if none
    /// </summary>
    /// <param name="screenPos">Screen position.</param>
    /// <param name="hit">Hit.</param>
	public int Raycast( Vector2 screenPos, out RaycastHit hit )
    {
        for ( int i = 0; i < cameras.Length; i++ )
        {
            if ( Raycast( cameras[i], screenPos, out hit, layerMasks[i] ) )
                return i;
        }

        hit = new RaycastHit();
        return -1;
    }
    
    bool Raycast( Camera cam, Vector2 screenPos, out RaycastHit hit, LayerMask withLayerMask )
    {
		if ( !cam )
        {
            hit = new RaycastHit();
			return false;
        }

        Ray ray = cam.ScreenPointToRay( screenPos );
        bool didHit = false;
        
        if( rayThickness > 0 )
            didHit = Physics.SphereCast( ray, 0.5f * rayThickness, out hit, Mathf.Infinity, withLayerMask );
        else
            didHit = Physics.Raycast( ray, out hit, Mathf.Infinity, withLayerMask );
        
        // vizualise ray
#if UNITY_EDITOR
        if( visualizeRaycasts )
        {
            if( didHit )
                Debug.DrawLine( ray.origin, hit.point, Color.green, 0.5f );
            else
                Debug.DrawLine( ray.origin, ray.origin + ray.direction * 9999.0f, Color.red, 0.5f );
        }
#endif
        
        return didHit;
    }
}
