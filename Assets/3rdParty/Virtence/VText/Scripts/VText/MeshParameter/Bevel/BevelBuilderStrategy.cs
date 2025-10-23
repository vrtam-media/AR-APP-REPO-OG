// ----------------------------------------------------------------------
// File: 			BevelBuilderStrategy
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2020 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System.Collections.Generic;

namespace Virtence.VText
{
	/// <summary>
	/// Base class for all bevel strategies.
	/// </summary>
	internal abstract class BevelBuilderStrategy
	{
        #region EVENTS

        /// <summary>
        /// 1.4142135 * _meshParameter.Bevel
        /// </summary>
        protected readonly float ROOT_TWO_TIMES_BEVEL;

		#endregion // EVENTS



		#region PROPERTIES
        /// <summary>
        /// Cached mesh parameter
        /// </summary>
		protected VTextMeshParameter _meshParameter;

		#endregion // PROPERTIES

		#region CONSTRUCTORS

		internal BevelBuilderStrategy(VTextMeshParameter meshParameter)
		{
			_meshParameter = meshParameter;
			ROOT_TWO_TIMES_BEVEL = 1.4142135f * _meshParameter.Bevel;
		}

		#endregion // CONSTRUCTORS


		#region METHODS

		/// <summary>
		/// Create front face bevels from mesh and contour attributes.
		/// </summary>
		/// <param name="meshAttribs"></param>
		internal abstract void AddBevelFrontFacesToMesh(ref MeshAttributes meshAttribs);

		/// <summary>
		/// Create back face bevels from mesh and contour attributes.
		/// </summary>
		/// <param name="meshAttribs"></param>
		internal abstract void AddBevelBackfacesToMesh(ref MeshAttributes meshAttribs);

		#endregion // METHODS
	}
}
