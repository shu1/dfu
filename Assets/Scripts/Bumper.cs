using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bumper : MonoBehaviour {

	public List<AudioClip> bumperSounds = new List<AudioClip>();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision)
	{
		audio.PlayOneShot(bumperSounds[(int)(Mathf.Floor(Random.value * 0.99f * bumperSounds.Capacity))]);
	}
}
