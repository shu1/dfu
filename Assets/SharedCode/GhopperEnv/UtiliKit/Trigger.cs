/* ExtensionMethods.cs
 * Copyright Eddie Cameron 2011
 * ----------------------------
 * Drop on a trigger collider to have it send events when specified layers enter/stay/exit.
 * Useful for triggers on child objects and for making dealing with layer mask problems far easier
 */

using UnityEngine;
using System.Collections;
using System;

public class Trigger : MonoBehaviour 
{
	public LayerMask layers; // Much faster, overrides tag string if set
	
	public string tagToCheck = "None";
	private bool checkNone;
	private bool checkString;
	
	public event Action<Transform> triggerEntered;
	public event Action<Transform> triggerStay;
	public event Action<Transform> triggerLeft; 
	
	
	void Start()
	{
		if ( ~layers.value == 0 )
		{
			checkString = true;
			if ( tagToCheck == "None" )
				checkNone = true;
		}
	}
	
	void OnTriggerEnter( Collider other )
	{	
		if ( checkNone || ( checkString && other.CompareTag( tagToCheck ) ) || ( ( 1 << other.gameObject.layer ) & layers.value ) != 0 )
		{
			if ( triggerEntered != null )
			{
				triggerEntered( other.transform );
			}
		}
	}
	
	void OnTriggerStay( Collider other )
	{
		// only for layers
		if ( checkNone || ( ( 1 << other.gameObject.layer ) & layers.value ) != 0 )
			if ( triggerStay != null ) triggerStay( other.transform );
	}
	
	void OnTriggerExit( Collider other )
	{
		if ( checkNone || ( checkString && other.CompareTag( tagToCheck ) ) || ( ( 1 << other.gameObject.layer ) & layers.value ) != 0 )
		{
			if ( triggerLeft != null )
				triggerLeft( other.transform );
		}
	}
}
