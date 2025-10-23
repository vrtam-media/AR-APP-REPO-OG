// ----------------------------------------------------------------------
// File: 			ITesselator
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------


using System.Collections.Generic;

namespace Virtence.VText.Tesselation
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ITesselator 
	{
		#region EVENTS

		#endregion // EVENTS


		#region PROPERTIES
		List<Contour> Contours { get; }								// the list of contours which should be tesselated
		#endregion // PROPERTIES


		#region METHODS
		void Cleanup();												// cleanup the tesselator
		void AddContour(Contour contour);							// add the given contour to the tesselator
		void Tesselate();											// tesselate the specified glyph
		#endregion // METHODS	
	}
}