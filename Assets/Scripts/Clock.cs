using UnityEngine;

using System.Collections;

public class Clock : MonoBehaviour {
 

 // ========================================
 //               VARIABLES
 // ========================================

	// Game Script handles
	public GameObject game;         // Handle to GhopperEnv (Game manager object)
	Game gameScript;                // Handle to game.cs (Game manager script)

	// Internal variables
	float gameTime;                 // Current value of game time
	bool bTimeRunning;              // Is the game clock running?
	
	// Goal Score Spawn Delay variables
	float goalScoreSpawnDelay;      // Delay in seconds between goal score and next spawn
	bool bGoalScoreSpawnDelay;      // Is goal-score-spawn-delay timer active?
	float goalScoreSpawnTime;       // Current time on goal-score-spawn-delay timer
	Vector3 goalSpawnPos;           // Location at which to spawn ball during callback

	// Missing ball variables
	float missingBallResetTime;     // Time interval (sec) at which check is made for missing ball
	float missingBallTime;          // Current time for which ball has been missing



 // ========================================
 //              FUNCTIONS
 // ========================================  


 // INITIALIZATION FUNCTION
 // ----------------------------
 // This function is utilized to perform
 // clock initialization.
 // ----------------------------
	void Start () {

		// Initialize clock
		gameTime = 0;
		bTimeRunning = false;

		// Grab handle to game.cs
		if(game == null) {
			Debug.Log ("Game object missing in Clock Script!");
		}
		else {
			gameScript = (Game)game.GetComponent("Game");
		}

		// Initialize Goal Score Spawn Delay parameters
		goalScoreSpawnDelay = 2;

		// Initialize Missing Ball parameters
		missingBallResetTime = 15;
		missingBallTime = 0;
	}
 
 // UPDATE
 // ------------------------
 // This function handles game clock time-keeping.
 // Also handles delayed function calls to
 // - Goal Score Spawn Delay
 // - Ball Missing Check
 // ------------------------

	 void Update () {
			
			// Game clock tick
			// ---------------
			if (bTimeRunning) {
				gameTime += Time.deltaTime;
			}

			// Goal Score Spawn Delay
			// ----------------------
			// Check whether GoalScoreSpawnDelay needs to be executed
			if (bGoalScoreSpawnDelay) {

				goalScoreSpawnTime += Time.deltaTime;

				// Check whether it is time for callback
				if (goalScoreSpawnTime >= goalScoreSpawnDelay) {

					gameScript.SpawnBall(goalSpawnPos);
					bGoalScoreSpawnDelay = false;
				}
			}

			// Missing Ball Check
			// ------------------
			// Increment missing ball time every update
			// The trigger volume will take care of resetting it
			missingBallTime += Time.deltaTime;

			if (missingBallTime >= missingBallResetTime) {

				// Find all existing balls
				GameObject[] balls;
				balls = GameObject.FindGameObjectsWithTag("ball");
				
				foreach (GameObject ball in balls) {
					// Despawn all existing balls
					gameScript.DespawnBall(ball);
				}

				// Spawn a fresh ball and reset missing ball time
				gameScript.SpawnBall(goalSpawnPos);
				ResetMissingBallTime();
			}

	 }
	

	
	// CLOCK CONTROL FUNCTIONS    
	// ---------------------------
	// These functions are for clock transport
	// ---------------------------

	// GetTime() returns the current game clock time

	public float GetTime() {
		return gameTime;
	}
	

	// StartTime() sets the clock to running mode
	// It DOES NOT reset the clock. The clock resumes
	// from its current game time.

	public void StartTime() {
		bTimeRunning = true;
	}
	

	// StopTime() pauses the game clock.

	public void StopTime() {
		bTimeRunning = false;
	}
	

	// ResetTime() sets the game time back to 0

	public void ResetTime() {
		gameTime = 0;
	}
	   


	// GOAL SCORE SPAWN DELAY    
	// ---------------------------
	// These functions handle the timer and callback
	// functionality of the delay between goal scored
	// and next ball spawned
	// --------------------------- 

	public void StartGoalScoreSpawnDelay() {
		goalScoreSpawnTime = 0;
		bGoalScoreSpawnDelay = true;
	}

	public void SetGoalSpawnPos(Vector3 spawnPos) {
		goalSpawnPos = spawnPos;
	}

	// Sets the missing timer back to zero. Called with every ShotFired(). If no shots are fired for a certain amount of time, it is safe to assume that all balls can be despawned and a fresh ball can be spawned.
	public void ResetMissingBallTime() {
		missingBallTime = 0;
	} 
}
