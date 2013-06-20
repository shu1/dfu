using UnityEngine;
using System.Collections;

public class Game : GhopperEnv {
	public GameObject playerPrefab;
	public GameObject ballPrefab;
	public GameObject invalidPrefab;
	public GameObject bumperPrefab;
	public GameObject pegPrefab;
	public GameObject boardObject;
	public GameObject clockObject;
	public Material[] boardMaterials;
	public Material[] ballMaterials;	// Material transfered to ball on collision with player

	const int scoreUp = 1;				// Points gained for scoring a goal
	const int scoreDown = 1;			// Points lost for being scored on
	const int winScore = 5;				// Points required to win
	const float boardRadius = 10;		// Radius of board
	const float spawnRadius = 5;		// Distance of ball spawn from center
	const float bufferHalfAngle = 10;	// Half the angle-width of buffer zone in degrees
	
	const float pegDistance = 9;		// Distance of score box from center
	public int numPlayers;				// Number of players
	int numBalls;						// Number of balls currently existing
	int[] playerScores;					// Array keeping track of player scores
	Player[] players;					// Array of GameObject handles to players
	PlayerScoreBezel scoreBezel;		// Bezel score stuff
	Clock clock;						// Handle to clock



	protected override void Awake() {
		base.Awake();
		Init();
	}
	
	public override void OnReady() {
		// Retrieve player information
		GhopperPlayer[] gPlayers = GetPlayers();
		numPlayers = gPlayers.Length;
		
		Camera.main.transform.Rotate(0, 0, -180 / numPlayers);	// Rotate camera to match board orientation with bezel
		
		boardObject.renderer.material = boardMaterials[numPlayers-2];	// Select board based on number of players

		// Game parameters initialization
		playerScores = new int[numPlayers];
		players = new Player[numPlayers];
		scoreBezel = new PlayerScoreBezel(numPlayers);
		clock = clockObject.GetComponent<Clock>();
		
		// Player info generation
		Color colorRed = new Color (1.0f, 0.2f, 0.2f);
		Color colorBlue = new Color (0.3f, 0.3f, 1.0f);
		Color colorGreen = new Color (0.3f, 1.0f, 0.3f);
		Color colorYellow = new Color (1.0f,0.72f, 0.3f);
		Color colorPurple = new Color (0.5f, 0.1f, 0.4f); 
		Color colorCyan = new Color(0.0f, 0.9f, 0.9f);
		Color[] playerColors = {colorRed, colorBlue, colorGreen, colorYellow, colorPurple, colorCyan};
		string[] playerNames = {"Red", "Blue", "Green", "Yellow", "Purple", "Cyan"};
		
		// Player info initialization
		for (int i = 0; i < numPlayers; ++i) {
			playerScores[i] = 0;

			GameObject playerObject = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
			players[i] = playerObject.GetComponent<Player>();
			players[i].InitPlayerInfo(i, playerColors[i], ballMaterials[i], GetBallSpawnPos(i));
			
			scoreBezel.SetPlayerName(i, playerNames[i]);
			scoreBezel.SetPlayerColor(i, playerColors[i]);
			scoreBezel.SetPlayerScore(i, playerScores[i]);
		}
		
		// Initialize Bezel
		bezel.LoadTemplate(scoreBezel);
		//scoreBezel.Show();
		
		SpawnBall(GetBallSpawnPos(0));	// Spawn a ball at player one's position

		clock.SetGoalSpawnPos(GetBallSpawnPos(0));	// Initialize clock's spawn position, in case the ball disappears before a goal is scored, it has a place to spawn again
		
//		SpawnBumpers();		TODO: prefab
//		SpawnPegs();

		// Add touch input listeners
		InputMaster.WorldTouchStarted += TouchStarted;
		InputMaster.WorldTouchUpdate += TouchUpdate;
		InputMaster.WorldTouchEnded += TouchEnded;
		
		Show();
	}


	
	// Determines which player's region it started in, and calls the TouchStarted() of the corresponding player
	void TouchStarted(TouchInfo touch) {
		int playerIndex = GetPlayerIndex(touch.worldPos.x, touch.worldPos.y);
		if (playerIndex >= 0) {	// -1 indicates buffer zone touch
			players[playerIndex].TouchStarted(touch);
		}
		else {
//			Instantiate(invalidPrefab, touch.worldPos, Quaternion.identity);	// TODO: prefab
		}
	}

	// Calls the TouchUpdate() of all players. Players determine whether the touch belongs to them or not. Because if a player's touch slides out of the player's boundary, the player can still keep track of it.
	void TouchUpdate(TouchInfo touch) {
		for (int i = 0; i < numPlayers; ++i) {
			players[i].TouchUpdate(touch);
		}
	}
	
	// Calls the TouchEnded() of all players. Players determine whether the touch belongs to them or not. Because if a player's touch ends outside the player's boundary, the player can still keep track of it.
	void TouchEnded(TouchInfo touch) {
		for (int i = 0; i < numPlayers; ++i) {
			players[i].TouchEnded(touch);
		}
	}

	

	// Spawns a ball at the given position
	public void SpawnBall(Vector3 ballPos) {
		GameObject.Instantiate(ballPrefab, ballPos, Quaternion.identity);
		numBalls = GameObject.FindGameObjectsWithTag("ball").Length;
	}
	
	// Destroys the ball
	public void DespawnBall(GameObject ball) {
		GameObject.Destroy(ball);
	}
	
	// Destroys the ball. If no more balls exist, one is spawned at the position of playerIndex
	public void DespawnBall(GameObject ball, int playerIndex) {
		GameObject.Destroy(ball);
		numBalls = GameObject.FindGameObjectsWithTag("ball").Length;
		if (playerIndex >= 0 && numBalls <= 1) {	// Check if last ball on table
			clock.SetGoalSpawnPos(GetBallSpawnPos(playerIndex));
			clock.StartGoalScoreSpawnDelay();    
			clock.StopTime();   
		}
	}

	

	// Called whenever a goal is scored, and updates player scores based on who scored on whom
	public void GoalScored(GameObject ball, Vector3 goalPos, int scorerIndex) {
		int scoreeIndex = GetPlayerIndex(goalPos.x, goalPos.y);
		
		if (scoreeIndex >= 0) {	// -1 means buffer zone
			if (scorerIndex == scoreeIndex) {	// Check for own goal
				if (playerScores[scoreeIndex] > 0){
					playerScores[scoreeIndex] -= scoreDown;
					players[scorerIndex].pegs.subtractPoint();
					scoreBezel.SetPlayerScore(scoreeIndex, playerScores[scoreeIndex]);
				}
			}
			
			else {
				playerScores[scorerIndex] += scoreUp;
				players[scorerIndex].pegs.addPoint(players[scoreeIndex].playerColor);
				scoreBezel.SetPlayerScore(scorerIndex, playerScores[scorerIndex]);
				
				if (playerScores[scorerIndex] == winScore) {
					PlayerWins(scorerIndex);
				}
			}
			
			// Play goal sound
			GameObject goalSound = GameObject.Find("soundGoal");
			goalSound.GetComponent<GoalSound>().PlayGoalSound();

			DespawnBall(ball, scoreeIndex);

			// Reset all players
			for (int i = 0; i < numPlayers; ++i) {
				players[i].ResetSphere();
			}
		}
	}

	// Called when a player has attained the win score
	void PlayerWins(int playerIndex) {
		Results results = GetComponent<Results>();
		results.winningPlayer = playerIndex;
		results.showResults = true;
	}



	// Returns the ball spawn position corresponding to the player
	Vector3 GetBallSpawnPos(int playerIndex) {
		float angle = (playerIndex * Mathf.PI * 2 + Mathf.PI) / numPlayers;
		return new Vector3(spawnRadius * Mathf.Sin(angle), spawnRadius * Mathf.Cos(angle), 0);
	}

	// Returns the player index that "owns" the point (x,y) on the board.
	int GetPlayerIndex(float x, float y) {
		float angle = GetAngle(x, y);
		float wedgeAngle = Mathf.PI * 2 / numPlayers;
		float nearestBufferAxis = Mathf.Round(angle / wedgeAngle) * wedgeAngle;
		int index = -1;	// -1 means in buffer zone
		if (Mathf.Abs(angle - nearestBufferAxis) > bufferHalfAngle * Mathf.Deg2Rad) {
			index = (int)(numPlayers * angle / (Mathf.PI * 2));
		}
		return index;
	}
	
	// Gives the angle that a point (x,y) on the board makes with the vertical, as per Grasshopper convention.
	float GetAngle(float x, float y) {
		float angle = Mathf.Atan2(x, y);
		if (angle < 0) {
			angle += Mathf.PI * 2;
		}
		return angle;
	}
	
	

	void SpawnBumpers() {
		float angle;
		GameObject[] bumpers = new GameObject[numPlayers];
		for (int i = 0; i < numPlayers; ++i) {
			angle = i * Mathf.PI * 2 / numPlayers;
			bumpers[i] = (GameObject)GameObject.Instantiate(bumperPrefab, Vector3.zero, Quaternion.identity);
			bumpers[i].transform.position = (new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0) * boardRadius);
		}
	}
	
	void SpawnPegs() {
		float angle;
		GameObject[] pegs = new GameObject[numPlayers];
		for (int i = 0; i < numPlayers; ++i) {
			angle = (i * Mathf.PI * 2 + Mathf.PI) / numPlayers;
			pegs[i] = (GameObject)GameObject.Instantiate(pegPrefab, Vector3.zero, Quaternion.identity);
			pegs[i].transform.Rotate(new Vector3(0, 0, -angle * Mathf.Rad2Deg));
			pegs[i].transform.position = (new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0) * pegDistance);
			pegs[i].transform.SetZPos(-6); //Remove once new results screen is in.
			players[i].pegs = pegs[i].GetComponent<ScoreBehavior>();
		}
	}
}
