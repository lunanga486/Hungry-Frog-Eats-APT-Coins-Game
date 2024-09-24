using UnityEngine;

namespace RoadCrossing
{
	/// <summary>
	/// This script runs a function in a target object when clicked on. In order to detect clicks you need to attach a collider to this object. It can also have
	/// an input button assigned (ex "Fire1") to trigger it with keyboard or gamepad controls.
	/// </summary>
	public class RCGButtonFunction : MonoBehaviour
	{
		// The target object in which the function needs to be executed
		public Transform functionTarget;
	
		// The name of the function that will be executed
		public string functionName;
	
		// The numerical parameter passed along with this function
		public string functionParameter;
	
		// The sound of the click and the source of the sound
		public AudioClip soundClick;
		public Transform soundSource;

		/// <summary>
		/// Raises the mouse over event.
		/// </summary>
		void OnMouseOver()
		{
			// Execute the button function when there's a mouse click on it
			if ( Time.deltaTime > 0 && Input.GetMouseButton(0) )    ExecuteFunction();
		}
	
		/// <summary>
		/// Executes the function defined in the inspector by name and transform that holds the script to execute.
		/// </summary>
		void ExecuteFunction()
		{
			// Play a sound from the source
			if( soundSource )
				if( soundSource.GetComponent<AudioSource>() )
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundClick);
		
			// Run the function at the target object
			if( functionName != string.Empty )
			{  
				if( functionTarget )
				{
					// Send the message to the target object
					functionTarget.SendMessage(functionName, functionParameter);
				}
			}
		}
	}
}