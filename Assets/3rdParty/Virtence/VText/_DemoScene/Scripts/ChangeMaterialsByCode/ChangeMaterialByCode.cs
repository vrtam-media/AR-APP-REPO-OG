// ----------------------------------------------------------------------
// File: 			ChangeMaterialTest
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;

namespace Virtence.VText.Demo
{
    /// <summary>
    /// change the vtext material by script
    /// </summary>
    public class ChangeMaterialByCode : MonoBehaviour
	{
		#region EXPOSED
		[Tooltip("the main vtext component")]
		public VText VTextParent;							// the main vtext component

		[Tooltip("the materials which should be set at startup")]
		public Material[] textMaterials;					// the materials which should be set at startup

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
			VTextParent.TextRenderingFinished += OnTextRenderingFinished;

			VTextParent.Rebuild();
		}

		/// <summary>
        /// change the materials of the vtext to the specified ones
        /// </summary>
		private void ChangeMaterial() {
			if (VTextParent != null)
			{
				if (VTextParent.RenderParameter.Materials != textMaterials)
				{
					VTextParent.RenderParameter.Materials = textMaterials;
				}
			}
		}
		#endregion // METHODS

		#region EVENTHANDLER
		/// <summary>
        /// this is called if vtext finished its rendering
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnTextRenderingFinished(object sender, System.EventArgs e)
		{
			Loom.QueueOnMainThread(() => {
				ChangeMaterial();
			});

		}
		#endregion // EVENTHANDLER
	}
}
