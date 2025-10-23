using UnityEngine;

namespace Virtence.VText
{
    /// <summary>
    /// Additional components for each glyph
    /// </summary>
    [System.Serializable]
    public class VTextAdditionalComponents
    {
        #region FIELDS

        [SerializeField]
        private GameObject _additionalComponentsObject;                 // a dummy gameobject which holds all components which should be added to each glyph

        private bool _modified = false;                                 // determine if the additional components are changed or not

        #endregion FIELDS

        #region PROPERTIES

        public GameObject AdditionalComponentsObject
        {
            get
            {
                return _additionalComponentsObject;
            }

            set
            {
                if (value != _additionalComponentsObject)
                {
                    _modified = true;
                }
                _additionalComponentsObject = value;
            }
        }

        #endregion PROPERTIES

        #region METHODS

        /// <summary>
        /// check if the parameters are modified and reset the modify flag
        /// </summary>
        /// <returns><c>true</c>, if the parameters are modified, <c>false</c> otherwise.</returns>
        public bool CheckClearModified()
        {
            if (_modified)
            {
                _modified = false;
                return true;
            }
            return false;
        }

        #endregion METHODS
    }
}