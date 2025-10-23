// ----------------------------------------------------------------------
// File: 			TesselatorBase
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Virtence.VText.Tesselation
{
    /// <summary>
    /// 
    /// </summary>
    internal class TesselatorBase : ITesselator
    {
        #region EVENTS
        #endregion // EVENTS

        #region CONSTANTS
        #endregion // CONSTANTS

        #region FIELDS
        private List<Contour> _contours;                // the list of contours which should be tesselated
        #endregion // FIELDS

        #region PROPERTIES
        public List<Contour> Contours => _contours;
        #endregion // PROPERTIES

        #region CONSTRUCTORS
        #endregion // CONSTRUCTORS

        #region METHODS
        /// <summary>
        /// cleanup this tesselator
        /// </summary>
        public void Cleanup()
        {
            if (_contours != null)
            {
                _contours.Clear();
            }
        }

        public void AddContour(Contour contour)
        {
            if (_contours == null)
            {
                _contours = new List<Contour>();
            }
            _contours.Add(contour);
        }

        /// <summary>
        /// tesselate the specified glyph
        /// </summary>
        /// <param name="glyph"></param>
        public virtual void Tesselate()
        {
            if (_contours == null)
            {
                UnityEngine.Debug.Log("no contours specified");
            }

            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();
            List<List<Vector3>> holes = new List<List<Vector3>>(); 
            for (int i = 0; i < _contours.Count; i++)
            {
                Contour c = _contours[i];
                if (i == 0) {                    
                    poly.outside = new List<Vector3>(c.VertexList);
                } else {
                    if (poly.holes == null) {
                        poly.holes = new List<List<Vector3>>();
                    }
                    poly.holes.Add(c.VertexList);
                }
                
                //UnityEngine.Debug.Log(string.Format("Contour: {0} --- {1} vertices (CLICK ME)\n{2}", i, c.VertexList.Count, c));
            }

            //Poly2Mesh.CreateGameObject(poly);
        }
        #endregion // METHODS

        #region EVENT HANDLERS
        #endregion // EVENT HANDLERS
    }
}
