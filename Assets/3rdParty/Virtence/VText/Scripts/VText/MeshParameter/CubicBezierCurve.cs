using System.Collections.Generic;
using UnityEngine;

namespace Virtence.VText
{
    internal struct CubicBezierCurve
    {
        // Bezier points
        private List<Vector3> _bezierControlPoints;

        public CubicBezierCurve(List<Vector3> ControlPoints)
        {
            _bezierControlPoints = ControlPoints;
        }

        /// <summary>
        /// Flatten bezier with a given resolution
        /// </summary>
        /// <param name="tolerance">tolerance
        internal List<Vector3> Flatten(double tolerance)
        {
            List<Vector3> points = new List<Vector3>();

            // First point
            Vector3 vector = GetBezierPoint(0);
            points.Add(new Vector3(vector.x, vector.y));

            int last = _bezierControlPoints.Count - 4;

            if (0 <= last)
            {
                // Tolerance needs to be non-zero positive
                tolerance = System.Math.Abs(tolerance);

                // Flatten individual segments
                for (int i = 0; i <= last; i += 3)
                {
                    FlattenSegment(i, tolerance, points);
                }
            }

            points.RemoveAt(points.Count - 1);
            points.Add(GetBezierPoint(_bezierControlPoints.Count - 1));
            return points;
        }

        /// <summary>
        /// Evaluate on a Bezier segment a point at a given parameter
        /// </summary>
        /// <param name="iFirst">Index of Bezier segment's first point
        /// <param name="t">Parameter value t
        /// <returns>Return the point at parameter t on the curve</returns>
        private Vector3 DeCasteljau(int iFirst, float t)
        {
            // Using the de Casteljau algorithm.  See "Curves & Surfaces for Computer
            // Aided Design" for the theory
            float s = 1.0f - t;

            // Level 1
            Vector3 Q0 = s * GetBezierPoint(iFirst) + t * GetBezierPoint(iFirst + 1);
            Vector3 Q1 = s * GetBezierPoint(iFirst + 1) + t * GetBezierPoint(iFirst + 2);
            Vector3 Q2 = s * GetBezierPoint(iFirst + 2) + t * GetBezierPoint(iFirst + 3);

            // Level 2
            Q0 = s * Q0 + t * Q1;
            Q1 = s * Q1 + t * Q2;

            // Level 3
            return s * Q0 + t * Q1;
        }

        /// <summary>
        ///  Flatten a Bezier segment within given resolution
        /// </summary>
        /// <param name="iFirst">Index of Bezier segment's first point
        /// <param name="tolerance">tolerance
        /// <param name="points">
        /// <returns></returns>
        private void FlattenSegment(int iFirst, double tolerance, List<Vector3> points)
        {
            // We use forward differencing.  It is much faster than subdivision
            int i, k;
            int nPoints = 1;
            Vector3[] Q = new Vector3[4];

            // The number of points is determined by the "curvedness" of this segment,
            // which is a heuristic: it's the maximum of the 2 medians of the triangles
            // formed by consecutive Bezier points.  Why median? because it is cheaper
            // to compute than height.
            double rCurv = 0;

            for (i = checked(iFirst + 1); i <= checked(iFirst + 2); i++)
            {
                // Get the longer median
                Q[0] = (GetBezierPoint(i - 1) + GetBezierPoint(i + 1)) * 0.5f - GetBezierPoint(i);

                double r = Q[0].magnitude;

                if (r > rCurv)
                {
                    rCurv = r;
                }
            }

            // Now we look at the ratio between the medain and the error tolerance.
            // the points are collinear then one point - the endpoint - will do.
            // Otherwise, since curvature is roughly inverse proportional
            // to the square of nPoints, we set nPoints to be the square root of this
            // ratio, but not less than 3.
            if (rCurv <= 0.5 * tolerance)  // Flat segment
            {
                Vector3 vector = GetBezierPoint(iFirst + 3);
                points.Add(new Vector3(vector.x, vector.y));
                return;
            }

            // Otherwise we'll have at least 3 points
            // Tolerance is assumed to be positive
            nPoints = (int)(System.Math.Sqrt(rCurv / tolerance)) + 3;
            if (nPoints > 10)
            {
                nPoints = 10; // Arbitrary limitation, but...
            }

            // Get the first 4 points on the segment in the buffer
            float d = 1.0f / nPoints;

            Q[0] = GetBezierPoint(iFirst);
            for (i = 1; i <= 3; i++)
            {
                Q[i] = DeCasteljau(iFirst, i * d);
                points.Add(new Vector3(Q[i].x, Q[i].y));
            }

            // Replace points in the buffer with differences of various levels
            for (i = 1; i <= 3; i++)
            {
                for (k = 0; k <= (3 - i); k++)
                {
                    Q[k] = Q[k + 1] - Q[k];
                }
            }

            // Now generate the rest of the points by forward differencing
            for (i = 4; i <= nPoints; i++)
            {
                for (k = 1; k <= 3; k++)
                {
                    Q[k] += Q[k - 1];
                }

                points.Add(new Vector3(Q[3].x, Q[3].y));
            }
        }

        /// <summary>
        /// Returns a single bezier control point at index
        /// </summary>
        /// <param name="index">Index
        /// <returns></returns>
        private Vector3 GetBezierPoint(int index)
        {
            return _bezierControlPoints[index];
        }
    }
}