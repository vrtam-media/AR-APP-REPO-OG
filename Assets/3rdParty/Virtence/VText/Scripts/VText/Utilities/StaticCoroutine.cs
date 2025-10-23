// ----------------------------------------------------------------------
// File: 			StaticCoroutine
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Michael Bißmann (michael.bissmann@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;

namespace Virtence.Common.Utilities
{
	/// <summary>
	/// Convinience class to allow the use of Coroutines in classes not derived from Monobehaviour. 
	/// Creates GameObject with StaticCoroutine component to Start Coroutines on. 
	/// Is implemented as lazy singleton so it will only be created if needed (if Instance property is called).
	/// Instance will be cached until Destroy.
	/// </summary>
	/// 

	[UnityEngine.Scripting.Preserve]
	public class StaticCoroutine : MonoBehaviour
	{
        #region FIELDS
        private static StaticCoroutine _instance;
        #endregion // FIELDS


        #region PROPERTIES
        /// <summary>
        /// Get StaticCoroutine Monobehaviour to Start/Stop Coroutines on (implemented as lazy Singleton).
        /// </summary>
        public static StaticCoroutine Instance
		{
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<StaticCoroutine>();

                if (_instance != null)
                    return _instance;

                _instance = new GameObject("Static Coroutines").AddComponent<StaticCoroutine>();

                if (_instance != null)
                    return _instance;

                Debug.LogError("StaticCoroutine.Instance: Coul not instantiate dummy object for static coroutines.");
                return null;
            }
    	}
		#endregion // PROPERTIES



		#region CONSTRUCTORS
		private StaticCoroutine() { }
        #endregion // CONSTRUCTORS



        #region METHODS
        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private void OnApplicationQuit()
        {
            Destroy(this.gameObject);
        }
        #endregion // METHODS



        #region EVENT HANDLERS

        #endregion // EVENT HANDLERS
    }
}
