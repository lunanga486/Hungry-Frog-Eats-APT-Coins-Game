using UnityEngine;
using System;
namespace RoadCrossing.Types
{
	[Serializable]
	public class ObjectDrop
	{
		// The object that can be dropped
		public Transform droppedObject;
		
		// The drop chance of the object
		public int dropChance = 1;
	}
}