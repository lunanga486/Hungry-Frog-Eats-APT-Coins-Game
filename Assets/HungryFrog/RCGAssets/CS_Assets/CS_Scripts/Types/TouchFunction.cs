using System;

namespace RoadCrossing.Types
{
	[Serializable]
	public class TouchFunction
	{
		// The name of the function that will run
		public string functionName = "CancelMove";
		
		// The tag of the target that the function will run on
		public string targetTag = "Player";
		
		// A parameter that is passed along with the function
		public float functionParameter = 0;
	}
}
