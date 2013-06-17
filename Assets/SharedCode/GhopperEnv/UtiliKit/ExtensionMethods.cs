/* ExtensionMethods.cs
 * Copyright Eddie Cameron 2012 / Grasshopper 2013
 * ----------------------------
 * Holds general useful extension methods in one handy place
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class ExtensionMethods 
{
	#region Transform
    /// <summary>
    /// Sets the X position.
    /// </summary>
    /// <param name="transform">Transform.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="space">Space, (world or self) Default is self</param>
	public static void SetXPos( this Transform transform, float x, Space space = Space.Self )
	{
		if ( space == Space.Self )
		{
			Vector3 newPos = transform.localPosition;
			newPos.x = x;
			transform.localPosition = newPos;
		}
		else
		{
			Vector3 newPos = transform.position;
			newPos.x = x;
			transform.position = newPos;
		}
	}
	
    /// <summary>
    /// Sets the Y position.
    /// </summary>
    /// <param name="transform">Transform.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="space">Space, (world or self) Default is self</param>
    public static void SetYPos( this Transform transform, float y, Space space = Space.Self )
	{
		if ( space == Space.Self )
		{
			Vector3 newPos = transform.localPosition;
			newPos.y = y;
			transform.localPosition = newPos;
		}
		else
		{
			Vector3 newPos = transform.position;
			newPos.y = y;
			transform.position = newPos;
		}
	}
	
    /// <summary>
    /// Sets the Z position.
    /// </summary>
    /// <param name="transform">Transform.</param>
    /// <param name="z">The z coordinate.</param>
    /// <param name="space">Space, (world or self) Default is self</param>
    public static void SetZPos( this Transform transform, float z, Space space = Space.Self )
	{
		if ( space == Space.Self )
		{
			Vector3 newPos = transform.localPosition;
			newPos.z = z;
			transform.localPosition = newPos;
		}
		else
		{
			Vector3 newPos = transform.position;
			newPos.z = z;
			transform.position = newPos;
		}
	}

    public static Vector3 WorldToLocalPosition( this Transform transform, Vector3 worldPos, bool affectedByScale = true )
    {
        if ( affectedByScale )
            return transform.InverseTransformPoint( worldPos );
        else
            return transform.InverseTransformDirection( worldPos );
    }

    public static Vector3 LocalToWorldPosition( this Transform transform, Vector3 localPos, bool affectedByScale = true )
    {
        if ( affectedByScale )
            return transform.TransformPoint( localPos );
        else
            return transform.TransformDirection( localPos );
    }
	#endregion
	
	#region Collections
	public static bool AddUnique<T>( this ICollection<T> collection, T itemToTryAdd )
	{
		if ( collection.Contains( itemToTryAdd ) )
			return false;
		
		collection.Add( itemToTryAdd );
        return true;
	}

    public static bool TryAdd<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey keyToAdd, TValue valueToAdd )
    {
        return dictionary.AddUnique( new KeyValuePair<TKey, TValue>( keyToAdd, valueToAdd ) );
    }

    public static IEnumerable<T> EnumerateRemoveNull<T>( this LinkedList<T> linkedList )
    {
        var node = linkedList.First;
        while ( node != null )
        {
            var next = node.Next;
            if ( node.Value == null )
                linkedList.Remove( node );
            else
                yield return node.Value;
            
            node = next;
        }
    }

    public static T RandomElement<T>( this IList<T> collection )
    {
        return collection[UnityEngine.Random.Range( 0, collection.Count )];
    }
	#endregion
	
	#region GameObject
	public static T GetComponentWithInterface<T>( this GameObject gameObject ) where T : class
	{
		foreach( Component c in gameObject.GetComponents<Component>() )
		{
			T t = c as T;
			if ( t != null )
				return t;
		}
		return null;
	}
	
	public static T[] GetComponentsWithInterface<T>( this GameObject gameObject ) where T : class
	{
		List<T> interfaceComponents = new List<T>();
		foreach( Component c in gameObject.GetComponents<Component>() )
		{
			T t = c as T;
			if ( t != null )
				interfaceComponents.Add( t );
		}
		return interfaceComponents.ToArray();
	}
	#endregion
}

public delegate void SafeEvent( object sender, System.EventArgs args );

public static class SafeEvents
{
    public static void SafeRaise( Action safeEvent )
    {
        Action eventCopy;
        lock ( safeEvent )
        {
            eventCopy = safeEvent;
        }

        if ( eventCopy != null )
        {
            foreach ( Action subscriber in eventCopy.GetInvocationList() )
            {
                try
                {
                    subscriber();
                }
                catch ( System.Exception e )
                {
                    DebugExtras.LogError( e );
                }
            }
        }
    }
    
    public static void SafeRaise<T>( Action<T> safeEvent, T arg )
    {
        Action<T> eventCopy;
        
        if ( safeEvent != null )
        {
            lock ( safeEvent )
            {
                eventCopy = safeEvent;
            }

            foreach ( Action<T> subscriber in eventCopy.GetInvocationList() )
            {
                try
                {
                    subscriber( arg );
                }
                catch ( System.Exception e )
                {
                    DebugExtras.LogError( e );
                }
            }
        }
    }

    public static void SafeRaise<T1, T2>( Action<T1, T2> safeEvent, T1 arg1, T2 arg2 )
    {
        Action<T1, T2> eventCopy;
        lock ( safeEvent )
        {
            eventCopy = safeEvent;
        }
        
        if ( eventCopy != null )
        {
            foreach ( Action<T1, T2> subscriber in eventCopy.GetInvocationList() )
            {
                try
                {
                    subscriber( arg1, arg2 );
                }
                catch ( System.Exception e )
                {
                    DebugExtras.LogError( e );
                }
            }
        }
    }

    public static void SafeRaise<T1, T2, T3>( Action<T1, T2, T3> safeEvent, T1 arg1, T2 arg2, T3 arg3 )
    {
        Action<T1, T2, T3> eventCopy;
        lock ( safeEvent )
        {
            eventCopy = safeEvent;
        }
        
        if ( eventCopy != null )
        {
            foreach ( Action<T1, T2, T3> subscriber in eventCopy.GetInvocationList() )
            {
                try
                {
                    subscriber( arg1, arg2, arg3 );
                }
                catch ( System.Exception e )
                {
                    DebugExtras.LogError( e );
                }
            }
        }
    }
    
    public static void SafeRaise<T1, T2, T3, T4>( Action<T1, T2, T3, T4> safeEvent, T1 arg1, T2 arg2, T3 arg3, T4 arg4 )
    {
        Action<T1, T2, T3, T4> eventCopy;
        lock ( safeEvent )
        {
            eventCopy = safeEvent;
        }
        
        if ( eventCopy != null )
        {
            foreach ( Action<T1, T2, T3, T4> subscriber in eventCopy.GetInvocationList() )
            {
                try
                {
                    subscriber( arg1, arg2, arg3, arg4 );
                }
                catch ( System.Exception e )
                {
                    DebugExtras.LogError( e );
                }
            }
        }
    }
}