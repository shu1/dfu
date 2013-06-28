using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ball : MonoBehaviour {
	GameObject goalRing;				// Gameobject containing the goal ring

	Game gameScript;					// Handle to game.cs (Game Manager) script
	Clock clockScript;					// Handle to clock.cs (Game Clock) script

	const float maxCurveMag = 2;		// Maximum curve magnitude that the ball is allowed to have
	const float maxSpeed = 15;			// Maximum speed that the ball is allowed to have

	int lastTouched;					// The player index of the last player to have touched the ball
	float currSpeed;					// Last imparted bookkept speed to ball
	float curveMag;						// Current amount of curve on the ball. Positive curves
	float spinFactor;
	float ballRadius;					// Ball radius to compute rolling
	Vector3 rollVelocity;				// Ball rolling to show velocity and direction

	
	
	public List<AudioClip> shotSounds = new List<AudioClip>();

	void Start () {

		// Find goal ring
		goalRing = GameObject.Find("goalRing");

		// Grab script handles
		gameScript = GameObject.Find("GhopperEnv").GetComponent<Game>();
		clockScript = GameObject.Find("Clock").GetComponent<Clock>();

		switch(gameScript.numPlayers) {
			case 2:
				spinFactor = 0.1f;
				break;
			case 3:
				spinFactor = 0.125f;
				break;
			case 4:
				spinFactor = 0.15f;
				break;
			case 5:
				spinFactor = 0.175f;
				break;
			case 6:
				spinFactor = 0.2f;
				break;
		}
		
		// Initialize internal variables
		lastTouched = -1;
		currSpeed   = 0;
		curveMag    = 0;
		ballRadius  = transform.localScale.x;	// Assuming that the ball is always scaled uniformly.
	}



	// Currently handles physics velocity limiting
	void Update() {
		// Ball Rotation
		rollVelocity = Vector3.Cross(rigidbody.velocity, Vector3.forward);
		transform.RotateAround(transform.position, rollVelocity, currSpeed * Time.deltaTime * 180 / (ballRadius * Mathf.PI));
	}

	void FixedUpdate() {
		// Speed limiting
		if (currSpeed > maxSpeed) {
			rigidbody.velocity = rigidbody.velocity.normalized * maxSpeed;
			currSpeed = maxSpeed;
		}

		// Curve implementation
//		rigidbody.velocity = (rigidbody.velocity + Vector3.Cross(rigidbody.velocity, new Vector3(0, 0, curveMag))).normalized * rigidbody.velocity.magnitude;
		rigidbody.AddForce(Vector3.Cross(rigidbody.velocity, Vector3.forward) * curveMag);
	}
	
	// Only collisions with the outer DeathField will trigger OnCollisionEnter() calls
	void OnCollisionEnter(Collision collision) {
		foreach(ContactPoint contact in collision.contacts) {
			if (contact.otherCollider.gameObject == goalRing) {
				gameScript.GoalScored(gameObject, contact.point, lastTouched);
				break;
			}
		}

		clockScript.ResetMissingBallTime();
	}

	// Set the value of lastTouched, to keep track of the last player to have touched the ball
	public void OnDeflect(int index) {
		lastTouched = index;
	}
	
	
	
	// Upon every collision of the ball, the ball keeps track of the speed imparted to it, to pass on to the next collider
	public void SetCurrSpeed(float argSpeed) {
		currSpeed = argSpeed;
	}
	
	public float GetCurrSpeed() {
		return currSpeed;
	}

	// Upon every collision of the ball, player will impart the new curve factor to be applied to the ball
	public void SetCurveMag(float argCurveMag) {
		curveMag = Mathf.Max(Mathf.Min(argCurveMag * spinFactor, maxCurveMag), -maxCurveMag);
	}

	public void PlayShotSound() {
		audio.PlayOneShot(shotSounds[(int)(Mathf.Floor(Random.value * 0.99f * shotSounds.Capacity))]);
	}
}
