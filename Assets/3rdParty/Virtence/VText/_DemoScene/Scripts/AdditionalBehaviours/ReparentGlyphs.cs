using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Virtence.VText
{
    public class ReparentGlyphs : MonoBehaviour
    {

        #region EXPOSED
        [Tooltip("determines if the parent should have the glyphs position or not")]
        public bool ParentIsPivot;                                          // determines if the parent should have the glyphs position or not

        [Tooltip("if the parent should have a fixed local position (ParentIsPivot == false) this will be the position of the parent")]
        public Vector3 PositionIfParentIsNotPivot = Vector3.zero;           // if the parent should have a fixed local position (ParentIsPivot == false) this will be the position of the parent
        #endregion // EXPOSED

        #region CONSTANTS
        #endregion // CONSTANTS

        #region FIELDS
        #endregion // FIELDS

        #region PROPERTIES
        #endregion // PROPERTIES

        #region METHODS

        void Start()
        {
            ReparentGlyph();
        }


        /// <summary>
        /// reparent the glyph
        /// if ParentIsPivot then the transformation will be on the parent and the local glyph position will be Vector3.zero
        /// else the parents local position will always be the PositionIfParentIsPivot == false
        /// </summary>
        public void ReparentGlyph()
        {
            if (transform.GetComponent<Renderer>() == null)
            {
                return;
            }

            GameObject parent = new GameObject(gameObject.name + "_parent");
            parent.transform.SetParent(transform.parent, false);

            if (ParentIsPivot)
            {
                parent.transform.localPosition = transform.localPosition;
            }
            else
            {
                parent.transform.localPosition = PositionIfParentIsNotPivot;
            }

            transform.SetParent(parent.transform, true);

        }
        #endregion // METHODS
    }
}