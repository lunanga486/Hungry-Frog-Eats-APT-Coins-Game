using UnityEngine;
using System;

namespace RoadCrossing.Types
{
	[Serializable]
	public class Powerup
	{
		// The name of the function 
		public string startFunctionA = "SetScoreMultiplier";
		public float startParamaterA = 2;

		// The name of the function 
		public string startFunctionB = "SetScoreMultiplier";
		public float startParamaterB = 2;

		// The duration of this powerup. After it reaches 0, the end functions run
		public float duration = 10;
		internal float durationMax;

		// The name of the function 
		public string endFunctionA = "SetScoreMultiplier";
		public float endParamaterA = 1;

		// The name of the function 
		public string endFunctionB = "SetScoreMultiplier";
		public float endParamaterB = 1;

		// The icon of this powerup
		public Transform icon;
	}
}
