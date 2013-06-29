using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GoalSound : MonoBehaviour {

	public List<AudioClip> goalSounds = new List<AudioClip>();
	public List<AudioClip> ownGoalSounds = new List<AudioClip>();

	// Use this for initialization
	public void PlayGoalSound(bool isOwnGoal) {
		if (isOwnGoal) {
			audio.PlayOneShot(ownGoalSounds[(int)(Mathf.Floor(Random.value * 0.99f * ownGoalSounds.Capacity))]);	
		}
		else {
			audio.PlayOneShot(goalSounds[(int)(Mathf.Floor(Random.value * 0.99f * goalSounds.Capacity))]);
		}
	}
}
