using UnityEngine;
using RoadCrossing.Types;

namespace RoadCrossing
{
	/// <summary>
	/// This script defines a a platform which allows a player to move on top of it by changing the move height of the player.
	/// </summary>
	public class RCGPlatform:MonoBehaviour
	{
//		// The tag of the object that can touch this platform
//		public string touchTargetTag = "Player";
//	
//		/// <summary>
//		/// Is executed when this obstacle touches another object with a trigger collider
//		/// </summary>
//		/// <param name="other"><see cref="Collider"/></param>
//		void OnTriggerEnter(Collider other)
//		{	
//			// Check if the object that was touched has the correct tag
//			if( other.tag == touchTargetTag )
//			{
//				// Go through the list of functions and runs them on the correct targets
//				foreach( var touchFunction in touchFunctions )
//				{
//					// Check that we have a target tag and function name before running
//					if( touchFunction.functionName != string.Empty )
//					{
//						// If the targetTag is "TouchTarget", it means that we apply the function on the object that ouched this lock
//						if ( touchFunction.targetTag == "TouchTarget" )
//						{
//							// Run the function
//							other.SendMessage(touchFunction.functionName, transform);
//						}
//						else if ( touchFunction.targetTag != string.Empty )    // Otherwise, apply the function on the target tag set in this touch function
//						{
//							// Run the function
//							GameObject.FindGameObjectWithTag(touchFunction.targetTag).SendMessage("ChangeHeight", touchFunction.functionParameter);
//						}
//					}
//				}
//			
//				// If there is an animation, play it
//				if( animation && hitAnimation )
//				{
//					// Stop the animation
//					animation.Stop();
//				
//					// Play the animation
//					animation.Play(hitAnimation.name);
//				}
//			
//				// If this object is removable, count down the touches and then remove it
//				if( isRemovable == true )
//				{
//					// Reduce the number of times this object was touched by the target
//					removeAfterTouches--;
//				
//					if( removeAfterTouches <= 0 )
//					{
//						if( deathEffect )
//							Instantiate(deathEffect, transform.position, Quaternion.identity);
//					
//						Destroy(gameObject);
//					}
//				}
//			
//				// If there is a sound source and a sound assigned, play it
//				if( soundSourceTag != string.Empty && soundHit )
//					GameObject.FindGameObjectWithTag(soundSourceTag).audio.PlayOneShot(soundHit);
//			}
//		}
	}
}