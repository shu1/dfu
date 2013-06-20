using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoalSound : MonoBehaviour {

	public List<AudioClip> goalSounds = new List<AudioClip>();

	// Use this for initialization
	public void PlayGoalSound() {
		audio.PlayOneShot(goalSounds[(int)(Mathf.Floor(Random.value * 0.99f * goalSounds.Capacity))]);
	}
}
