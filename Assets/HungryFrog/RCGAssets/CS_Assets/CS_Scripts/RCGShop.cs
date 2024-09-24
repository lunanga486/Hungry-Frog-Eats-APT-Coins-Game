using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using RoadCrossing.Types;

namespace RoadCrossing
{
	/// <summary>
	/// This script handles a shop, in which there are items that can be bought and unlocked with coins.
	/// </summary>
	public class RCGShop : MonoBehaviour
	{
		//How many coins we have left in the shop
		public int coinsLeft = 0;
		
		//The text that displays the coins we have
		public Transform coinsText;
		public Text coinsTextBlockchain;

		//The player prefs record of the coins we have
		public string coinsPlayerPrefs = "Coins";

		// An array of shop items that can be bought and unlocked
		public ShopItem[] shopItems;

		//The number of the currently selected item
		public int currentItem = 0;
		
		//This is the player prefs name that will be updated with the number of the currently selected item
		public string playerPrefsName = "CurrentPlayer";
		
		//The color of the item when we have at least one of it
		public Color unselectedColor = new Color(0.6f,0.6f,0.6f,1);
		
		//The color of the item when it is selected
		public Color selectedColor = new Color(1,1,1,1);
		
		//The color of the item when we can't afford it
		public Color errorColor = new Color(1,0,0,1);

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			//Get the number of coins we have
			coinsLeft = PlayerPrefs.GetInt(coinsPlayerPrefs, coinsLeft);
			
			//Update the text of the coins we have
			coinsText.GetComponent<Text>().text = coinsLeft.ToString();
			
			//Get the number of the current player
			currentItem = PlayerPrefs.GetInt(playerPrefsName, currentItem);
			
			//Update all the items
			UpdateItems();
		}

		/// <summary>
		/// Creates a lane, sometimes reversing the paths of the moving objects in it
		/// </summary>
		/// <param name="laneCount">Lane count.</param>
		void UpdateItems()
		{
			
			for ( int index = 0 ; index < shopItems.Length ; index++ )
			{
				//Get the lock state of this item from player prefs
				shopItems[index].lockState = PlayerPrefs.GetInt(shopItems[index].playerPrefsName, shopItems[index].lockState);
				//Update based on Singleton

				Debug.Log("index: " + index);
				Debug.Log(shopItems[index].itemButton);

				if (index == 1 & ResourceBoost.Instance.piggy == 1) {
					shopItems[index].lockState = 1;
				}
				else if (index == 2 & ResourceBoost.Instance.doggy == 1)
				{
					shopItems[index].lockState = 1;
				}
				else if (index == 3 & ResourceBoost.Instance.goaty == 1)
				{
					shopItems[index].lockState = 1;
				}
				else if (index == 4 & ResourceBoost.Instance.turtle == 1)
				{
					shopItems[index].lockState = 1;
				}
				else if (index == 5 & ResourceBoost.Instance.koala == 1)
				{
					shopItems[index].lockState = 1;
				}
				else if (index == 6 & ResourceBoost.Instance.sheep == 1)
				{
					shopItems[index].lockState = 1;
				}
				else if (index == 7 & ResourceBoost.Instance.duck == 1)
				{
					shopItems[index].lockState = 1;
				}

				//Deselect the item
				shopItems[index].itemButton.GetComponent<Image>().color = unselectedColor;
				
				//If we already unlocked this item, don't display its price
				if ( shopItems[index].lockState > 0 )
				{
					//Deactivate the price and coin icon
					shopItems[index].itemButton.Find("TextPrice").gameObject.SetActive(false);
					
					//Highlight the currently selected item
					if ( index == currentItem )    shopItems[index].itemButton.GetComponent<Image>().color = selectedColor;
				}
				else
				{
					//Update the text of the cost
					shopItems[index].itemButton.Find("TextPrice").GetComponent<Text>().text = shopItems[index].costToUnlock.ToString();
				}
			}
		}

		/// <summary>
		/// Creates a lane, sometimes reversing the paths of the moving objects in it
		/// </summary>
		/// <param name="laneCount">Lane count.</param>
		void BuyItem( int itemNumber )
		{
			//If we already unlocked this item, just select it
			if ( shopItems[itemNumber].lockState > 0 )
			{
				//Select the item
				SelectItem(itemNumber);
			}
			else if ( shopItems[itemNumber].costToUnlock <= coinsLeft ) //If we have enough coins, buy this item
			{
				//Increase the item count
				shopItems[itemNumber].lockState = 1;
				
				//Register the item count in the player prefs
				PlayerPrefs.SetInt(shopItems[itemNumber].playerPrefsName, shopItems[itemNumber].lockState);
				
				//Deduct the price from the coins we have
				coinsLeft -= shopItems[itemNumber].costToUnlock;
				
				//Update the text of the coins we have
				coinsText.GetComponent<Text>().text = coinsLeft.ToString();

				//Register the item lock state in the player prefs
				PlayerPrefs.SetInt(coinsPlayerPrefs, coinsLeft);
				
				//Select the item
				SelectItem(itemNumber);
			}
			
			//Update all the items
			UpdateItems();
		}
		
		//This function selects an item
		/// <summary>
		/// Creates a lane, sometimes reversing the paths of the moving objects in it
		/// </summary>
		/// <param name="laneCount">Lane count.</param>
		void SelectItem( int itemNumber )
		{
			currentItem = itemNumber;
			
			PlayerPrefs.SetInt( playerPrefsName, itemNumber);
		}
	}
}



