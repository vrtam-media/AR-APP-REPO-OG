// ----------------------------------------------------------------------
// File: 			IVFont
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2017 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System.Collections.Generic;
using Virtence.OpenTypeCS;

namespace Virtence.VText
{
	/// <summary>
	/// Interface for OpenTypeCS
	/// </summary>
	public interface IVFont 
	{
		#region PROPERTIES
		/// <summary>
		/// the ascender 
		/// </summary>
		short Ascender { get; }

		/// <summary>
		/// the descender
		/// </summary>
		short Descender { get; }

        /// <summary>
        /// the scale to pixel offset 
        /// </summary>
        float ScaleToPixelOffset { get; }

		/// <summary>
		/// Container for previous calculated glyph mesh attribues
		/// </summary>
		Dictionary<char,MeshAttributes> GlyphMeshAttributesHash { get; set; }
		#endregion // PROPERTIES


		#region METHODS
		/// <summary>
		/// get the horizontal advance for the specified character
		/// </summary>
		/// <param name="c"> the character for which we want to horizontal advancement</param>
		/// <returns></returns>
		float GetAdvance(char c);

		/// <summary>
		/// get the kerning distance between two chararacters
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		int GetKernDistance(char left, char right);

		/// <summary>
		/// get the recommended spacing between two lines
		/// </summary>
		/// <returns></returns>
		int CalculateRecommendedLineSpacing();

		/// <summary>
		/// get the glyph object for the specified character
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		Glyph GetGlyph(char c);

		#endregion // METHODS
	}
}