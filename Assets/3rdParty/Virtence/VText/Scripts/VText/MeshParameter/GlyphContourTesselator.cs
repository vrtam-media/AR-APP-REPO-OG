//#define verbose

using LibTessDotNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Virtence.VText.Tesselation
{
    /// <summary>
    /// Util class for translating glyph contours to tesselated polygon
    /// </summary>
    internal class GlyphContourTesselator
    {
        #region EXPOSED

        /// <summary>
        /// Created Contours
        /// </summary>
        public readonly List<Contour> Contours = new List<Contour>();

        #endregion EXPOSED

        #region FIELDS

        /// <summary>
        /// Vertex Cache
        /// </summary>
        private List<Vector3> _verts = new List<Vector3>();

        /// <summary>
        /// Step size of Bezier tesselation
        /// </summary>
        private double _resolution;

        /// <summary>
        /// internal tesselator
        /// </summary>
        private Tess _tess;

        /// <summary>
        /// internal _outLine;
        /// </summary>
        private Tess _outLine;

        /// <summary>
        /// should be true if you need contours
        /// </summary>
        private bool _doCreateContours;

        #endregion FIELDS

        #region PROPERTIES

        /// <summary>
        /// Output Verts
        /// </summary>
        public List<Vector3> Verticies
        {
            get
            {
                if (_tess.VertexCount > 0)
                {
                    return _tess.Vertices.ToList();
                }
                else
                {
                    return new List<Vector3>();
                }
            }
        }

        /// <summary>
        /// Output Indices
        /// </summary>
        public int[] Indices
        {
            get
            {
                if (_tess.ElementCount > 0)
                {
                    return _tess.Elements;
                }
                else
                {
                    return new int[0];
                }
            }
        }

        /// <summary>
        /// Output Normals
        /// </summary>
        public List<Vector3> Normals
        {
            get
            {
                var ret = new List<Vector3>();
                for (int i = 0; i < _tess.VertexCount; ++i)
                {
                    ret.Add(Vector3.back);
                }
                return ret;
            }
        }

        #endregion PROPERTIES

        #region CONSTRUCTOR

        /// <summary>
        /// no empty polygons
        /// </summary>
        /// <param name="resolution"></param>
        public GlyphContourTesselator(double resolution = 1, bool doCreateContours = true)
        {
            _resolution = resolution;
            // Create an instance of the tessellator. Can be reused.
            _tess = new Tess();
            _tess.NoEmptyPolygons = true;
            _outLine = new Tess();
            _outLine.NoEmptyPolygons = true;
            _doCreateContours = doCreateContours;
        }

        #endregion CONSTRUCTOR

        #region METHODS

        public void CloseContour()
        {
#if verbose
            for (int i = 0; i < _verts.Count; i++)
            {
                Vector3 vector = _verts[i];
                Debug.LogFormat("vert {1}: {0}", vector.ToString("F5"), i);
            }
#endif

            if (_verts.Count > 2)
            {
                //if first and last are equal
                if (Vector3.Distance(_verts[0], _verts[_verts.Count - 1]) < 0.00001f)
                {
                    _verts.RemoveAt(_verts.Count - 1);
                }
            }
            if (_verts.Count > 2)
            {
                Vector3[] verts = _verts.ToArray();
                _tess.AddContour(verts);

                if (_doCreateContours)
                {
                    _outLine.AddContour(verts);
                }
            }
            _verts = new List<Vector3>();
        }

        public void Curve3(float x1, float y1, float x2, float y2)
        {
            //convert curve3 to curve4
            //from http://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve

            Vector3 last = _verts.Count > 0 ? _verts[_verts.Count - 1] : Vector3.zero;

            float c1x = last.x + ((2f / 3f) * (x1 - last.x));
            float c1y = last.y + ((2f / 3f) * (y1 - last.y));

            float c2x = x2 + ((2f / 3f) * (x1 - x2));
            float c2y = y2 + ((2f / 3f) * (y1 - y2));

            Curve4(c1x, c1y, c2x, c2y, x2, y2);
        }

        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            Vector3 last = _verts.Count > 0 ? _verts[_verts.Count - 1] : Vector3.zero;

            var curve = new CubicBezierCurve(new List<Vector3>(){
                                                last,
                                                new Vector3(x1,y1),
                                                new Vector3(x2,y2),
                                                new Vector3(x3,y3)});
            if (_verts.Count > 0)
            {
                _verts.RemoveAt(_verts.Count - 1);
            }

#if verbose
            for (int i = 0; i < curve.Flatten(_resolution).Count; i++)
            {
                Vector3 vector = curve.Flatten(_resolution)[i];
                Debug.LogFormat("i: {1} flats: {0}", vector.ToString("F5"), i);
            }
#endif
            _verts.AddRange(curve.Flatten(_resolution));
        }

        public void LineTo(float x1, float y1)
        {
            Vector3 add = new Vector3(x1, y1);
            if (_verts.Count > 0)
            {
                //Debug.Log(add.ToString("F5") + " " + _verts[_verts.Count - 1].ToString("F5"));
                if ((add == _verts[_verts.Count - 1]))
                {
#if verbose
                    Debug.LogFormat("killed: {0}", add.ToString("F5"));
#endif
                    return;
                }
            }

#if verbose
                Debug.LogFormat("add line: {0}", add.ToString("F5"));
#endif
            _verts.Add(add);
        }

        public void MoveTo(float x0, float y0)
        {
            CloseContour();
#if verbose
            Debug.LogFormat("add move: {0}", new Vector3(x0, y0).ToString("F5"));
#endif
            _verts.Add(new Vector3(x0, y0));
        }

        public void BeginRead(int contourCount)
        {
            _verts.Clear();
        }

        public void EndRead()
        {
            WindingRule windingRule = WindingRule.NonZero;

            if (_doCreateContours)
            {
                _outLine.Tessellate(windingRule, ElementType.BoundaryContours, 3, null, Vector3.back);
                // unnessesary sanity check. If previous Tessellate failed next should fail
                //if (_outLine != null && _outLine.Elements != null)
                {
                    for (int i = 1; i < _outLine.Elements.Length; i += 2)
                    {
                        Contours.Add(new Contour(_outLine.Vertices.ToList().GetRange(_outLine.Elements[i - 1], _outLine.Elements[i]).ToArray(), _outLine.Normal));
                    }
                }
            }

            //Debug.Log("tesselate: " + _verts.Count);

            //_testTesselator.Tesselate();

            _tess.Tessellate(windingRule, ElementType.Polygons, 3, null, Vector3.back);
        }

#endregion METHODS
    }
}