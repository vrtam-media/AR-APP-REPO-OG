using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Virtence.VText
{
    /// <summary>
    /// Util class for contour iteration.
    /// Please note: all getter do not check verts.
    /// Utilize IsValid instead before using any methods
    /// </summary>
    internal struct Contour
    {
        private readonly Vector3[] verts;

        public int Count
        {
            get
            {
                return verts.Length;
            }
        }

        public Vector3[] Vertices {
            get { return verts; }
        }

        public List<Vector3> VertexList
        {
            get { return verts.ToList(); }
        }

        public Vector3 Normal;

        public Contour(Vector3[] vertexPositions, Vector3 normal)
        {
            verts = vertexPositions;//.ToList().Distinct().ToArray();

            this.Normal = normal;
        }

        public bool IsValid()
        {
            return verts != null && verts.Length > 2;
        }

        private void Clamp(ref int index)
        {
            while (index < 0)
            {
                index += verts.Length;
            }

            while (index >= verts.Length)
            {
                index -= verts.Length;
            }
        }

        public Vector3 GetPrev(int index)
        {
            index--;
            Clamp(ref index);
            return verts[index];
        }

        public Vector3 GetCurr(int index)
        {
            Clamp(ref index);
            return verts[index];
        }

        public Vector3 GetNext(int index)
        {
            index++;
            Clamp(ref index);
            return verts[index];
        }

        public Vector3 GetPrevFaceNormal(int index)
        {
            return Vector3.Cross(GetCurr(index) - GetPrev(index), Normal).normalized;
        }

        public Vector3 GetNextFaceNormal(int index)
        {
            return Vector3.Cross(GetNext(index) - GetCurr(index), Normal).normalized;
        }

        public Vector3 GetAverageFaceNormal(int index)
        {
            return Vector3.Normalize(GetNextFaceNormal(index) + GetPrevFaceNormal(index));
        }

        public Vector3 GetPrevFaceBevelVertexDirection(int index)
        {
            var ret = GetPrevFaceNormal(index);
            ret.z += 1.0f;
            return ret;
        }

        public Vector3 GetAverageFaceBevelVertexDirection(int index)
        {
            var ret = GetAverageFaceNormal(index);
            ret.z += 1.0f;
            return ret;
        }

        public Vector3 GetNextFaceBevelVertexDirection(int index)
        {
            var ret = GetNextFaceNormal(index);
            ret.z += 1.0f;
            return ret;
        }

        public Vector3 GetCornerBevelVertexDirection(int index)
        {
            var ret = GetNextFaceNormal(index) + GetPrevFaceNormal(index);
            ret.z += 1.0f;
            return ret;
        }

        /// <summary>
        /// To understand this see below
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Vector3 GetIntersectingFaceBevelVertexDirection(int index)
        {
            Vector3 curr = GetCurr(index);
            Vector3 nextDir = GetNext(index) - curr;
            Vector3 prevDir = GetPrev(index) - curr;

            Vector3 nextToPrev = (GetPrevFaceNormal(index) - GetNextFaceNormal(index));

            float t = nextDir.x * prevDir.y - nextDir.y * prevDir.x;

            if(Mathf.Abs(t) > Mathf.Epsilon)
            {
                float s = nextToPrev.x * prevDir.y - nextToPrev.y * prevDir.x;
                s = s / t;
                /*
                Debug.Log("Index: " + index + " curr: " + curr.ToString("F5") + " nextDir: " + nextDir.ToString("F5") + " prevDir: " + prevDir.ToString("F5")
                    + "\nlineVec3: " + nextToPrev.ToString("F5")
                    + "\ns: " + s.ToString("F5"));
                    */
                var ret = GetNextFaceNormal(index) + nextDir * s;
                ret.z += 1.0f;
                return ret;
            }
            else
            {
                var ret = GetNextFaceNormal(index);
                ret.z += 1.0f;
                return ret;
            }
        }

        /*
        public Vector3 GetAverageFaceBevelVertex(int index)
        {
            Vector3 curr = GetCurr(index);
            Vector3 next = GetNext(index);
            Vector3 prev = GetPrev(index);

            var ret = LineLineIntersection(
                next + GetNextFaceNormal(index),
                curr - next,
                prev + GetPrevFaceNormal(index),
                curr - prev,
                index
                );
            //Debug.Log("Index: " + index + " ret: " + ret.ToString("F5") + " length: " + ret.magnitude);
            ret.z += 1.0f;
            return ret;
        }
        
        private Vector3 LineLineIntersection(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2, int index)
        {
            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;

            
            Debug.Log("Index: " + index
                + "\nlineVec3: " + lineVec3.ToString("F5")
                + "\ncrossVec1and2: " + crossVec1and2.ToString("F5")
                + "\ncrossVec1and2.sqrMagnitude: " + crossVec1and2.sqrMagnitude.ToString("F5")
                + "\ncrossVec3and2: " + crossVec3and2.ToString("F5")
                + "\ns: " + s.ToString("F5"));
                
            return linePoint1 + (lineVec1 * s);
        }
        */

        public float GetSignedAngle(int index)
        {
            return Vector3.SignedAngle(GetPrevFaceNormal(index), GetNextFaceNormal(index), Normal);
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < verts.Length; ++i)
            {
                sb.AppendLine(string.Format("v{0}: {1}",i, verts[i].ToString("F5")));
            }

            return sb.ToString();
        }
    }
}