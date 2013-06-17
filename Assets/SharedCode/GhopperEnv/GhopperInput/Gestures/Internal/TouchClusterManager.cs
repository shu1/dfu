/* TouchClusterManager.cs
 * Copyright Grasshopper 2012 (adapted with permission from FingerGestures copyright William Ravaine)
 * ----------------------------
 * Creates and manages groups of similar touches (those near in time and position to each other)
 */

using UnityEngine;
using System.Collections.Generic;

public class TouchClusterManager : StaticBehaviour<TouchClusterManager> 
{   
    public float clusterRadius = 0.2f; // spatial grouping, in screen space size
    public float timeTolerance = 0.5f;  // temporal grouping
    
    static Dictionary<int,TouchCluster> clusters = new Dictionary<int, TouchCluster>( new IntComparer() ); // active clusters
	static Dictionary<TouchInfo, TouchCluster> clusteredTouches = new Dictionary<TouchInfo, TouchCluster>();	// cluster that each touch is assigned to
    static int nextClusterId = 1;

    protected override void Start()
    {
        base.Start();

        InputMaster.WorldTouchStarted += OnNewTouch;
        InputMaster.WorldTouchEnded += OnEndTouch;
    }

    void OnNewTouch( TouchInfo touch )
    {
        // try to add finger to existing cluster
        TouchCluster cluster = FindClosestCluster( touch );
        
        // no valid active cluster found for that finger, create a new cluster
        if ( cluster == null )
            cluster = NewCluster();
        
        // add finger to selected cluster
        cluster.touches.Add( touch );
		clusteredTouches.Add ( touch, cluster );
    }

    void OnEndTouch( TouchInfo touch )
    {
        // update active clusters
		TouchCluster cluster;
		if ( clusteredTouches.TryGetValue( touch, out cluster ) )
		{
	        if( cluster.touches.Remove( touch ) )
	        {
	            // retire clusters that no longer have any fingers left
	            if( cluster.touches.Count == 0 )
	            {
	                // remove from active clusters list
	                clusters.Remove( cluster.id );
	            }
	        }

			clusteredTouches.Remove ( touch );
		}
    }
    
    public static IEnumerable<TouchCluster> GetClusters()
    {
        foreach ( var cluster in clusters )
            yield return cluster.Value;
    }
    
    public static TouchCluster GetCluster( int clusterId )
    {
        TouchCluster cluster;
        if ( clusters.TryGetValue( clusterId, out cluster ) )
            return cluster;

        return null;
    }
    
    static TouchCluster NewCluster()
    {
        TouchCluster cluster = new TouchCluster();
        
        // assign a new ID
        cluster.id = nextClusterId++;
        
        // add to active clusters
        clusters.Add( cluster.id, cluster );    // add cluster to active clusters list
        
        return cluster;
    }
    
    // Find closest cluster within radius
    static TouchCluster FindClosestCluster( TouchInfo touch )
    {
        TouchCluster best = null;
        float bestSqrDist = float.MaxValue;
        
        // account for higher pixel density touch screens
        float adjustedClusterRadius = instance.clusterRadius;
        
        foreach( var cluster in clusters )
        {
            float elapsedTime = touch.timeDown - cluster.Value.startTime;

            // temporal grouping criteria
            if( elapsedTime > instance.timeTolerance )
                continue;
            
            Vector2 centroid = cluster.Value.touches.GetAvgScreenPos();

            float sqrDist = Vector2.SqrMagnitude( touch.screenPos - centroid ) * InputMaster.pixelToScreenRatio * InputMaster.pixelToScreenRatio;
            if ( sqrDist < bestSqrDist && sqrDist < ( adjustedClusterRadius * adjustedClusterRadius ) )
            {
                best = cluster.Value;
                bestSqrDist = sqrDist;
            }
        }

        return best;
    }
}

[System.Serializable]
public class TouchCluster
{
    public int id = 0;
    public float startTime = 0;
    public TouchGroup touches = new TouchGroup();

    public TouchCluster()
    {
        startTime = Time.time;
    }
}