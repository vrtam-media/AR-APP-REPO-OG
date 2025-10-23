// ----------------------------------------------------------------------
// File: 			IDHandler
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace Virtence.VText.Demo
{
	/// <summary>
	/// 
	/// </summary>
	public class MenuLineHandler : MonoBehaviour
	{
		#region EXPOSED 
		[Tooltip("the animation duration for the particles")]
		public float ParticleAnimationDuration = 1.5f;              // the animation duration for the particles
		public float ParticleOffset = 0.0f;
		public GameObject[] Particles;

		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		private int _id = 0;

		private float _fontSize;
		#endregion // FIELDS

		#region PROPERTIES
		public int ID {
			get { return _id; }
			set { _id = value; }
		}

		public float FontSize {
			get { return _fontSize; }
		}
		#endregion // PROPERTIES

		#region METHODS	
		public void Start()
		{
			_fontSize = GetComponentInChildren<VText>().LayoutParameter.Size;

		}

		/// <summary>
		/// Calculate bounds of entrie
		/// </summary>
		/// <returns></returns>
		public Bounds GetBounds(MeshRenderer[] renderers)
		{
			Bounds bounds = new Bounds();
			if (renderers.Length > 0)
			{
				//Find first enabled renderer to start encapsulate from it
				foreach (Renderer renderer in renderers)
				{
					if (renderer.enabled)
					{
						bounds = renderer.bounds;
						break;
					}
				}
				//Encapsulate for all renderers
				foreach (Renderer renderer in renderers)
				{
					if (renderer.enabled)
					{
						bounds.Encapsulate(renderer.bounds);
					}
				}
			}
			Debug.Log("BoundsSize(" + ID + "): " + bounds.size);
			Debug.Log("BoundsCenter(" + ID + "): " + bounds.center);
			return bounds;
		}

		/// <summary>
		/// create one collider for all glyphs of menu entry and calculate position of collider
		/// </summary>
		public void InitializeItem()
		{
			MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

			if (renderers.Length > 0) {
				//add colider to menu entry
				Debug.Log("******* add box collider: " + gameObject.name);
				BoxCollider bc = GetComponent<BoxCollider>();
				if (bc == null) {
					bc = this.gameObject.AddComponent<BoxCollider>();
				}
				///calc position
				Bounds bounds = GetBounds(renderers);
				bc.size = bounds.size;
				bc.center = new Vector3(0, bounds.size.y * .25f, bounds.center.z); // center boxcollider
				bc.isTrigger = true;            // enable Triger 

				//calc position of particles
				if(Particles != null)
				{
					Particles[0].transform.localPosition = new Vector3(bounds.extents.x + ParticleOffset, bounds.size.y * .25f, bounds.center.z);
					Particles[1].transform.localPosition = new Vector3(-bounds.extents.x - ParticleOffset, bounds.size.y * .25f, bounds.center.z);

				}
				
			}
		}


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

		/// <summary>
		/// play the particle effect and change the menu afterwards
		/// </summary>
		/// <returns></returns>
		private IEnumerator ChangeMenu() {
			ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in particles) {
				particleSystem.Play();
			}
			GetComponentInParent<MenuLogic>().PlaySelectionSound();
			yield return new WaitForSeconds(ParticleAnimationDuration);
			GetComponentInParent<MenuLogic>().HandleMenus(GetComponentInParent<MenuController>().MenuID, GetComponentInParent<MenuLineHandler>().ID + 1);
		}

		void OnMouseDown()
		{
			StopCoroutine(ChangeMenu());
			StartCoroutine(ChangeMenu());
			//this.gameObject.GetComponentInChildren<VText>().PhysicsParameter.RigidbodyUseGravity = true;
		}

		#endregion // METHODS
	}
}
