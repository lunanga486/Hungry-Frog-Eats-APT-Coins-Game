using UnityEngine;
using System.Collections;

namespace RoadCrossing
{
	public class RCGRemoveAfterDistance : MonoBehaviour
	{

		internal Transform thisTransform;
		internal Transform cameraObject;

		public float distance = 20;

		// Use this for initialization
		void Start()
		{
			thisTransform = transform;
			cameraObject = GameObject.FindGameObjectWithTag("MainCamera").transform;
		}

		// Update is called once per frame
		void Update()
		{
			if (thisTransform.position.x < cameraObject.position.x - distance) Destroy(gameObject);
		}
	}
}
