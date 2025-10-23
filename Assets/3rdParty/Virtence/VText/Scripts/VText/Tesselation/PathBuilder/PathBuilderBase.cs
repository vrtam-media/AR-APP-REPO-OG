// ----------------------------------------------------------------------
// File: 			PathBuilderBase
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using System.Collections.Generic;
using Virtence.OpenTypeCS;

namespace Virtence.VText.Tesselation
{
	/// <summary>
	/// 
	/// </summary>
	internal class PathBuilderBase : IPathBuilder
	{
		#region EVENTS
		#endregion // EVENTS

		#region CONSTANTS
		#endregion // CONSTANTS

		#region FIELDS
		protected IVFont _font;					// the font interface
		protected List<Contour> _contours;		// the contour definitions for the glyphs
		#endregion // FIELDS

		#region PROPERTIES
		#endregion // PROPERTIES

		#region CONSTRUCTORS
		public PathBuilderBase(IVFont font) {
			_font = font;
			_contours = new List<Contour>();

		}

        
        #endregion // CONSTRUCTORS

        #region METHODS
        /// <summary>
        /// create contour informations from the given glyph
        /// </summary>
        /// <param name="glyph"></param>
        public virtual void CreateContoursFromGlyph(Glyph glyph)
		{
			_contours = new List<Contour>();
		}

        /// <summary>
        /// create the necessary informations for the tesselator
        /// </summary>
        /// <param name="tesselator"></param>
		public virtual void ApplyTesselator(ref GlyphContourTesselator tesselator, Glyph glyph)
		{
			// throw new System.NotImplementedException();
		}
		#endregion // METHODS

		#region EVENT HANDLERS
		#endregion // EVENT HANDLERS
	}
}
