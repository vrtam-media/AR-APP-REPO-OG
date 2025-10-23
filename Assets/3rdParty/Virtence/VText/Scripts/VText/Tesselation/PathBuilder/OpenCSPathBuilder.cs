// ----------------------------------------------------------------------
// File: 			OpenCSPathBuilder
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

//#define verbose

using UnityEngine;
using Virtence.OpenTypeCS;

namespace Virtence.VText.Tesselation
{
    /// <summary>
    ///
    /// </summary>
    internal class OpenCSPathBuilder : PathBuilderBase
    {
        #region CONSTRUCTORS

        public OpenCSPathBuilder(IVFont font) : base(font)
        {
        }

        #endregion CONSTRUCTORS

        #region METHODS

        /// <summary>
        /// create contours from glyphs
        /// </summary>
        /// <param name="glyph"></param>
        public override void CreateContoursFromGlyph(Glyph glyph)
        {
        }

        /// <summary>
        /// setup a tesselator for the specified glyph
        /// </summary>
        /// <param name="tesselator"></param>
        public override void ApplyTesselator(ref GlyphContourTesselator tesselator, Glyph glyph)
        {
            base.ApplyTesselator(ref tesselator, glyph);

            if (tesselator == null)
            {
                Debug.LogWarning("tesselator is null");
                return;
            }

            if (glyph == null)
            {
                Debug.LogWarning("glyph is null");
                return;
            }

            Path glyphPath = glyph.Path * _font.ScaleToPixelOffset;

            if (glyphPath == null)
            {
                Debug.LogWarning("glyphpath is null");
                return;
            }
            if (glyphPath.Commands == null)
            {
                Debug.LogWarning("glyphpath.Commands is null");
                return;
            }

            tesselator.BeginRead(0);
#if verbose
            float debugLength = 10f;
            for (var i = 1; i < glyphPath.Commands.Count; ++i)
            {
                var pref = glyphPath.Commands[i - 1];
                var curr = glyphPath.Commands[i];

                // M ... move to (create new contour?)
                // L ... line to
                // C ... bezier curve
                // Q ... quadric bezier curve
                // Z ... close

                switch (curr.Type)
                {
                    case 'M':
                        Debug.DrawLine(new Vector3(pref.X, pref.Y), new Vector3(curr.X, curr.Y), Color.grey);
                        break;

                    case 'L':
                        Debug.DrawLine(new Vector3(pref.X, pref.Y), new Vector3(curr.X, curr.Y), Color.cyan);
                        break;

                    case 'C':

                    case 'Q':
                        Debug.DrawRay(new Vector3(curr.X, curr.Y), Vector3.forward * debugLength, Color.red);
                        Debug.DrawRay(new Vector3(curr.X1, curr.Y1), Vector3.forward * debugLength, Color.green);
                        Debug.DrawRay(new Vector3(curr.X2, curr.Y2), Vector3.forward * debugLength, Color.blue);
                        break;
                }
                debugLength += 100f;
            }
#endif

            foreach (Command command in glyphPath.Commands)
            {
                // M ... move to (create new contour?)
                // L ... line to
                // C ... bezier curve
                // Q ... quadric curve
                // Z ... close

                switch (command.Type)
                {
                    case 'M':
#if verbose
                        Debug.LogFormat("MoveTo: ({0}|{1})", command.X.ToString("F5"), command.Y.ToString("F5"));
#endif
                        tesselator.MoveTo(command.X, command.Y);
                        break;

                    case 'L':
#if verbose
                        Debug.LogFormat("LineTo: ({0}|{1})", command.X.ToString("F5"), command.Y.ToString("F5"));
#endif
                        tesselator.LineTo(command.X, command.Y);
                        break;

                    case 'Q':
#if verbose
                        Debug.LogFormat("Curve3 to: ({0}|{1}), cp1 ({2}|{3}) cp2 ({4}|{5})", command.X.ToString("F5"), command.Y.ToString("F5"), command.X1.ToString("F5"), command.Y1.ToString("F5"), command.X2.ToString("F5"), command.Y2.ToString("F5"));
#endif
                        tesselator.Curve3(command.X1, command.Y1, command.X, command.Y);
                        break;

                    case 'C':
#if verbose
                        Debug.LogFormat("Curve3 to: ({0}|{1}), cp1 ({2}|{3}) cp2 ({4}|{5})", command.X.ToString("F5"), command.Y.ToString("F5"), command.X1.ToString("F5"), command.Y1.ToString("F5"), command.X2.ToString("F5"), command.Y2.ToString("F5"));
#endif
                        tesselator.Curve4(command.X1, command.Y1, command.X2, command.Y2, command.X, command.Y);
                        break;

                    case 'Z':
#if verbose
                        Debug.Log("Close");
#endif
                        tesselator.CloseContour();
                        break;
                }
            }

            tesselator.EndRead();
        }

        #endregion METHODS
    }
}