using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public GameObject explodePrefab;		// Particle effect obj to be spawned every ShotFired()
	public GameObject invalidPrefab;		// Particle on invalid touch
	public GameObject overloadPrefab;		// Particle when nearing overload

	const float minTouchRadius = 1;			// Crease boundary inner radius
	const float maxTouchRadius = 10;		// Crease boundary outer radius
	const float time1 = 1;					// Time from touch start at which collision changes from tier 1 to tier 2
	const float time2 = 2;					// Time from touch start at which collision changes from tier 2 to tier 3
	const float speedFactor1 = 1;			// Speed multiplication factor for tier 1 collision
	const float speedFactor2 = 1.25f;		// Speed multiplication factor for tier 2 collision
	const float speedFactor3 = 1.5f;		// Speed multiplication factor for tier 3 collision
	const float maxSpeedFactor = 1.2f;
	const float minSpeed = 8;				// Internal varibale to hold minimum speed to impart to ball on collision
	const float maxChargeTime = 3;			// Maximum charge time from touch start that contributes towards sphere size
	const float maxScale = 2.5f;			// Maximum scale that the player sphere mesh grows to
	const float overloadTime = 3.5f;		// Time from touch start at which sphere overloads
	const float cooldownRate = 2;			// Rate at which cooldown takes place relative to charge up
	const float penaltyFactor = 0.5f;		// Extra factor applied to cooldown rate when overload occurs
	const float spamTimerStart = 0.5f;		// After touch start, how long before another sphere can be started. Prevents Tiny sphere spamming.
	const float ballRadius = 1;				// Radius of ball, used for collision calculation
	
	bool bOverload;							// Flag whether the overload warning particles are being displayed
	bool bPenalty;							// Being penalized for overload?
	bool bTouchDown;						// Is there a valid touch?
	bool bCooldown;							// Can the sphere be charged?
	float chargeTime;						// Internal variable to keep track of current charge time
	float spamTimer;						// Current spam timer value
	float sphereRadius;						// Internal variable updated to sphere radius for collision calculation
	float impartSpeed;						// Internal variable to hold speed to impart to ball on collision
	float spin;
	int playerIndex;						// Player index, between 0 and numPlayers-1
	public Color playerColor;				// Player color
	Material ballMaterial;					// Material that is given to ball upon collision
	Vector3 playerForward, playerRight;		// Vectors for gesture calculations
	TouchInfo downTouch;					// Currently valid touch
	GameObject[] balls;						// Internal variable that serves as handle to balls
	Clock clockScript;						// Clock script handle
	public ScoreBehavior pegs;				// Score pegs assigned to each player
	GameObject overloadParticleSystem;		// Handle to overload particle system
	

	
	// Called by Game.cs to initialize player properties
	public void InitPlayerInfo(int argIndex, Color argColor, Material argPlayerMaterial, Material argBallMaterial, Vector3 argSpawnPos) {
		playerIndex = argIndex;
		playerColor = argColor;
		renderer.material = argPlayerMaterial;
		ballMaterial = argBallMaterial;

		playerForward = (Vector3.zero - argSpawnPos).normalized;
		playerRight = Vector3.Cross(playerForward, Vector3.forward).normalized;
	}
	
	void Start() {
		clockScript = GameObject.Find("Clock").GetComponent<Clock>();
		ResetSphere();
	}
	
	// Reset sphere to intial conditions to be called whenever a goal is scored
	public void ResetSphere() {
		bTouchDown = false;
		bPenalty = false;
		bCooldown = false;
		bOverload = false;
		chargeTime = 0;

		downTouch = null;
		renderer.enabled = false;
		collider.enabled = false;
		balls = GameObject.FindGameObjectsWithTag("ball");	// TODO: don't do this in Update()!
		if (overloadParticleSystem) {
			overloadParticleSystem.GetComponent<ParticleSystem>().Stop();
		}

		UpdateSphere(null);
	}



	void Update() {
		if (spamTimer > 0) {
			spamTimer -= Time.deltaTime;	
		}
		
		if (bCooldown) {
			if (bPenalty) {
				chargeTime = Mathf.Max(0, chargeTime - Time.deltaTime * cooldownRate * penaltyFactor);
			}
			else {
				chargeTime = Mathf.Max(0, chargeTime - Time.deltaTime * cooldownRate);
			}
			
			UpdateSphere(null);

			if (chargeTime == 0) {
				bCooldown = false;
				bOverload = false;
				renderer.enabled = false;
				collider.enabled = false;
			}
		}
	}

	// Updates sphere position and scale based on current charge time. Also detects sphere overload condition.
	void UpdateSphere(TouchInfo touch) {
		// Update sphere size according to current charge time
		float scale = Mathf.Min( (chargeTime * maxScale/maxChargeTime), maxScale);	// TODO: what are these /2 constants?
		transform.localScale = new Vector3(scale, scale, scale);
		transform.Rotate(Vector3.forward * -spin);
		sphereRadius = scale / 2;	// TODO: is this right? Neil- I dunno... It'd only work if the original sphere is 1 unit in diameter. Scale is the multiplier on the original size.
		
		// Overload warning
		if (!bOverload && chargeTime > overloadTime) {
//			overloadParticleSystem = (GameObject)Instantiate(overloadPrefab, transform.position, Quaternion.identity);	TODO: prefab
			bOverload = true;
		}
		
		// Overload condition
		if (chargeTime > overloadTime) {
			TouchEnded(touch);
			bPenalty = true;
		}
	}
	


	// Checks whether any valid touch already exists. If not, it makes the incoming touch the currently valid touch.
	public void TouchStarted(TouchInfo touch) {
		bool bInvalid = true;

		// Determine whether a currently valid touch already exists
		if (!bTouchDown && !bCooldown && spamTimer <= 0) {
			float dx = Mathf.Abs(touch.worldPos.x);
			float dy = Mathf.Abs(touch.worldPos.y);
			float distance = Mathf.Sqrt(dx * dx + dy * dy);

			// Determine whether touch occurred within crease
			if ((distance < maxTouchRadius) && (distance > minTouchRadius)) {
				balls = GameObject.FindGameObjectsWithTag("ball");        
				
				foreach (GameObject ball in balls) {
					//Checks if the sphere is being spawned on top of a ball
					Vector3 touchPos = new Vector3(touch.worldPos.x, touch.worldPos.y, 0);
					if ((touchPos - ball.transform.position).sqrMagnitude > (sphereRadius + ballRadius) * (sphereRadius + ballRadius)) {
						bInvalid = false;
						bPenalty = false;
						bTouchDown = true;
						renderer.enabled = true;
						collider.enabled = true;
						spamTimer = spamTimerStart;
						downTouch = touch;
						transform.position = touchPos;
						chargeTime = 1;
					}
				}
			}
		}

		if (bInvalid) {
//			Instantiate(invalidPrefab, touch.worldPos, Quaternion.identity);	TODO: prefab
		}
	}
	
	// Performs a regular update upon a touch being held down, only if the held touch is the currently valid touch.
	public void TouchUpdate(TouchInfo touch) {
		if (touch == downTouch && !bCooldown) {	// Check if this is the currently valid touch
			chargeTime = chargeTime + Time.deltaTime;

			Vector3 gestureVector = touch.worldPos - transform.position;
//			accel = Vector3.Dot(gestureVector, playerForward);
			spin = Vector3.Dot(gestureVector, playerRight);
			
			UpdateSphere(touch);
			
			balls = GameObject.FindGameObjectsWithTag("ball");	// TODO: don't do this in Update()!
		}
	}
	
	// Performs regular touch end functions only if the ended touch is the currently valid touch.
	public void TouchEnded(TouchInfo touch) {
		if (touch == downTouch) {	// Check if this is the currently valid touch
			downTouch = null;
			bTouchDown = false;
			bCooldown = true;	// Enter cooldown mode upon touch release
		}
	}
	
	// Calls ShotFired() if a ball enters trigger volume
	void OnTriggerEnter(Collider ball) {
			if (ball.tag == "ball" && renderer.enabled) {
				ShotFired(ball.gameObject);
			}
	}

	// Called every time a collision is detected between sphere and a ball. Computes the imparted speed and direction of ball and imparts them to ball.
	public void ShotFired(GameObject ball) {
		// Get handle to incoming ball
		Ball ballComponent = (Ball)ball.GetComponent<Ball>();
		float incomingSpeed = ballComponent.GetCurrSpeed();
		
		// Determine impart speed
		impartSpeed = incomingSpeed * ( (chargeTime - 1) * (maxSpeedFactor - 1) / (maxChargeTime - 1) + 1);
		Debug.Log("Speed : "+impartSpeed);
		impartSpeed = Mathf.Max(impartSpeed, minSpeed);
		
		// Particles
//		Instantiate(explodePrefab, transform.position, Quaternion.identity);	TODO: prefab
		
		// Update ball properties
		ball.rigidbody.velocity =  impartSpeed * (ball.transform.position - transform.position).normalized;
		ball.renderer.material = ballMaterial;
		ballComponent.SetCurrSpeed(impartSpeed);
		ballComponent.SetCurveMag(spin);
		ballComponent.OnDeflect(playerIndex);
		
		// Play sound
		ball.GetComponent<Ball>().PlayShotSound();
		
		// Enter cooldown mode
		bCooldown = true;
		
		// Ensure that clock starts running if not running presently
		clockScript.StartTime();
		clockScript.ResetMissingBallTime();
	}
}
