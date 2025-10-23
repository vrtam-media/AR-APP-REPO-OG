// ----------------------------------------------------------------------
// File: 			SceneSelectionButtonController
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Virtence.VText.Demo
{
	/// <summary>
	/// 
	/// </summary>
	public class SceneSelectionButtonController : MonoBehaviour, IPointerClickHandler
	{
		#region EVENTS
		public event EventHandler<GenericEventArgs<int>> ButtonClicked;             // this is called if we click this button (it sends the scene index)
        #endregion // EVENTS

        #region EXPOSED
        [Tooltip("the label of the scene")]
		public Text NameLabel;                      // the label of the scene
		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		private int _sceneIndex = -1;               // the index of the scene which should be loaded
		private string _name;                       // the name for the button
		#endregion // FIELDS

		#region PROPERTIES
        public int SceneIndex {
			get { return _sceneIndex; }
			set { _sceneIndex = value; }
        }

        public string Name {
			get { return _name; }
			set {
				_name = value;
				if (NameLabel != null) {
					NameLabel.text = value;
                }
            }
        }
		#endregion // PROPERTIES

		#region METHODS

		public void Start()
		{

		}

        /// <summary>
        /// start loading the specified scene
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
			ButtonClicked.Invoke(this, new GenericEventArgs<int>(_sceneIndex));
		}

        #endregion // METHODS
    }
}
