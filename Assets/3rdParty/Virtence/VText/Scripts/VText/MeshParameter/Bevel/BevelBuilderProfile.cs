// ----------------------------------------------------------------------
// File: 			BevelBuilderProfile
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2020 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Virtence.VText
{
	/// <summary>
	/// Chiseld bevel strategy
	/// </summary>
	internal class BevelBuilderProfile : BevelBuilderStrategy
	{
        #region CONSTANTS

        #endregion // CONSTANTS



        #region FIELDS

        #endregion // FIELDS



        #region PROPERTIES
        /// <summary>
        /// _meshParameter.Bevel divided by _meshParameter.BevelProfile.lengthkK;
        /// </summary>
        private float _bevelFraction;
        #endregion // PROPERTIES



        #region CONSTRUCTORS

        internal BevelBuilderProfile(VTextMeshParameter meshParameter) : base(meshParameter)
        {
            if (_meshParameter.BevelProfile.length < 2)
            {
                _meshParameter.BevelProfile.AddKey(0, 0);
                _meshParameter.BevelProfile.AddKey(_meshParameter.Bevel, _meshParameter.Bevel);
            }

            _bevelFraction = _meshParameter.Bevel / _meshParameter.BevelProfile.length;
        }

        #endregion // CONSTRUCTORS



        #region METHODS
        internal override void AddBevelBackfacesToMesh(ref MeshAttributes meshAttribs)
        {

            var indices = meshAttribs.Indicies[1].ToList();
            int offset = meshAttribs.Verticies.Count;

            //offset from front to backface
            Vector3 backFaceOffset = Vector3.forward * (_meshParameter.Bevel * 2.0f + _meshParameter.Depth);

            // for each contour
            for (int j = 0; j < meshAttribs.VertexAttributes.Count; ++j)
            {
                float uk0 = 0.0f;
                float uk1 = 0.0f;
                for (int k = 1; k < _meshParameter.BevelProfile.length; k++)
                {
                    // previous and current animation keys
                    var prev = _meshParameter.BevelProfile.keys[k - 1];
                    var curr = _meshParameter.BevelProfile.keys[k];

                    // delta keys
                    float deltaTime = curr.time - prev.time;
                    float deltaValue = curr.value - prev.value;

                    // calculate u coordinates
                    uk0 = uk1;
                    uk1 = uk0 + Mathf.Sqrt(deltaTime * deltaTime + deltaValue * deltaValue) * _meshParameter.Bevel;

                    // for each attribute list
                    foreach (var attr in meshAttribs.VertexAttributes[j])
                    {
                        // add verts
                        Vector3 target = Vector3.ProjectOnPlane(attr.BevelVertexDirection * _meshParameter.Bevel, Vector3.forward);
                        Vector3 v0 = attr.VertexPosition + backFaceOffset + Vector3.back * prev.time * _meshParameter.Bevel + target * prev.value;
                        Vector3 v1 = attr.VertexPosition + backFaceOffset + Vector3.back * curr.time * _meshParameter.Bevel + target * curr.value;
                        meshAttribs.Verticies.Add(v0);
                        meshAttribs.Verticies.Add(v1);

                        // add norms
                        Vector3 n = new Vector3(attr.Normal.x * -deltaTime, attr.Normal.y * -deltaTime, -deltaValue).normalized;
                        meshAttribs.Normals.Add(-n);
                        meshAttribs.Normals.Add(-n);

                        // calculate v coordinates
                        float vk0 = Mathf.Lerp(attr.VDistanceContour, attr.VDistanceBevel, prev.time - (prev.time - prev.value));
                        float vk1 = Mathf.Lerp(attr.VDistanceContour, attr.VDistanceBevel, curr.time - (curr.time - curr.value));

                        // add uvs
                        meshAttribs.UVs.Add(new Vector3(uk0, vk0));
                        meshAttribs.UVs.Add(new Vector3(uk1, vk1));
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
                float uk0 = 0.0f;
                float uk1 = 0.0f;
                for (int k = 1; k < _meshParameter.BevelProfile.length; k++)
                {
                    // previous and current animation keys
                    var prev = _meshParameter.BevelProfile.keys[k - 1];
                    var curr = _meshParameter.BevelProfile.keys[k];

                    // delta keys
                    float deltaTime = curr.time - prev.time;
                    float deltaValue = curr.value - prev.value;

                    // calculate u coordinates
                    uk0 = uk1;
                    uk1 = uk0 + Mathf.Sqrt(deltaTime * deltaTime + deltaValue * deltaValue) * _meshParameter.Bevel;

                    //float t0 = (float)(k-1) / (_meshParameter.BevelProfile.length - 1);
                    //float t1 = (float)(k) / (float)(_meshParameter.BevelProfile.length - 1);

                    // for each attribute list
                    foreach (var attr in meshAttribs.VertexAttributes[j])
                    {
                        // add verts
                        Vector3 target = Vector3.ProjectOnPlane(attr.BevelVertexDirection * _meshParameter.Bevel, Vector3.forward);
                        Vector3 v0 = attr.VertexPosition + Vector3.forward * prev.time * _meshParameter.Bevel + target * prev.value;
                        Vector3 v1 = attr.VertexPosition + Vector3.forward * curr.time * _meshParameter.Bevel + target * curr.value;
                        meshAttribs.Verticies.Add(v0);
                        meshAttribs.Verticies.Add(v1);

                        // add norms
                        Vector3 n = new Vector3(attr.Normal.x * deltaTime, attr.Normal.y * deltaTime, -deltaValue).normalized;
                        meshAttribs.Normals.Add(n);
                        meshAttribs.Normals.Add(n);

                        // calculate v coordinates
                        float vk0 = Mathf.Lerp(attr.VDistanceContour, attr.VDistanceBevel, prev.time - (prev.time - prev.value));
                        float vk1 = Mathf.Lerp(attr.VDistanceContour, attr.VDistanceBevel, curr.time - (curr.time - curr.value));

                        // add uvs
                        meshAttribs.UVs.Add(new Vector3(uk0, vk0));
                        meshAttribs.UVs.Add(new Vector3(uk1, vk1));
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

            }
            meshAttribs.Indicies.Add(indices.ToArray());
        }

		#endregion // METHODS
	}
}
