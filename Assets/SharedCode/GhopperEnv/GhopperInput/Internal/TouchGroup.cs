/* TouchGroup.cs
 * Copyright Grasshopper 2012
 * ----------------------------
 * A wrapper for a list of touches. Includes some utility methods
 */

using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public class TouchGroup : List<TouchInfo>
{
    public TouchGroup() : base()
    {
    }

    public TouchGroup( TouchInfo touch ) : base()
    {
        Add( touch );
    }

    public TouchGroup( IEnumerable<TouchInfo> touches ) : base( touches )
    {
    }

    public ReadOnlyCollection<TouchInfo> GetAsReadOnly()
    {
        return AsReadOnly();
    }

    /// <summary>
    /// Returns touch with oldest timeDown in group
    /// </summary>
    public TouchInfo GetOldest()
    {
        TouchInfo oldest = null;
        foreach ( var touch in this )
        {
            if ( touch.isDown && ( !oldest || touch.timeDown < oldest.timeDown ) )
                oldest = touch;
        }

        return oldest;
    }

    /// <summary>
    /// Gets the average screen position (pixel space) of all touches in group
    /// </summary>
    public Vector2 GetAvgScreenPos()
    {
        Vector2 totalPos = Vector2.zero;
        int points = 0;
        foreach ( var touch in this )
        {
            if ( touch.isDown )
            {
                totalPos += touch.screenPos;
                points++;
            }
        }

        if ( points == 0 )
            return Vector2.zero;

        return totalPos / points;
    }

    /// <summary>
    /// Gets the average distance that all touches have moved from their respective starts
    /// </summary>
    public Vector2 GetAvgDistFromStart()
    {
        Vector2 totalMovement = Vector2.zero;
        int points = 0;
        foreach ( var touch in this )
        {
            if ( touch.isDown )
            {
                totalMovement += touch.screenPos - touch.startScreenPos;
                points++;
            }
        }

        return totalMovement / points;
    }

    /// <summary>
    /// Returns true if all touches in group are moving (with tolerance of one pixel)
    /// </summary>
    public bool AllMoving()
    {
        foreach ( var touch in this )
        {
            if ( touch.deltaScreenPos.sqrMagnitude > 1f )
                return true;
        }

        return false;
    }

    /// <summary>
    /// Are all touches moving in the same direction
    /// </summary>
    /// <returns><c>true</c>, if moving in same direction, <c>false</c> otherwise.</returns>
    /// <param name="tolerance">Tolerance that touch dot products are allowed to be within (0 - no tolerance. 1 - 90 deg difference allowed</param>
    public bool AreMovingInSameDirection( float tolerance )
    {
        if( Count < 2 )
            return true;
        
        float minDOT = Mathf.Max( 0.1f, 1.0f - tolerance ); 
        
        Vector2 refDir = this[0].screenPos - this[0].startScreenPos;
        refDir.Normalize();
        
        for( int i = 1; i < Count; ++i )
        {
            Vector2 dir = this[i].screenPos - this[i].startScreenPos;
            dir.Normalize();
            
            if( Vector2.Dot( refDir, dir ) < minDOT )
                return false;
        }
        
        return true;
    }


    // TODO : allow for > 2 touches

    /// <summary>
    /// Tests if two touches are moving in opposite directions
    /// </summary>
    /// <returns><c>true</c>, if moving in opposite direction, <c>false</c> otherwise.</returns>
    /// <param name="tolerance">Tolerance that touch dot can be from fully opposite. (0 - no tolerance, 1- up to 90 deg away from opposite) ca</param>
    public bool AreMovingInOppositeDirection( float tolerance )
    {
        if ( Count != 2 )
            return false;

        tolerance = Mathf.Clamp01( tolerance );

        return Vector2.Dot( this[0].deltaScreenPos, this[1].deltaScreenPos ) < -1f + tolerance;
    }
}
