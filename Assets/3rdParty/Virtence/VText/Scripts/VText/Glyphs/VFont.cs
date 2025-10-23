// ----------------------------------------------------------------------
// File: 			VFont
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
	/// VText representation of a OpenTypeCS Font object with glyph hash.
	/// </summary>
	internal class VFont : IVFont
	{
		#region FIELDS
		private readonly Font _font;								// the typeface object
        private readonly float _scaleToPixelOffset;					// the scale to pixel offset
		private Dictionary<char, MeshAttributes> _glyphHash;		// the hash for previous calculated glyph mesh attribues
		#endregion // FIELDS


		#region PROPERTIES
		public Font Font
        {
			get { return _font; }
		}


		/// <summary>
		/// get the ascender
		/// </summary>
		/// <returns></returns>
		public short Ascender
		{
			get
			{
				short result = 0;
				if (_font != null)
				{
					result = _font.Ascender;
				}
				return result;
			}
		}


		/// <summary>
		/// get the descender
		/// </summary>
		/// <returns></returns>
		public short Descender
		{
			get
			{
				short result = 0;
				if (_font != null)
				{
					result = _font.Descender;
				}
				return result;
			}
		}

		public float ScaleToPixelOffset
        {
            get { return _scaleToPixelOffset; }
        }


		Dictionary<char, MeshAttributes> IVFont.GlyphMeshAttributesHash
		{
			get
			{
				return _glyphHash ?? (_glyphHash = new Dictionary<char, MeshAttributes>());
			}
			set
			{
				_glyphHash = value;
			}
		}
		#endregion // PROPERTIES


		#region CONSTRUCTORS
		public VFont(Font font) {
			_font = font;
            _scaleToPixelOffset = 1.3333333f / _font.UnitsPerEm;
            //UnityEngine.Debug.Log("Scale: " + _scaleToPixelOffset);
        }
		#endregion // CONSTRUCTORS


		#region METHODS

		/// <summary>
		/// get the horizontal advance for the specified character
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public float GetAdvance(char c)
		{
			float advance = 0.0f;

			if (_font != null)
			{
				advance = _font.CharToGlyph(c).AdvanceWidth;
			}

			return advance;
		}

		/// <summary>
		/// get the recommended spacing between two lines
		/// </summary>
		/// <returns></returns>
		public int CalculateRecommendedLineSpacing()
		{
			int result = 0;
			if (_font != null)
			{
				result = _font.Ascender - _font.Descender;
			}
			return result;
		}

		/// <summary>
		/// get the kerning distance between the specified characters
		/// </summary>
		/// <param name="leftChar"></param>
		/// <param name="rightChar"></param>
		/// <returns></returns>
		public int GetKernDistance(char leftChar, char rightChar) {
			var result = 0;
			if (_font != null)
			{
				result = _font.GetKerningValue(leftChar, rightChar);
            }

			return result;
		}

		/// <summary>
		/// get the glyph for the specified character
		/// </summary>
		/// <param name="c"></param>
		/// <returns>OpenTypeCS Glyph object</returns>
		public Glyph GetGlyph(char c) {
			Glyph result = null;
			if (_font != null)
			{
				result = _font.CharToGlyph(c);
			}
			return result;
		}
		#endregion // METHODS
	}
}