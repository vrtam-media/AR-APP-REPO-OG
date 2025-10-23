// ----------------------------------------------------------------------
// File: 			SceneSelectionController
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Virtence.VText.Demo
{
	/// <summary>
	/// 
	/// </summary>
	public class SceneSelectionController : MonoBehaviour
	{
		#region EXPOSED
        [Tooltip("the transform of the scrollview which shows the scene selection buttons")]
        public Transform ContentPanel;          // the transform of the scrollview which shows the scene selection buttons

        [Tooltip("the button prefab which allows us to select a new scene")]
		public GameObject SceneSelectionButtonPrefab;    // the button prefab which allows us to select a new scene

        [Tooltip("the main menu UI")]
		public CanvasGroup MainMenuUI;                  // the main menu UI

		[Tooltip("the interface which is visible if a scene is loaded (esp. the back button)")]
		public CanvasGroup InOtherScenesUI;             // the interface which is visible if a scene is loaded (esp. the back button)

		#endregion // EXPOSED

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		private int _loadedSceneIndex = -1;             // the index of the actually loaded scene
		#endregion // FIELDS

		#region PROPERTIES
		#endregion // PROPERTIES

		#region METHODS

		public void Start()
		{
			ShowMainMenu(true);
			SetupSceneLoadingButtons();
		}

        /// <summary>
        /// generate the buttons for all scenes of the scene settings 
        /// </summary>
		private void SetupSceneLoadingButtons() {
			int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

			for (int i = 1; i < sceneCount; i++) {
				GameObject go = Instantiate(SceneSelectionButtonPrefab, ContentPanel);
				UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(i);
				SceneSelectionButtonController buttonControl = go.GetComponent<SceneSelectionButtonController>();
				if (buttonControl != null && scene != null) {
					Debug.Log("try adding scene at index: " + i);
					buttonControl.SceneIndex = i;
					buttonControl.Name = GetSceneNameByIndex(i);
                    buttonControl.ButtonClicked += OnSceneLoadingButtonClick;
					Debug.Log("scene name: " + buttonControl.Name);
				}
               
            }
		}

        /// <summary>
        /// get the name of the scene (in build settings) by the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetSceneNameByIndex(int index) {
				string path = SceneUtility.GetScenePathByBuildIndex(index);
				int slash = path.LastIndexOf('/');
				string name = path.Substring(slash + 1);
				int dot = name.LastIndexOf('.');
				return name.Substring(0, dot);
		}

        /// <summary>
        /// load the scene with the specified index
        /// </summary>
        /// <param name="index"></param>
		private void LoadScene(int index)
		{
			if (_loadedSceneIndex > 0)
			{
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_loadedSceneIndex);
			}
			_loadedSceneIndex = index;
			UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_loadedSceneIndex, LoadSceneMode.Additive);

			ShowMainMenu(false);
		}

        /// <summary>
        /// show the main menu
        /// </summary>
		public void ShowMainMenu() {
			ShowMainMenu(true);
        }

        /// <summary>
        /// show the non persistant ui
        /// </summary>
		public void ShowMainMenu(bool show) {
			MainMenuUI.gameObject.SetActive(show);
			InOtherScenesUI.gameObject.SetActive(!show);
        }
	
		#endregion // METHODS

		#region EVENTHANDLER
        /// <summary>
        /// load the scene with the specified index in the build settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void OnSceneLoadingButtonClick(object sender, GenericEventArgs<int> e)
		{
			Loom.QueueOnMainThread(() => {
				LoadScene(e.Value);
            });
		}
		#endregion // EVENTHANDLER
	}
}
