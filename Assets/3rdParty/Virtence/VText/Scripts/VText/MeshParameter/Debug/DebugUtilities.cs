// ----------------------------------------------------------------------
// File: 			DebugUtilities
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2017 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using UnityEngine;

namespace Virtence.VText.DebugUtilities
{
	/// <summary>
	/// utility class for drawing debug informations
	/// </summary>
	public class DebugUtilities 
	{
		#region EVENTS
		#endregion // EVENTS


		#region CONSTANTS

		#endregion // CONSTANTS


		#region FIELDS
		private float _sphereRadius = 1.0f;         // the radius of debug spheres
		#endregion // FIELDS


		#region PROPERTIES
        public float SphereRadius {
			get { return _sphereRadius; }
			set { _sphereRadius = value; }
        }
		#endregion // PROPERTIES


		#region CONSTRUCTORS
		#endregion // CONSTRUCTORS


		#region METHODS
		public void DrawSphere(Vector3 worldPosition) {
        }
		#endregion // METHODS


		#region EVENT HANDLERS
		#endregion // EVENT HANDLERS
	}
}