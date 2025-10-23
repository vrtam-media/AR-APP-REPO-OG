using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Virtence.VText
{
    /// <summary>
    /// Container for mesh data: Vertex, Index, Normal, UV etc.
    /// </summary>
    public class MeshAttributes
    {
        #region EXPOSED

        internal List<Vector3> Verticies;   // mesh verticies
        internal List<int[]> Indicies;      // mesh triangles for each submesh 0 == face; 1 == bevel; 2 == sides
        internal List<Vector3> Normals;     // mesh normals
        internal List<Vector2> UVs;         // mesh uv channel 0

        internal List<List<ContourVertexAttributes>> VertexAttributes; 

        #endregion EXPOSED

        #region FIELDS

        //

        #endregion FIELDS

        #region CONSTRUCTOR

        /// <summary>
        /// Standard constructor
        /// </summary>
        public MeshAttributes()
        {
            Verticies = new List<Vector3>();
            Indicies = new List<int[]>();
            Indicies.Add(new int[0]);
            Normals = new List<Vector3>();
            UVs = new List<Vector2>();
            VertexAttributes = new List<List<ContourVertexAttributes>>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public MeshAttributes(MeshAttributes other)
        {
            Verticies = new List<Vector3>();
            Verticies.AddRange(other.Verticies);
            Indicies = new List<int[]>(other.Indicies.Count);
            foreach(var intArray in other.Indicies)
            {
                var newArray = new int[intArray.Length];
                for(int i = 0; i < newArray.Length; ++i)
                {
                    newArray[i] = intArray[i];
                }
                Indicies.Add(newArray);
            }
            Normals = new List<Vector3>();
            Normals.AddRange(other.Normals);
            UVs = new List<Vector2>();
            UVs.AddRange(other.UVs);
            VertexAttributes = new List<List<ContourVertexAttributes>>(other.VertexAttributes.Count);
            for (int i = 0; i < other.VertexAttributes.Count; ++i)
            {
                VertexAttributes.Add(other.VertexAttributes[i]);
            }
        }

        public MeshAttributes(Mesh mesh)
        {
            mesh.GetVertices(Verticies);
            Indicies = new List<int[]>();
            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                Indicies.Add(mesh.GetIndices(i));
            }
            mesh.GetNormals(Normals);
            mesh.GetUVs(0, UVs);
        }

        public MeshAttributes(List<Vector3> verticies, List<int[]> indicies, List<Vector3> normals, List<Vector2> uvs)
        {
            Verticies = verticies;
            Indicies = indicies;
            Normals = normals;
            UVs = uvs;
        }

        #endregion CONSTRUCTOR

        #region METHODS

        /// <summary>
        /// Convert fields into a mesh
        /// </summary>
        /// <returns></returns>
        internal Mesh ToMesh(float scale = 1.0f)
        {
            Mesh mesh = new Mesh();

            if (scale != 1.0f)
            {
                var verts = new List<Vector3>(Verticies.Count);
                for(int i = 0; i < Verticies.Count; ++i)
                {
                    verts.Add(Verticies[i] * scale);
                }
                mesh.SetVertices(verts);

                var uvs = new List<Vector2>(UVs.Count);
                for(int i = 0; i < UVs.Count; ++i)
                {
                    uvs.Add(UVs[i] * scale);
                }
                mesh.SetUVs(0, uvs);
            }
            else
            {
                mesh.SetVertices(Verticies);
                mesh.SetUVs(0, UVs);
            }

            mesh.SetNormals(Normals);
            mesh.subMeshCount = Indicies.Count;

            for (int i = 0; i < Indicies.Count; ++i)
            {
                mesh.SetIndices(Indicies[i], MeshTopology.Triangles, i);
            }
            return mesh;
        }

        /// <summary>
        /// Plot data
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < Indicies.Count; ++i)
            {
                sb.AppendLine("Indices " + i);
                for (int j = 0; j < Indicies[i].Length; ++j)
                {
                    sb.AppendLine(Indicies[i][j].ToString());
                }
            }

            sb.AppendLine("Verts");
            for (int i = 0; i < Verticies.Count; ++i)
            {
                sb.AppendLine(Verticies[i].ToString("F5"));
            }

            /*
            sb.AppendLine("EndIndices");
            for (int offset = 0, i = 0; i < ContourVertexCounts.Count; ++i)
            {
                sb.AppendLine(ContourVertexCounts[i].ToString() + ": " + Verticies[offset + ContourVertexCounts[i]].ToString("F5"));
                offset += ContourVertexCounts[i];
            }
            */
            sb.AppendLine("Norms");
            for (int i = 0; i < Normals.Count; ++i)
            {
                sb.AppendLine(Normals[i].ToString("F5"));
            }

            sb.AppendLine("UVs");
            for (int i = 0; i < UVs.Count; ++i)
            {
                sb.AppendLine(UVs[i].ToString("F5"));
            }

            return sb.ToString();
        }

        #endregion METHODS
    }
}