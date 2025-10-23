using UnityEngine;

namespace Virtence.VText.DebugUtilities
{
    /// <summary>
    /// Debug class for Glyph
    /// </summary>
    internal class GlyphContourPointsDrawer
    {

        #region CONSTANTS
        private const float DEBUG_DURATION = 30.0f;             // the duration of debug lines in seconds
        #endregion // CONSTANTS

        private Vector3 _previousVertex;
        private System.Text.StringBuilder _stringBuilder;

        public void BeginRead(int contourCount)
        {
            _stringBuilder = new System.Text.StringBuilder();
            _stringBuilder.AppendLine(string.Format("Begin read: {0} contours.", contourCount));
        }

        public void CloseContour()
        {
            Debug.DrawRay(_previousVertex, Vector3.forward * 0.25f, Color.magenta, DEBUG_DURATION);
            _stringBuilder.AppendLine("Close " + _previousVertex.ToString("F5"));
        }

        public void Curve3(float x1, float y1, float x2, float y2)
        {
            //convert curve3 to curve4
            //from http://stackoverflow.com/questions/9485788/convert-quadratic-curve-to-cubic-curve

            Vector3 last = _previousVertex;

            float c1x = last.x + ((2f / 3f) * (x1 - last.x));
            float c1y = last.y + ((2f / 3f) * (y1 - last.y));

            float c2x = x2 + ((2f / 3f) * (x1 - x2));
            float c2y = y2 + ((2f / 3f) * (y1 - y2));

            //Curve4(c1x, c1y, c2x, c2y, x2, y2);
            Debug.DrawLine(_previousVertex, new Vector3(x2, y2), Color.blue, DEBUG_DURATION);
            _previousVertex = new Vector3(x2, y2);
            _stringBuilder.AppendLine("Curve3 " + _previousVertex.ToString("F5"));
        }

        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            Debug.DrawLine(_previousVertex, new Vector3(x3, y3), Color.green, DEBUG_DURATION);
            _previousVertex = new Vector3(x3, y3);
            _stringBuilder.AppendLine("silvio Curve4 " + _previousVertex.ToString("F5"));
        }

        public void EndRead()
        {
            Debug.Log(_stringBuilder.ToString());
        }

        public void LineTo(float x1, float y1)
        {
            Debug.DrawLine(_previousVertex, new Vector3(x1, y1), Color.green, DEBUG_DURATION);
            _previousVertex = new Vector3(x1, y1);
            _stringBuilder.AppendLine("LineTo " + _previousVertex.ToString("F5"));
        }

        public void MoveTo(float x0, float y0)
        {
            Debug.DrawLine(_previousVertex, new Vector3(x0, y0), Color.clear, DEBUG_DURATION);
            _previousVertex = new Vector3(x0, y0);
            _stringBuilder.AppendLine("MoveTo " + _previousVertex.ToString("F5"));
        }
    }
}