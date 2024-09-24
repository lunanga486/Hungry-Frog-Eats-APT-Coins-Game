using UnityEngine;

namespace RoadCrossing
{
	/// <summary>
	/// Animates a sprite or a text mesh with several colors over time. You can set a list of colors, and the speed at
	/// which they change.
	/// </summary>
	public class RCGAnimateColors : MonoBehaviour
	{
		//An array of the colors that will be animated
		public Color[] colorList;
	
		//The index number of the current color in the list
		public int colorIndex = 0;
	
		//How long the animation of the color change lasts, and a counter to track it
		public float changeTime = 1;
		public float changeTimeCount = 0;
	
		//How quickly the sprite animates from one color to another
		public float changeSpeed = 1;
	
		//Is the animation paused?
		public bool isPaused = false;
	
		//Is the animation looping?
		public bool isLooping = true;
	
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			// Apply the chosen color to the sprite or text mesh
			SetColor();
		}
	
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update()
		{
			// If the animation isn't paused, animate it over time
			if( isPaused == false )
			{
				if( changeTime > 0 )
				{
					// Count down to the next color change
					if( changeTimeCount < changeTime )
					{
						changeTimeCount += Time.deltaTime;
					}
					else
					{
						changeTimeCount = 0;
					
						// Switch to the next color
						if( colorIndex < colorList.Length - 1 )
						{
							colorIndex++;
						}
						else
						{
							if( isLooping == true )
								colorIndex = 0;
						}
					}
				}
			
				TextMesh textMesh = GetComponent<TextMesh>();
				SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

				// If we have a text mesh, animated its color
				if( textMesh )
				{
					textMesh.color = Color.Lerp(textMesh.color, colorList[colorIndex], changeSpeed * Time.deltaTime);
				}
			
				// If we have a sprite renderer, animated its color
				if( spriteRenderer )
				{
					spriteRenderer.color = Color.Lerp(spriteRenderer.color, colorList[colorIndex], changeSpeed * Time.deltaTime);
				}
			
				if( GetComponent<Renderer>().sharedMaterial )
				{
					GetComponent<Renderer>().sharedMaterial.color = Color.Lerp(GetComponent<Renderer>().sharedMaterial.color, colorList[colorIndex], changeSpeed * Time.deltaTime);
				}
			}
			else
			{
				// Apply the chosen color to the sprite or text mesh
				SetColor();
			}
		}
	
		/// <summary>
		/// Applies the chosen color to the sprite based on the index from the list of colors
		/// </summary>
		void SetColor()
		{
			TextMesh textMesh = GetComponent<TextMesh>();
			SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

			// If you have a text mesh component attached to this object, set its color
			if( textMesh )
			{
				textMesh.color = colorList[colorIndex];
			}

			// If you have a sprite renderer component attached to this object, set its color
			if( spriteRenderer )
			{
				spriteRenderer.color = colorList[colorIndex];
			}
		}
	}
}