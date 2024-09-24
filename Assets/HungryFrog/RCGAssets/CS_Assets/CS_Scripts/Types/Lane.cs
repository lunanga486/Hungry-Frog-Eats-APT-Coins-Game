using UnityEngine;
using System;

namespace RoadCrossing.Types
{
	[Serializable]
	public class Lane
	{
		public Transform laneObject;
		public int laneChance = 1;
		public float laneWidth = 1;
		public float itemChance = 1;
	}
}