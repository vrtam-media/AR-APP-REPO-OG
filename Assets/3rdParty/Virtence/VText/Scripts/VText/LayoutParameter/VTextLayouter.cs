using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Virtence.VText
{
    internal class VTextLayouter
    {
        #region FIELDS

        /// <summary>
        /// VText root transform
        /// </summary>
        private readonly Transform _root;

        /// <summary>
        /// Layout params
        /// </summary>
        private readonly VTextLayoutParameter _layout;

        /// <summary>
        /// used for circular bending
        /// </summary>
        private readonly float _depth;

        #endregion FIELDS

        #region CONSTRUCTOR

        public VTextLayouter(Transform root, VTextLayoutParameter layout, float depth = 0.0f)
        {
            _root = root;
            _layout = layout;
            _depth = depth;
        }

        #endregion CONSTRUCTOR

        #region METHODS

		public void Layout(string text, List<Transform> glyphs, IVFont font, bool layoutOnly = false)
		{
			// no text? no pain, no game
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			List<string> lines = new List<string>();

			using (var reader = new StringReader(text))
			{
				while (reader.Peek() > 0)
				{
					lines.Add(reader.ReadLine());
				}
			}

			/*
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = 0; j < lines[i].Length; j++)
                {
                    Debug.Log("Line " + i + ": " + lines[i] + " j: " + j + " " + lines[i][j]);
                }
            }
            */

			//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
			//--- 1. Calculate Line width and height for each line to determine maxima
			//--------------------------------------------------------------------------------------------------------------------------------------------------------------------


			// Debug.Log("Font info create text " + str);
			float xoffset = 0.0f;
			float yoffset = 0.0f;
			float ascender = font.Ascender * font.ScaleToPixelOffset;

			//float descender = GetDescender(m_fontHandle);
			//Debug.Log(ascender.ToString("F5") + " descender " + descender.ToString("F5"));
			float xSpacing = ascender * _layout.Spacing;
			float ySpacing = 0.0f;
			// Debug.Log("*** yoff " + yoffset + " " + ySpacing);

			//Debug.Log(sa.Length + " lines in text " + str);
			//Vector2 glyphShift = new Vector2(-typeface.Bounds.XMin, -typeface.Bounds.YMin);
			//Vector2 maxGlyphSize = new Vector2(typeface.Bounds.XMax - typeface.Bounds.XMin, typeface.Bounds.YMax - typeface.Bounds.YMin);
			// calculate linesizes and total
			Vector2[] linesizes = new Vector2[lines.Count];
			float maxWidth = 0.0f;
			float maxHeight = 0.0f;

			float kern = 0.0f;
			int charCount = 0;
			Vector3 locpos = new Vector3(0f, 0f, 0f);
			// calc bounds
			for (int k = 0; k < lines.Count; k++)
			{
				linesizes[k] = CalculateLineSize(lines[k], font);
				if (lines[k].Length > 0)
				{
					float h = linesizes[k].y * _layout.Spacing;
					if (h > ySpacing)
					{
						ySpacing = h;
						//Debug.Log(k + " *** ysize " + linesizes[k].y + " " + ySpacing);
					}
				}
				if (_layout.IsHorizontal)
				{
					if (linesizes[k].x > maxWidth)
					{
						maxWidth = linesizes[k].x;
					}
					maxHeight += linesizes[k].y;
				}
				else
				{
					maxWidth += linesizes[k].x;
					if (linesizes[k].y > maxHeight)
					{
						maxHeight = linesizes[k].y;
					}
				}
			}
			//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
			//--- 2. Calculate Offset for Alignment
			//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

			float startX = 0.0f;
			int whiteSpaceCounterFromLastLine = 0;
			//for each line
			for (int k = 0; k < lines.Count; k++)
			{
				if (_layout.IsHorizontal)
				{
					switch (_layout.Major)
					{
						case Align.Base:
						case Align.Start:
						case Align.Justified:
							xoffset = 0.0f;
							break;

						case Align.Center:
							xoffset = -linesizes[k].x * 0.5f;
							startX = -maxWidth * 0.5f;
							break;

						case Align.End:
							xoffset = -linesizes[k].x;
							startX = -maxWidth;
							break;
					}
					switch (_layout.Minor)
					{
						case Align.Base:
							yoffset = 0.0f;
							break;

						case Align.Center:
							yoffset = maxHeight * 0.5f - ascender;
							break;

						case Align.End:
							yoffset = maxHeight - ascender;
							break;

						case Align.Start:
						case Align.Justified:
							yoffset = -ascender * _layout.Size;
							break;
					}
					yoffset -= k * ySpacing;
					//Debug.Log(k + " yoffset " + yoffset + " " + ySpacing);
				}
				else
				{ // vertical
					switch (_layout.Major)
					{
						case Align.Base:
						case Align.Start:
						case Align.Justified:
							yoffset = 0.0f;
							break;

						case Align.Center:
							yoffset = linesizes[k].y * 0.5f;
							break;

						case Align.End:
							yoffset = linesizes[k].y - ascender;
							break;
					}
					switch (_layout.Minor)
					{
						case Align.Base:
						case Align.Start:
						case Align.Justified:
							xoffset = 0.0f;
							break;

						case Align.Center:
							xoffset = -maxWidth * 0.5f;
							break;

						case Align.End:
							xoffset = -maxWidth;
							break;
					}
					xoffset += k * xSpacing;
					//Debug.Log(k +  " xoff " + xoffset + " spacing: " + xSpacing + " yOffset: " + yoffset);
				}


				//--------------------------------------------------------------------------------------------------------------------------------------------------------------------
				//--- 3. Align glyphs along line
				//--------------------------------------------------------------------------------------------------------------------------------------------------------------------

				char c;
				char prev = '\0';
				float adjustX = 0.0f;
				int whiteSpaceCounter = 0;

				for (int j = 0; j < lines[k].Length; j++)
				{
					c = lines[k][j];

					if (c >= ' ')
					{
						if (!_layout.IsHorizontal)
						{
							// adjust x to center of glyph
							//adjustX = (((xSpacing - _layout.Size) * 0.5f) - typeface.GetHFrontSideBearingFromGlyphIndex(c)) * _layout.Size;
							adjustX = font.GetAdvance(c) * -0.5f * _layout.Size * font.ScaleToPixelOffset;
							//Debug.Log(typeface.GetHAdvanceWidthFromGlyphIndex(c) + " adjust " + adjustX + " sz " + _layout.Size);
						}
						if ('\0' != prev)
						{
							if (_layout.IsHorizontal)
							{
                                // only horizontal kerning adjust!
                                kern = font.GetKernDistance(prev, c) * _layout.Size * font.ScaleToPixelOffset;
                                //Debug.LogFormat("Kern ({0}|{1}): {2}", prev, c, kern);
                            }
						}
						if (c > ' ')
						{
							// no GameObject for space
							string gname = c + "_" + k + "_" + j;
							GameObject go = null;
							Transform trans = null;
							if (layoutOnly)
							{
								trans = glyphs[charCount - whiteSpaceCounter - whiteSpaceCounterFromLastLine];
								go = trans.gameObject;
								go.hideFlags = HideFlags.DontSave;
							}
							else
							{
								go = new GameObject(gname);
								go.hideFlags = HideFlags.DontSave;
								trans = go.transform;
								trans.SetParent(_root, false);
								glyphs.Add(trans);
							}

							locpos.x = xoffset + adjustX + kern;
							float normX = (maxWidth > 0f) ? (locpos.x - startX) / maxWidth : 0f;

							// Debug.Log("normX: " + normX + " startX " + startX + " locX " + locpos.x + " maxw " + maxWidth);
							float y0 = _layout.CurveXY.Evaluate(normX);
                            locpos.y = yoffset + y0;// + kern;
                            float z0 = _layout.CurveXZ.Evaluate(normX);
							locpos.z = z0;
							Quaternion orient = new Quaternion(0f, 0f, 0f, 1f);
							const float txDelta = 0.01f;
							float x1 = normX + txDelta;
							if (maxWidth > 0.0f)
							{
								if (_layout.OrientationXY)
								{
									float y1 = _layout.CurveXY.Evaluate(x1);
									float dY = (y1 - y0) / maxWidth;
									float rotZ = Mathf.Atan2(dY, txDelta);
									orient *= Quaternion.AngleAxis(Mathf.Rad2Deg * rotZ, new Vector3(0f, 0f, 1f));
								}
								if (_layout.OrientationXZ)
								{
									float z1 = _layout.CurveXZ.Evaluate(x1);
									float dZ = (z1 - z0) / maxWidth;
									float rotY = Mathf.Atan2(dZ, txDelta);
									orient *= Quaternion.AngleAxis(Mathf.Rad2Deg * rotY, new Vector3(0f, -1f, 0f));
								}
								if (_layout.OrientationCircular)
								{
									normX *= maxWidth;
									normX += font.GetAdvance(c) * font.ScaleToPixelOffset * _layout.Size * 0.5f;
									normX /= maxWidth;

									float angle = Mathf.Lerp(-_layout.StartRadius, -_layout.EndRadius, normX);
									Quaternion rot = Quaternion.AngleAxis(angle, new Vector3(0f, 1f, 0f));

									//Debug.Log("NormX: " + normX + " Angle: " + angle);

									locpos.x = 0.0f;
									float fac = _layout.CircleRadius + _depth * _layout.Size;
									if (_layout.AnimateRadius)
									{
										fac *= _layout.CurveRadius.Evaluate(normX);
									}
									locpos += (rot * new Vector3(0.0f, 0.0f, -1.0f)) * fac;
									// locpos += (rot * new Vector3(-typeface.GetHAdvanceWidthFromGlyphIndex(c) * STRANGE_MAGIC_NUMBER * _layout.Size * 0.5f, 0.0f, 0.0f));
									orient *= rot;
								}
							}
							trans.localPosition = locpos;

							trans.localRotation = orient;
							//trans.localScale = new Vector3(_layout.Size, _layout.Size, _layout.Size);
						}
						else
						{
							whiteSpaceCounter++;
						}

						charCount++;
						prev = c;
						float spaceOffset = 0f;
						if (_layout.IsHorizontal)
						{
							if (_layout.Major == Align.Justified)
							{
								if (c == ' ')
								{
									int spaceCount = lines[k].Count(char.IsWhiteSpace);
									if (linesizes[k].x < maxWidth)
									{
										spaceOffset = (maxWidth - linesizes[k].x) / spaceCount;
									}
								}
							}

							float hAdvance = font.GetAdvance(c);

							xoffset += hAdvance * font.ScaleToPixelOffset * _layout.Size + _layout.GlyphSpacing + spaceOffset;
						}
						else
						{
							if (_layout.Major == Align.Justified)
							{
								if (c == ' ')
								{
									int spaceCount = lines[k].Count(char.IsWhiteSpace);
									if (linesizes[k].y < maxHeight)
									{
										spaceOffset = (maxHeight - linesizes[k].y) / spaceCount;
									}
								}
							}

							// yoffset -= font.CalculateRecommendedLineSpacing() * font.ScaleToPixelOffset * _layout.Size + _layout.GlyphSpacing + spaceOffset;
							yoffset -= (font.Ascender - font.Descender) * font.ScaleToPixelOffset * _layout.Size + _layout.GlyphSpacing + spaceOffset;
							adjustX = 0.0f;
						}
					}
				}
				whiteSpaceCounterFromLastLine += whiteSpaceCounter;
			}
		}

		/// <summary>
		/// calculate the size of a string with the specified fonttype
		/// </summary>
		/// <param name="line"></param>
		/// <param name="font"></param>
		/// <returns></returns>
		private Vector2 CalculateLineSize(string line, IVFont font)
		{
			float xmax = 0.0f;
			float ymax = 0.0f;
			float lw = 0.0f;
			float lh = 0.0f;
			if (_layout.IsHorizontal)
			{
				float kern = 0.0f;// = new Vector2(0f, 0f);
				char prev = '\0';
				for (int i = 0; i < line.Length; i++)
				{
					char c = line[i];

					if ('\0' != prev)
					{
                        kern = font.GetKernDistance(prev, c) * _layout.Size * font.ScaleToPixelOffset;
                        //Debug.LogFormat("Kern ({0}|{1}): {2}", prev, c, kern);
                    }

					if (i < line.Length)
					{
						float hAdvance = font.GetAdvance(c);
						lw += hAdvance;
					}
					else
					{
						lw += _layout.Size + kern;
					}
					lh = font.Ascender - font.Descender;
					// lh = font.CalculateRecommendedLineSpacing();
					if (ymax < lh)
					{
						ymax = lh;
					}
					prev = c;
				}
				//Debug.Log(" width " + lw);
				if (lw > xmax)
				{
					xmax = lw;
				}
			}
			else
			{
				// vertical layout
				for (int i = 0; i < line.Length; i++)
				{
					// lh += font.CalculateRecommendedLineSpacing();
					lh += font.Ascender - font.Descender;
				}
				ymax = lh;
				lw = font.Ascender;

				//Debug.Log(" Ascender width " + lw);
				xmax = lw;
			}
			Vector2 result = new Vector2(xmax * _layout.Size, ymax * _layout.Size) * font.ScaleToPixelOffset;
			if (line.Length > 1)
			{
				result.x += (line.Length - 1) * _layout.GlyphSpacing;
			}
			return result;
		}
        #endregion METHODS
    }
}