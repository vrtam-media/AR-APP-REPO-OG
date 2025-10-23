// ----------------------------------------------------------------------
// File: 			MenueController
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Virtence.VText.Demo
{
	/// <summary>
	/// 
	/// </summary>
	public class MenuController : MonoBehaviour
	{
		public int MenuID = 0;
		#region EXPOSED
		public string[] MenuContent;
		public float LineOffset = -2.0f;

		// FontSize
		public bool ActiveEntry_ChangeFontSize = true;
		public float FontsizeDeactivated = 1.0f;
		public float FontsizeActivated = 2.0f;

		//GlyphSpacing
		public bool ActiveEntry_ChangeGlyphSpacing = true;
		public float GlyphSpacingDeactivated = 0.0f;
		public float GlyphSpacingActivated = 1.0f;

		//Material
		public bool ActiveEntry_ChangeMaterial = true;
		public Material FaceMaterialDeactivated;
		public Material BevelMaterialDeactivated;
		public Material SideMaterialDeactivated;
		public Material FaceMaterialActivated;
		public Material BevelMaterialActivated;
		public Material SideMaterialActivated;

		public GameObject MenuItemPrefab;



		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		private int SelectedMenuItem = -1;
		private List<GameObject> MenuEntries = new List<GameObject>();

		#endregion // FIELDS

		#region PROPERTIES
		#endregion // PROPERTIES

		#region METHODS

		public void Start()
		{
			CreateMenu();
		}

		/// <summary>
		/// creat menu and id for each entry
		/// </summary>
		private void CreateMenu()
		{
			if (MenuContent != null && MenuItemPrefab != null)
			{
				//create menu, instantiate menuItem prefab
				for (int i = 0; i < MenuContent.Length; i++)
				{
					string str = MenuContent[i];
					GameObject obj = Instantiate(MenuItemPrefab, this.transform) as GameObject;
					MenuEntries.Add(obj);

					// add menue entries to list
					obj.transform.localPosition = new Vector3(0, LineOffset * i, 0);
					obj.GetComponentInChildren<VText>().SetText(str);
					obj.GetComponentInChildren<MenuLineHandler>().ID = i;

					obj.GetComponentInChildren<VText>().TextRenderingFinished += MenuController_TextRenderingFinished;

					// layout
					obj.GetComponentInChildren<VText>().LayoutParameter.GlyphSpacing = GlyphSpacingDeactivated;
					obj.GetComponentInChildren<VText>().LayoutParameter.Size = FontsizeDeactivated;

					Material[] m_deactive = new Material[3];
					m_deactive[0] = FaceMaterialDeactivated;
					m_deactive[1] = SideMaterialDeactivated;
					m_deactive[2] = BevelMaterialDeactivated;
					obj.GetComponentInChildren<VText>().RenderParameter.Materials = m_deactive;
				}
			}
		}

		private void MenuController_TextRenderingFinished(object sender, System.EventArgs e)
		{			
			VText vtext = sender as VText;
			if (vtext != null) {
				MenuLineHandler menuLineHandler = vtext.gameObject.GetComponentInParent<MenuLineHandler>();
				if (menuLineHandler != null) {
					Debug.Log("initialize items");
					menuLineHandler.InitializeItem();
				}
			}
			//obj.GetComponentInChildren<MenuLineHandler>().InitializeItem();
		}

		IEnumerator delayedAction(UnityAction action)
		{

			yield return null; // optional
			yield return new WaitForEndOfFrame(); // Wait for the next frame
	;
			action.Invoke(); // execute a delegate
		}


		/// <summary>
		/// select a menuItem
		/// </summary>
		/// <param name="id"></param>
		public void SetSelectedMenuItem(int id) {
			//check if a new menuItem is selected
			if(SelectedMenuItem != id)
			{
				SelectedMenuItem = id;
				UpdateMenu();
			}
		
		}


		/// <summary>
		/// Update Menu
		/// </summary>
		private void UpdateMenu()
		{
			Debug.Log("Update Menu");
			if (ActiveEntry_ChangeFontSize)
				MenuHandlingChangeFontSize();

			if (ActiveEntry_ChangeGlyphSpacing)
				MenuHandlingChangeGlyphSpacing();

			if (ActiveEntry_ChangeMaterial)
				MenuHandlingChangeMaterial();

		}

		/// <summary>
		/// Update Menu
		/// </summary>
		public void ActivateMenuItemate()
		{
			foreach (GameObject g in MenuEntries)
			{
				if (g.GetComponentInChildren<MenuLineHandler>().ID != SelectedMenuItem)
				{
					//g.GetComponentInChildren<VText>().PhysicsParameter.RigidbodyUseGravity = true;
				}
				
			}
		}

		/// <summary>
		/// change fontsize for active entry
		/// </summary>
		private void MenuHandlingChangeFontSize()
		{
			foreach (GameObject g in MenuEntries)
			{
				if (g.GetComponentInChildren<MenuLineHandler>().ID == SelectedMenuItem)
				{
					g.GetComponentInChildren<VText>().LayoutParameter.Size = FontsizeActivated;
				}
				else
				{
					g.GetComponentInChildren<VText>().LayoutParameter.Size = FontsizeDeactivated;
					}
			}
		}


		/// <summary>
		/// change glyphspace for active netry
		/// </summary>
		private void MenuHandlingChangeGlyphSpacing()
		{
			foreach (GameObject g in MenuEntries)
			{
				if (g.GetComponentInChildren<MenuLineHandler>().ID == SelectedMenuItem)
				{
					g.GetComponentInChildren<VText>().LayoutParameter.GlyphSpacing = GlyphSpacingActivated;
				}
				else
				{
					g.GetComponentInChildren<VText>().LayoutParameter.GlyphSpacing = GlyphSpacingDeactivated;
				}
			}
		}

		/// <summary>
		/// change glyphspace for active netry
		/// </summary>
		private void MenuHandlingChangeMaterial()
		{
			if (FaceMaterialActivated != null && SideMaterialActivated != null && BevelMaterialActivated != null && FaceMaterialActivated != null && SideMaterialActivated != null && BevelMaterialActivated != null)
			{
				Material[] m_active = new Material[3];
				m_active[0] = FaceMaterialActivated;
				m_active[1] = SideMaterialActivated;
				m_active[2] = BevelMaterialActivated;

				Material[] m_deactive = new Material[3];
				m_deactive[0] = FaceMaterialDeactivated;
				m_deactive[1] = SideMaterialDeactivated;
				m_deactive[2] = BevelMaterialDeactivated;

				foreach (GameObject g in MenuEntries)
				{
					if (g.GetComponentInChildren<MenuLineHandler>().ID == SelectedMenuItem)
					{
						g.GetComponentInChildren<VText>().RenderParameter.Materials = m_active;
					}
					else
					{
						g.GetComponentInChildren<VText>().RenderParameter.Materials = m_deactive;
					}
				}
			}
		}

		

	}

	#endregion // METHODS
}

