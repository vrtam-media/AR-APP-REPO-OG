// ----------------------------------------------------------------------
// File: 			MenuItemController
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;

namespace Virtence.VText.Demo
{
	/// <summary>
	/// 
	/// </summary>
	public class MenuBehaviour: MonoBehaviour
	{
		#region EXPOSED 
		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		#endregion // FIELDS

		#region PROPERTIES
		#endregion // PROPERTIES

		#region METHODS

		public void Start()
		{
		}


		/// <summary>
		/// this is called if the mouse enters the glyph
		/// </summary>
		void OnMouseEnter()
		{
			///send ID from selectet Item to MenuController
			GetComponentInParent<MenuController>().SetSelectedMenuItem(GetComponentInParent<MenuLineHandler>().ID);

		}

		/// <summary>
		/// this is called if the mouse leaves the glyph
		/// </summary>
		void OnMouseExit()
		{		
			///send reset value for selected Item to MenuController
			GetComponentInParent<MenuController>().SetSelectedMenuItem(-1);
			
		}

		void OnMouseDown()
		{
			GetComponentInParent<MenuController>().SetSelectedMenuItem(GetComponentInParent<MenuLineHandler>().ID);
		}

	


		#endregion // METHODS
	}
}
