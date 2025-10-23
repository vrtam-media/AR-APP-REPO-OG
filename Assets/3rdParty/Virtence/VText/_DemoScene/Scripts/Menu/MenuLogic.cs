// ----------------------------------------------------------------------
// File: 			MenuLogic
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Jan Knothe (jan.knothe@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;

namespace Virtence.VText.Demo
{
	/// <summary>
	/// 
	/// </summary>
	public class MenuLogic : MonoBehaviour
	{
		#region EXPOSED 
		public GameObject[] Menus;

		public AudioClip SelectionSound;

		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		private int _activeMenu = 0;
		#endregion // FIELDS

		#region PROPERTIES
		#endregion // PROPERTIES

		#region METHODS


		/// <summary>
		/// Control Menus
		/// </summary>
		/// <param name="i"></param>
		public void HandleMenus(int IDMenu, int IDItem)
		{
			ControlVisibility(IDMenu, IDItem);			
		}

		/// <summary>
		/// play the selection sound
		/// </summary>
		public void PlaySelectionSound() {
			GetComponent<AudioSource>().clip = SelectionSound;
			GetComponent<AudioSource>().Play();
		}

		/// <summary>
		/// simple logic, handle visibility of each menu, all items of submenus bring you back to mainmenu
		/// </summary>
		/// <param name="i"></param>
		private void ControlVisibility(int IDMenu, int IDItem)
		{
			if (Menus == null)
				return;

			for (int i = 0; i< Menus.Length; i++) 
				{
					Menus[i].SetActive(false);
				}

			if (IDMenu == 0) // chgeck if mainmenu
			{
				Menus[IDItem].SetActive(true); // activate submenus

			}
			else
			{
				// very item in submenus restart main menu
				Menus[0].SetActive(true);
			}
			

		}

		public void Start()
		{
			// show Mainmenu;
			HandleMenus(0, 0);

		}

		#endregion // METHODS
	}
}
