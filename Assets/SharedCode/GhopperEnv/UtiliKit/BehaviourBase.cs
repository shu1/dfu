/* BehaviourBase.cs
 * Copyright Eddie Cameron 2012 / Grasshopper 2013
 * ----------------------------
 * Replacement for stock MonoBehaviour. To be used for all scripts
 * - Make sure to override (rather than hide with 'new') any unity methods like Start/Update in a subclass, so that any future code here will be executed
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BehaviourBase : MonoBehaviour 
{
	Transform _transform;
	public new Transform transform 
    {
		get 
        {
			if ( !_transform )
				_transform = GetComponent<Transform>();

			return _transform;
		}
	}

    Renderer _renderer;
    public new Renderer renderer
    {
        get
        {
            if ( !_renderer )
                _renderer = GetComponent<Renderer>();

            return _renderer;
        }
    }

    AudioSource _audioSource;
    public new AudioSource audio
    {
        get
        {
            if ( !_audioSource )
                _audioSource = GetComponent<AudioSource>();

            return _audioSource;
        }
    }

    Camera _camera;
    public new Camera camera
    {
        get
        {
            if ( !_camera )
                _camera = GetComponent<Camera>();

            return _camera;
        }
    }
	
	protected virtual void Awake()
	{
		
	}
	
	protected virtual void Start()
	{
		
	}
	
	protected virtual void Update()
	{
		
	}
	
	public T GetComponentWithInterface<T>() where T : class
	{
		return gameObject.GetComponentWithInterface<T>();
	}
	
	public T[] GetComponentsWithInterface<T>() where T : class
	{
		return gameObject.GetComponentsWithInterface<T>();	
	}
}

/// <summary>
/// Base class for all Singletons, T is the actual type 
/// eg: public class MyClass : StaticBehaviour<MyClass> {...}
/// Please note that Coroutines started via string must not be static
/// </summary>
public class StaticBehaviour<T> : BehaviourBase where T : BehaviourBase
{
	static T _instance;
	
	protected static T instance {
		get {
			if ( !_instance )
				UpdateInstance();
			return _instance;
		}
	}
	
	protected override void Awake()
    {
        if ( _instance )
        {
            DebugExtras.LogWarning( "Duplicate instance of " + GetType() + " found. Removing " + name );
            Destroy( this );
            return;
        }

		UpdateInstance();
		
		base.Awake();
	}
	
	static void UpdateInstance()
    {
        _instance = GameObject.FindObjectOfType( typeof( T ) ) as T;
		if ( !_instance )
        {
            DebugExtras.LogWarning( "No object of type : " + typeof( T ) + " found in scene. Creating" );
            _instance = new GameObject( typeof( T ).ToString() ).AddComponent<T>();
        }
	}
}

/// <summary>
/// Base class for a nullable type, so you can use  " if ( myClass ) {...} " to check for null
/// </summary>
public class Nullable
{
    public static implicit operator bool( Nullable me )
    {
        return me != null;
    }
}