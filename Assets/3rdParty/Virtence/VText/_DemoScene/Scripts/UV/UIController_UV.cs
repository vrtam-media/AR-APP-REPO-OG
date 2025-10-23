// ----------------------------------------------------------------------
// File: 		UIController
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2014 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Virtence.VText.Demo {
	/// <summary>
	/// control the main ui in the start scene
	/// </summary>
	public class UIController_UV : MonoBehaviour 
	{	

		#region EXPOSED 
	    [Tooltip("The wrapper for the VTextInterface")]
	    public VtextHandler_UV VTextController;                    // the wrapper for the VTextInterface

	    //[Tooltip("The dropdown which holds the available fonts")]
	    //public Dropdown FontDropdown;                           // the dropdown which holds the available fonts

	    [Header("Common")]
	    [Tooltip("The label which shows the current font name")]
	    public Text FontNameLabel;                              // the label which shows the current font name

	    [Tooltip("The slider which shows the bevel value")]
	    public Slider DepthSlider;                              // the slider which shows the depth value

	    [Tooltip("The slider which shows the bevel value")]
	    public Slider BevelSlider;                              // the slider which shows the bevel value



	    #endregion // EXPOSED


		#region CONSTANTS

		#endregion // CONSTANTS


		#region FIELDS
	    private int _currentFontIndex;      // the index of the current used font
		#endregion // FIELDS


		#region PROPERTIES

		#endregion // PROPERTIES


		#region METHODS
		
		// initialize
		void Awake() 
		{
	        VTextController.FontNameChanged  += OnFontNameChanged;
	        VTextController.BevelValueChanged += OnBevelChanged;
	}

	    /// <summary>
	    /// initialize
	    /// </summary>
	    void Start() {
	        //FontDropdown.ClearOptions();
	        //FontDropdown.AddOptions(VTextInterface.GetAvailableFonts());
	    }

	    /// <summary>
	    /// Selects the next font.
	    /// </summary>
	    public void SelectNextFont() {
	        SetFontByIndex(_currentFontIndex + 1);

	    }

	    /// <summary>
	    /// Selects the previous font.
	    /// </summary>
	    public void SelectPreviousFont() {
	        SetFontByIndex(_currentFontIndex - 1);
	    }

	    /// <summary>
	    /// set the depth of the VText
	    /// </summary>
	    /// <param name="value">Value.</param>
	    public void SetSize(float value) {        
	        VTextController.SetSize(Mathf.Clamp01(value));
	    }
		
	    /// <summary>
	    /// set the depth of the VText
	    /// </summary>
	    /// <param name="value">Value.</param>
	    public void SetDepth(float value) {
	        VTextController.SetDepth(Mathf.Clamp01(value));
	    }

	    /// <summary>
	    /// set the depth of the VText
	    /// </summary>
	    /// <param name="value">Value.</param>
	    public void SetBevel(float value) {
	        VTextController.SetBevel(Mathf.Clamp01(value));
	    }

		/// <summary>
		/// set use face uvs of the VText
		/// </summary>
		/// <param name="value"></param>
		public void SetUseFaceUVs(bool value) {
			VTextController.SetUseFaceUVs(value);
		}
	    
		/// <summary>
	    /// set the font by specifying an index (the index int the AvailableFonts)
	    /// </summary>
	    /// <param name="index">Index.</param>
	    private void SetFontByIndex(int index) {
	        List<string> availableFonts = VTextFontHash.GetAvailableFonts();

	        _currentFontIndex = index;

	        if (index < 0) {
	            _currentFontIndex = availableFonts.Count - 1;
	        }

	        if (index >= availableFonts.Count) {
	            _currentFontIndex = 0;
	        }

	        VTextController.SetFont(availableFonts[_currentFontIndex]);
	    }

		#endregion // METHODS

		#region EVENTHANDLERS
	    /// <summary>
	    /// this is called if the current used fontname changes
	    /// </summary>
	    /// <param name="sender">Sender.</param>
	    /// <param name="e">E.</param>
	    void OnFontNameChanged(object sender, GenericEventArgs<string> e)
	    {

	        _currentFontIndex = VTextFontHash.GetAvailableFonts().IndexOf(e.Value);
	        FontNameLabel.text = string.Format("{0} ({1}/{2})", e.Value, _currentFontIndex + 1, VTextFontHash.GetAvailableFonts().Count);
	    }


	    /// <summary>
	    /// this is called if the bevel value of the vtext object changes
	    /// </summary>
	    /// <param name="sender">Sender.</param>
	    /// <param name="e">E.</param>
	    void OnBevelChanged(object sender, GenericEventArgs<float> e)
	    {
	        BevelSlider.value = e.Value;
	    }


		#endregion // EVENTHANDLERS
	}
}