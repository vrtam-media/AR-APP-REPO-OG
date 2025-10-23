// ----------------------------------------------------------------------
// File: 			IPathBuilder
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using Virtence.OpenTypeCS;

namespace Virtence.VText.Tesselation
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IPathBuilder
	{
		#region EVENTS
		#endregion // EVENTS

		#region PROPERTIES
		#endregion // PROPERTIES

		#region METHODS
        // TODO: let us create an IGlyph definition
		void CreateContoursFromGlyph(Glyph glyph);          // create contour definitions for the specified glyph

        // TODO: we need a tesselator interface and base class
		void ApplyTesselator(ref GlyphContourTesselator tesselator, Glyph glyph);        // apply an tesselator to create mesh informations
		#endregion // METHODS
	}
}
