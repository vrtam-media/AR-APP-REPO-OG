using UnityEngine;

namespace Virtence.VText
{
    internal struct ContourVertexAttributes
    {
        #region EXPOSED

        internal Vector3 VertexPosition;
        internal Vector3 BevelVertexDirection;
        internal Vector3 Normal;
        internal float VDistanceContour;
        internal float VDistanceBevel;

        #endregion EXPOSED

        #region FIELDS

        //

        #endregion FIELDS

        #region PROPERTIES

        //

        #endregion PROPERTIES

        #region CONSTRUCTOR

        internal ContourVertexAttributes(Vector3 vertexPosition, Vector3 bevelVertexPosition, Vector3 normal, float vDistContour, float vDistBevel)
        {
            VertexPosition = vertexPosition;
            BevelVertexDirection = bevelVertexPosition;
            Normal = normal;
            VDistanceContour = vDistContour;
            VDistanceBevel = vDistBevel;
        }

        #endregion CONSTRUCTOR
    }
}