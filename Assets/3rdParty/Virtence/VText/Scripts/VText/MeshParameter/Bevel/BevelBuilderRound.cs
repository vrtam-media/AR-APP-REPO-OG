// ----------------------------------------------------------------------
// File: 			BevelBuilderRound
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2020 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Vector3 = UnityEngine.Vector3;

namespace Virtence.VText
{
	/// <summary>
	/// Chiseld bevel strategy
	/// </summary>
	internal class BevelBuilderRound : BevelBuilderStrategy
	{
        #region CONSTANTS

        #endregion // CONSTANTS



        #region FIELDS

        #endregion // FIELDS



        #region PROPERTIES

        #endregion // PROPERTIES



        #region CONSTRUCTORS

        internal BevelBuilderRound(VTextMeshParameter meshParameter) : base(meshParameter)
        {
        }

        #endregion // CONSTRUCTORS



        #region METHODS
        internal override void AddBevelBackfacesToMesh(ref MeshAttributes meshAttribs)
        {
            var indices = meshAttribs.Indicies[1].ToList();
            int offset = meshAttribs.Verticies.Count;

            //offset from front to backface
            Vector3 backfaceOffset = Vector3.forward * (_meshParameter.Bevel * 2.0f + _meshParameter.Depth);

            // for each contour
            for (int j = 0; j < meshAttribs.VertexAttributes.Count; ++j)
            {
                // for each attribute list
                foreach (var attr in meshAttribs.VertexAttributes[j])
                {
                    // add verts
                    meshAttribs.Verticies.Add(attr.VertexPosition + backfaceOffset);
                    meshAttribs.Verticies.Add(attr.VertexPosition + attr.BevelVertexDirection * _meshParameter.Bevel + Vector3.forward * _meshParameter.Depth);

                    // add norms
                    meshAttribs.Normals.Add(Vector3.back);
                    meshAttribs.Normals.Add(attr.Normal);

                    // add uvs
                    if (_meshParameter.UseFaceUVs)
                    {
                        // face uvs
                        meshAttribs.UVs.Add(attr.VertexPosition);
                        meshAttribs.UVs.Add(attr.VertexPosition + attr.BevelVertexDirection * _meshParameter.Bevel);
                    }
                    else
                    {
                        // side uvs
                        meshAttribs.UVs.Add(new Vector3(0.0f, attr.VDistanceContour));
                        meshAttribs.UVs.Add(new Vector3(ROOT_TWO_TIMES_BEVEL, attr.VDistanceBevel));
                    }
                }

                // add triangles in this manner:
                // 3, 1, 2
                // 2, 1, 0
                // then
                // 5, 3, 4
                // 4, 3, 2
                // and so forth
                int i = 0;
                for (; i < meshAttribs.VertexAttributes[j].Count * 2 - 2; i += 2)
                {
                    indices.Add(offset + i + 3);
                    indices.Add(offset + i + 1);
                    indices.Add(offset + i + 2);
                    indices.Add(offset + i + 2);
                    indices.Add(offset + i + 1);
                    indices.Add(offset + i);
                }

                // increment offset
                offset += i + 2;
            }

            meshAttribs.Indicies[1] = indices.ToArray();
        }

        internal override void AddBevelFrontFacesToMesh(ref MeshAttributes meshAttribs)
        {
            var indices = new List<int>();
            int offset = meshAttribs.Verticies.Count;

            // for each contour
            for (int j = 0; j < meshAttribs.VertexAttributes.Count; ++j)
            {
                // for each attribute list
                foreach (var attr in meshAttribs.VertexAttributes[j])
                {
                    // add verts
                    meshAttribs.Verticies.Add(attr.VertexPosition);
                    meshAttribs.Verticies.Add(attr.VertexPosition + attr.BevelVertexDirection * _meshParameter.Bevel);

                    // add norms
                    meshAttribs.Normals.Add(Vector3.back);
                    meshAttribs.Normals.Add(attr.Normal);

                    // add uvs
                    if (_meshParameter.UseFaceUVs)
                    {
                        // face uvs
                        meshAttribs.UVs.Add(attr.VertexPosition);
                        meshAttribs.UVs.Add(attr.VertexPosition + attr.BevelVertexDirection * _meshParameter.Bevel);
                    }
                    else
                    {
                        // side uvs
                        meshAttribs.UVs.Add(new Vector3(0.0f, attr.VDistanceContour));
                        meshAttribs.UVs.Add(new Vector3(ROOT_TWO_TIMES_BEVEL, attr.VDistanceBevel));
                    }
                }

                // add triangles in this manner:
                // 0, 1, 2
                // 2, 1, 3
                // then
                // 2, 3, 4
                // 4, 3, 5
                // and so forth
                int i = 0;
                for (; i < meshAttribs.VertexAttributes[j].Count * 2 - 2; i += 2)
                {
                    indices.Add(offset + i);
                    indices.Add(offset + i + 1);
                    indices.Add(offset + i + 2);
                    indices.Add(offset + i + 2);
                    indices.Add(offset + i + 1);
                    indices.Add(offset + i + 3);
                }

                // increment offset
                offset += i + 2;
            }

            meshAttribs.Indicies.Add(indices.ToArray());
        }

		#endregion // METHODS
	}
}
