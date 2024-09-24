using System.Collections;
using UnityEngine;
using RoadCrossing.Types;

namespace RoadCrossing
{
	/// <summary>
	/// Defines the player, its speed and movement limits, as well as the different types of deaths it may suffer.
	/// </summary>
	public class RCGPlayer : MonoBehaviour
	{
		// The farthest point the player reached forward
		public int moveProgress = 0;
		public int moveScore = 1;
		
		internal Transform thisTransform;
		internal GameObject gameController;

		// The player's movement speed, and variables that check if the player is moving now, where it came from, and where it's going to
		public float speed = 2.8f;
		static float speedMultiplier = 1;
		internal bool  isMoving = false;
		internal Vector3 previousPosition;
		static Vector3 targetPosition;

		// The movement limits object. This object contains some colliders that bounce the player back into the game area
		public Transform moveLimits;

		// The player can't move or perform actions for a few seconds
		public float moveDelay = 0;

		// Holds the next move the player should have. This is recorded if you try to move while the player is already moving, so the next move executes immediately after this move is finished
		internal string nextMove = "stop";
		internal string currentMove;

		// The height at which the player is currently moving. This is used to allow the player to move on higher/lower platforms
		internal float moveHeight = 0;
		
		// Various animations for the player
		public AnimationClip animationMove;
		public AnimationClip animationCoin;
		public AnimationClip animationSpawn;
		public AnimationClip animationVictory;
		
		// The Animator controller for the player. This is used to replace the Animation system.
		public Animator animatorObject;

		// Death effects that show when the player is killed
		public Transform[] deathEffect;

		// Is this player invulnerable? If so, then you don't die when recieving a Die() function call
		public bool isInvulnerable = false;

		// The object that this player is currently standing on, such as a platform
		public Transform attachedToObject;

		// Did the player win the game?
		internal bool isVictorious = false;

		// Various sounds and their source
		public AudioClip[] soundMove;
		public AudioClip[] soundCoin;
		public string soundSourceTag = "GameController";
	
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			speedMultiplier = 1;

			thisTransform = transform;

			targetPosition = thisTransform.position;
		
			gameController = GameObject.FindGameObjectWithTag("GameController");
		}
	
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update()
		{
			if ( isVictorious == false )
			{
				// Keep the move limits object moving forward/backward along with the player
				if( moveLimits )
				{
					Vector3 newPosition = new Vector3();

					newPosition.x = thisTransform.position.x;

					moveLimits.position = newPosition;
				}

				// Count down the move delay
				if( moveDelay > 0 )
					moveDelay -= Time.deltaTime * speedMultiplier;

				// If the player is not already moving, it can move
				if( Time.timeScale > 0 )
				{
					// You can move left/right only if you are not already moving forward/backwards
					if( Input.GetAxisRaw("Vertical") == 0 )
					{
						// Moving right
						if( Input.GetAxisRaw("Horizontal") > 0 )
						{
							// Move one step to the right
							Move("right");
						}
					
						// Moving left
						if( Input.GetAxisRaw("Horizontal") < 0 )
						{
							// Move one step to the left
							Move("left");
						}
					}
				
					// You can move forward/backwards only if you are not already moving left/right
					if( Input.GetAxisRaw("Horizontal") == 0 )
					{
						// Moving forward
						if( Input.GetAxisRaw("Vertical") > 0 )
						{
							// Move one step forward
							Move("forward");
						}
					
						// Moving backwards
						if( Input.GetAxisRaw("Vertical") < 0 )
						{
							// Move one step backwards
							Move("backward");
						}
					}
				}

				// If the player is moving, move it to its target
				if ( isMoving == true )
				{
					// Keep moving towards the target position until we reach it
					if ( attachedToObject == null )
					{
						if( Vector3.Distance( thisTransform.position, targetPosition) > 0.1 )
						{
							// Move this object towards the target position
							thisTransform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed * speedMultiplier);
						}
						else
						{
							thisTransform.position = targetPosition;
							
							if ( thisTransform.position.x > moveProgress )    
							{
								gameController.SendMessage("ChangeScore", moveScore);
								
								moveProgress++;
							}
							
							// The object is not moving anymore
							isMoving = false;
						}
					}
				}
				else if ( nextMove != "stop" && nextMove != currentMove ) // If there is a next move recorded, move the player to it and clear it
				{
					// Move the player to the next move
					Move(nextMove);
					
					// Clear the next move
					nextMove = "stop";
				}
			}
		}
	
		/// <summary>
		/// Moves an object from its current position to a target position, over time
		/// </summary>
		/// <param name="currentMove">Move direction.</param>
		void Move(string moveDirection)
		{
			if ( isVictorious == false )
			{
				if( isMoving == false && moveDelay <= 0 )
				{
					currentMove = moveDirection;

					// If the player is attached to an object, detach from it
					if ( attachedToObject )    Detach();

					// The object is moving
					isMoving = true;
				
					switch( currentMove.ToLower() )
					{
						case "forward":
							// Turn to the front
							Vector3 newEulerAngle = new Vector3();
							newEulerAngle.y = 0;
							thisTransform.eulerAngles = newEulerAngle;
					
							// Set the new target position to move to
							targetPosition = thisTransform.position + new Vector3(1, 2, 0);
					
							// Make sure the player lands on the grid 
							targetPosition.x = Mathf.Round(targetPosition.x);
							targetPosition.z = Mathf.Round(targetPosition.z);
					
							// Register the last position the player was at, so we can return to it if the path is blocked
							previousPosition = thisTransform.position;
					
							break;
					
						case "backward":
							// Turn to the back
							newEulerAngle = new Vector3();
							newEulerAngle.y = 180;
							thisTransform.eulerAngles = newEulerAngle;

							// Register the last position the player was at, so we can return to it if the path is blocked
							previousPosition = thisTransform.position;
					
							// Make sure the player lands on the grid 
							targetPosition.x = Mathf.Round(targetPosition.x);
							targetPosition.z = Mathf.Round(targetPosition.z);
					
							// Set the new target position to move to
							targetPosition = thisTransform.position + new Vector3(-1, 2, 0);
					
							break;
					
						case "right":
							// Turn to the right
							newEulerAngle = new Vector3();
							newEulerAngle.y = 90;
							thisTransform.eulerAngles = newEulerAngle;

							// Register the last position the player was at, so we can return to it if the path is blocked
							previousPosition = thisTransform.position;
					
							// Make sure the player lands on the grid 
							targetPosition.x = Mathf.Round(targetPosition.x);
							targetPosition.z = Mathf.Round(targetPosition.z);
					
							// Set the new target position to move to
							targetPosition = thisTransform.position + new Vector3(0, 2, -1);
					
							break;
					
						case "left":
							// Turn to the left
							newEulerAngle = new Vector3();
							newEulerAngle.y = -90;
							thisTransform.eulerAngles = newEulerAngle;
					
							// Register the last position the player was at, so we can return to it if the path is blocked
							previousPosition = thisTransform.position;
					
							// Make sure the player lands on the grid 
							targetPosition.x = Mathf.Round(targetPosition.x);
							targetPosition.z = Mathf.Round(targetPosition.z);
					
							// Set the new target position to move to
							targetPosition = thisTransform.position + new Vector3(0, 2, 1);
							
							break;
					
						default:
							// Turn to the front
							newEulerAngle = new Vector3();
							newEulerAngle.y = 0;
							thisTransform.eulerAngles = newEulerAngle;
					
							// Set the new target position to move to
							targetPosition = thisTransform.position;
					
							targetPosition.Normalize();
					
							// Register the last position the player was at, so we can return to it if the path is blocked
							previousPosition = thisTransform.position;
					
							break;
					}

					targetPosition.y = moveHeight;

					// If we are using an Animator Object, use it for animation
					if ( animatorObject )
					{
						//print(animatorObject.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
						//animatorObject["Jump"].time = 0f;

						animatorObject.Play("Jump", -1, 0);			
						//print("F");

						//if ( animator && !animator.GetCurrentAnimatorStateInfo(0).IsName("walk") )    animator.Play("walk", -1, 0f);

					}
					else if ( GetComponent<Animation>() && animationMove )    // Otherwise, if there is an animation component, play it
					{
						// Stop the animation

						GetComponent<Animation>().Stop();
					
						// Play the animation
						GetComponent<Animation>().Play(animationMove.name);
					
						// Set the animation speed base on the movement speed
						GetComponent<Animation>()[animationMove.name].speed = speed * speedMultiplier;
					}

					// If there is a sound source and more than one sound assigned, play one of them from the source
					if( soundSourceTag != string.Empty && soundMove.Length > 0 )
						GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(soundMove[Mathf.FloorToInt(Random.value * soundMove.Length)]);
				}
				else
				{
					// If we are still moving, record the next move for smoother controls
					nextMove = moveDirection;
				}
			}
		}
	
		/// <summary>
		/// Cancels the player's current move, bouncing it back to it's previous position 
		/// </summary>
		/// <returns><c>true</c> if this instance cancel move the specified moveDelayTime; otherwise, <c>false</c>.</returns>
		/// <param name="moveDelayTime">Move delay time.</param>
		void CancelMove(float moveDelayTime)
		{
			// If there is an animation, play it
			if( GetComponent<Animation>() && animationMove )
			{
				// Set the animation speed base on the movement speed
				GetComponent<Animation>()[animationMove.name].speed = -speed * speedMultiplier;
			}
		
			// Set the previous position as the target position to move to
			targetPosition = previousPosition;
			
			// If there is a move delay, prevent movement for a while
			moveDelay = moveDelayTime + 0.2f;
		}

		/// <summary>
		/// Changes the height of the player. Used to allow the player to move to higher/lower platforms
		/// </summary>
		/// <param name="newHeight">The value of the new height, the y position of the player</param>
		void ChangeHeight( Transform targetHeight )
		{
			targetPosition.y = moveHeight = targetHeight.position.y;
		}
	
		//This function animates a coin added to the player
		/// <summary>
		/// Adds the coin/changes score.
		/// </summary>
		/// <param name="addValue">value to add to score</param>
		void AddCoin(int addValue)
		{
			gameController.SendMessage("ChangeScore", addValue);
		
			// If there is an animation, play it
			if( GetComponent<Animation>() && animationCoin && animationMove )
			{
				// Play the animation
				GetComponent<Animation>()[animationCoin.name].layer = 1; 
				GetComponent<Animation>().Play(animationCoin.name); 
				GetComponent<Animation>()[animationCoin.name].weight = 0.5f;
			}
		
			// If there is a sound source and more than one sound assigned, play one of them from the source
			if( soundSourceTag != string.Empty && soundCoin.Length > 0 )
				GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(soundCoin[Mathf.FloorToInt(Random.value * soundCoin.Length)]);
		}
	
		//This function destroys the player, and triggers the game over event
		/// <summary>
		/// Destroys the player, and triggers the game over event
		/// </summary>
		/// <param name="deathType">Death effect type.</param>
		void Die(int deathType)
		{
			//If you are invulnerable, don't die
			if ( isInvulnerable == false )
			{
				// If the player is attached to an object, detach from it
				if ( attachedToObject )    Detach();

				// Change the number of lives the player has, and check for game over
				gameController.SendMessage("ChangeLives", -1);

				// If we have death effects...
				if( deathEffect.Length > 0 )
				{
					// Create the correct death effect
					Instantiate(deathEffect[deathType], thisTransform.position, thisTransform.rotation);
				}
			
				// Deactivate the player object
				gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Spawns the player
		/// </summary>
		void Spawn()
		{
			// Activate the player object
			gameObject.SetActive(true); 
			
			// If there is an animation, update the animation speed based on speedMultiplier
			if( GetComponent<Animation>() && animationSpawn )    GetComponent<Animation>().Play(animationSpawn.name);
		}

		/// <summary>
		/// Changes the speed of the player
		/// </summary>
		/// <param name="setValue">The new speed of the player</param>
		void SetPlayerSpeed( float setValue )
		{
			speedMultiplier = setValue;

			// If there is an animation, update the animation speed base on speedMultiplier
			if( GetComponent<Animation>() && animationMove )    GetComponent<Animation>()[animationMove.name].speed = speed * speedMultiplier;

			// If there is an animation, update the animation speed base on speedMultiplier
			if( GetComponent<Animation>() && animationCoin )    GetComponent<Animation>()[animationCoin.name].speed = speed * speedMultiplier;
		}

		/// <summary>
		/// Detach this object from the object it is currently attached to
		/// </summary>
		void AttachToThis( Transform attachedObject )
		{
			// Set the current attached object
			attachedToObject = attachedObject;

			// Set the object we are attached to as the parent
			thisTransform.parent = attachedToObject;

			// Set the position to 0 locally
			thisTransform.localPosition = Vector3.zero;
			//StartCoroutine(AttachAnimation(attachedToObject));

			// The player is not moving
			isMoving = false;
		}

		/// <summary>
		/// Animates the player into the correct position it is attached to ( ex: on a moving platform )
		/// </summary>
		/// <returns>The animation.</returns>
		/// <param name="attachTarget">Attach target.</param>
		IEnumerator AttachAnimation( Transform attachTarget )
		{
			// Move towards the target posiiton
			while ( Vector3.Distance( thisTransform.position, attachTarget.position) > 0.1f )
			{
				thisTransform.position = Vector3.Slerp( thisTransform.position, attachTarget.position, Time.deltaTime * 10);

				yield return new WaitForFixedUpdate();
			}

			// Set the target position exactly
			thisTransform.position = attachTarget.position;
		}

		/// <summary>
		/// Detach this object from the object it is currently attached to
		/// </summary>
		void Detach()
		{
			// No object is attached
			attachedToObject = null;

			// The player has no parent
			thisTransform.parent = null;
		}

		/// <summary>
		/// Runs when the player wins the level
		/// </summary>
		void Victory()
		{
			isVictorious = true;
			
			// If there is a victory animation, play it
			if( GetComponent<Animation>() && animationVictory )    GetComponent<Animation>().Play(animationVictory.name);
		}
	}
}