// ----------------------------------------------------------------------
// File: 			VTextGlyphBuilder
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2020 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Virtence.VText.Tesselation;

namespace Virtence.VText
{
    /// <summary>
    /// Creates beautiuly 3D Mesh for a glyph
    /// </summary>
    [UnityEngine.Scripting.Preserve]
	internal class VTextGlyphBuilder
    {
        #region FIELDS

        /// <summary>
        /// Mesh params
        /// </summary>
        private VTextMeshParameter _meshParameter;

        /// <summary>
        /// The Font.
        /// </summary>
        private IVFont _font;

        /// <summary>
        /// Builder for glyph paths
        /// </summary>
        private IPathBuilder _pathBuilder;

        /// <summary>
        /// processed glyphs container
        /// </summary>
        private readonly Dictionary<char, MeshAttributes> _glyphMeshes;

        #endregion FIELDS

        #region PROPERTIES

        //

        #endregion PROPERTIES

        #region CONSTRUCTOR

        /// <summary>
        /// Init fresh hashes
        /// </summary>
        /// <param name="meshParameter"></param>
        /// <param name="font"></param>
        internal VTextGlyphBuilder(VTextMeshParameter meshParameter, IVFont font)
        {
            _meshParameter = meshParameter;
            _font = font;
            _pathBuilder = new OpenCSPathBuilder(font);
            _glyphMeshes = font.GlyphMeshAttributesHash;
            
            /*
            // for debugging kill the glyph hash
            if(!Application.isPlaying)
            {
                _glyphMeshes = new Dictionary<char, MeshAttributes>();
            }
            */
        }

        #endregion CONSTRUCTOR

        #region METHODS

        /// <summary>
        /// Add MeshFilter components onto transforms
        /// </summary>
        /// <param name="transforms"></param>
        public IEnumerable<MeshFilter> AddGlyphMeshFilters(List<Transform> transforms, float size)
        {
            // return list
            var filters = new List<MeshFilter>();
            //foreach child
            foreach (var t in transforms)
            {
                //current char is coded into the name
                var currentChar = t.name[0];

                MeshAttributes mesh;
                // if not already generated
                if (!_glyphMeshes.ContainsKey(currentChar))
                {
                    mesh = new MeshAttributes();
                    ApplyFrontface(ref mesh, currentChar);
                    if (mesh != null)
                    {
                        _glyphMeshes.Add(currentChar, new MeshAttributes(mesh));
                    }
                }
                else
                {
                    mesh = new MeshAttributes(_glyphMeshes[currentChar]) ?? new MeshAttributes();
                }
                ApplyBackface(ref mesh);
                ApplySides(ref mesh);

                if (mesh != null) {
                    Mesh m = mesh.ToMesh(size);
                    m.name = t.name;
                    if(_meshParameter.GenerateTangents)
                    {
                        m.RecalculateTangents();
                    }
                    var filter = t.gameObject.AddComponent<MeshFilter>();
                    filter.sharedMesh = m;
                    filters.Add(filter);
                }
            }

            return filters;
        }

#region glyphface

        /// <summary>
        /// Add the glyph face to mesh attribs
        /// </summary>
        /// <param name="meshAttribs"></param>
        /// <param name="selectedChar"></param>
        private void ApplyFrontface(ref MeshAttributes meshAttribs, char selectedChar)
        {
            //----------------------- check typography glyph for early exit
            var glyph = _font.GetGlyph(selectedChar);
            if (glyph.Index == 0)
            {
                meshAttribs = null;
                Debug.LogWarning("Current font does not contain a definition for '" + selectedChar + "'.");
                return;
            }

            var tesselator = new GlyphContourTesselator(_meshParameter.Resolution);
            _pathBuilder.ApplyTesselator(ref tesselator, glyph);

            //----------------------- assign
            meshAttribs.Verticies = tesselator.Verticies;
            meshAttribs.Indicies[0] = tesselator.Indices;
            meshAttribs.Normals = tesselator.Normals;

            //----------------------- uvs.xy == verts.xy (without z)
            meshAttribs.UVs = meshAttribs.Verticies.Select(v => new Vector2(v.x, v.y)).ToList();
            CalculateVertexAttributes(ref meshAttribs, tesselator.Contours);
        }

        /// <summary>
        /// Add the glyph back face to mesh attribs
        /// </summary>
        /// <param name="meshAttribs"></param>
        private void ApplyBackface(ref MeshAttributes meshAttribs)
        {
            // early exit
            if (!_meshParameter.HasBackface || meshAttribs == null)
            {
                return;
            }

            // offset to back face
            Vector3 backFaceOffset = Vector3.forward * (_meshParameter.Depth + 2 * _meshParameter.Bevel);

            int vertCount = meshAttribs.Verticies.Count;
            for (int i = 0; i < vertCount; i++)
            {
                //----------------------- add displaced verts
                meshAttribs.Verticies.Add(meshAttribs.Verticies[i] + backFaceOffset);

                //----------------------- normals
                meshAttribs.Normals.Add(Vector3.forward);

                //----------------------- uvs
                meshAttribs.UVs.Add(meshAttribs.UVs[i]);
            }

            //----------------------- indices
            var backindices = meshAttribs.Indicies[0].ToList();
            for (int i = meshAttribs.Indicies[0].Length - 1; i >= 0; --i)
            {
                backindices.Add(backindices[i] + vertCount);
            }
            meshAttribs.Indicies[0] = backindices.ToArray();
        }

#endregion glyphface

#region glyphrest

        /// <summary>
        /// Creates vertex attribute list from contours and
        /// mesh from vertex attribute lists
        /// </summary>
        /// <param name="meshAttribs"></param>
        private void ApplySides(ref MeshAttributes meshAttribs)
        {
            //early exit
            if (_meshParameter.Bevel < Mathf.Epsilon && _meshParameter.Depth < Mathf.Epsilon || meshAttribs == null)
            {
                return;
            }

            // if bevel add bevel edges with bevel builder strategy
            if (_meshParameter.Bevel > 0.0f)
            {
                var bevelBuilder = new BevelBuilder(_meshParameter);

                bevelBuilder.AddBevelFrontFacesToMesh(ref meshAttribs);
                if (_meshParameter.HasBackface)
                {
                    bevelBuilder.AddBevelBackfacesToMesh(ref meshAttribs);
                }
            }

            // if depth add sides
            if (_meshParameter.Depth > 0.0f)
            {
                AddSidesToMesh(ref meshAttribs);
            }
        }

        /// <summary>
        /// Creates VertexAttributes for a collection of contours
        /// </summary>
        /// <param name="meshAttribs"></param>
        /// <param name="contours"></param>
        protected void CalculateVertexAttributes(ref MeshAttributes meshAttribs, List<Contour> contours)
        {
            // to fill:
            meshAttribs.VertexAttributes = new List<List<ContourVertexAttributes>>();

            // fill list of vertex attributes foreach contour
            foreach (var contour in contours)
            {
                List<ContourVertexAttributes> contourAttributes = CalculateVertexAttributes(contour);
                if (contourAttributes != null)
                {
                    meshAttribs.VertexAttributes.Add(contourAttributes);
                }
            }
        }

        /// <summary>
        /// convert a contour into a vertex attribute list
        /// </summary>
        /// <param name="contour"></param>
        /// <returns></returns>
        protected List<ContourVertexAttributes> CalculateVertexAttributes(Contour contour)
        {
            // if no contour or consists of two vertex
            if (!contour.IsValid())
            {
                return null;
            }


            //to fill:
            List<ContourVertexAttributes> contourAttributes = new List<ContourVertexAttributes>();

            var vDist = 0.0f;

            // for each vertex
            for (int i = 0; i <= contour.Count; ++i)
            {
                //get current and next position
                Vector3 curr = contour.GetCurr(i);

                var angle = contour.GetSignedAngle(i);

#if debug
                Debug.Log("i: "+ i + " curr: " + (curr ).ToString("F5") + " angle: " + angle);
                
                if (angle < -45)
                    Debug.DrawRay(curr, Vector3.back * _meshParameter.Bevel * 10, Color.green, 10f);
                if (angle < 90)
                    Debug.DrawRay(curr, Vector3.back * _meshParameter.Bevel * 10, Color.red, 10f);
                else if (angle < 135)
                    Debug.DrawRay(curr, Vector3.back * _meshParameter.Bevel * 10, Color.cyan, 10f);
                else
                    Debug.DrawRay(curr, Vector3.back * _meshParameter.Bevel * 10, Color.magenta, 10f);
#endif


                if (angle < -45) // is acute angle (<45°)
                {
                    //Debug.Log("acute");

                    Vector3 previousBevel = contour.GetPrevFaceBevelVertexDirection(i);
                    Vector3 nextBevel = contour.GetNextFaceBevelVertexDirection(i);

                    // add pos with prev normal
                    // add pos with next normal
                    Vector3 prevNormal = contour.GetPrevFaceNormal(i);
                    Vector3 nextNormal = contour.GetNextFaceNormal(i);

                    contourAttributes.Add(new ContourVertexAttributes(curr, previousBevel, prevNormal, vDist, vDist));
                    contourAttributes.Add(new ContourVertexAttributes(curr, nextBevel, nextNormal, vDist, vDist));

                    //Debug.DrawRay(curr, prevNormal * _meshParameter.Bevel, Color.yellow, 10f);
                    //Debug.DrawRay(curr, nextNormal * _meshParameter.Bevel, Color.magenta, 10f);
                }
                else if(angle < 45) // is flat
                {
                    //Debug.Log("flat");

                    //get bevel
                    Vector3 currBevel = contour.GetIntersectingFaceBevelVertexDirection(i);

                    // add pos with average normal
                    Vector3 averNormal = contour.GetAverageFaceNormal(i);

                    contourAttributes.Add(new ContourVertexAttributes(curr, currBevel, averNormal, vDist, vDist));

                    //Debug.DrawRay(curr, averNormal * _meshParameter.Bevel*10, Color.cyan, 10f);
                }
                else if(angle < 135)//is not flat
                {
                    //Debug.Log("not flat");

                    //get bevels
                    Vector3 currBevel = contour.GetIntersectingFaceBevelVertexDirection(i);
                    Vector3 prevBevel = contour.GetPrevFaceBevelVertexDirection(i);
                    Vector3 nextBevel = contour.GetNextFaceBevelVertexDirection(i);

                    // add pos with prev normal
                    // add pos with next normal
                    Vector3 prevNormal = contour.GetPrevFaceNormal(i);
                    Vector3 nextNormal = contour.GetNextFaceNormal(i);

                    contourAttributes.Add(new ContourVertexAttributes(curr, prevBevel, prevNormal, vDist, vDist));
                    contourAttributes.Add(new ContourVertexAttributes(curr, currBevel, prevNormal, vDist, vDist + Vector3.Distance(currBevel, prevBevel) * _meshParameter.Bevel));
                    contourAttributes.Add(new ContourVertexAttributes(curr, currBevel, nextNormal, vDist, vDist - Vector3.Distance(currBevel, nextBevel) * _meshParameter.Bevel));
                    contourAttributes.Add(new ContourVertexAttributes(curr, nextBevel, nextNormal, vDist, vDist));

                    //Debug.DrawRay(curr, prevNormal * _meshParameter.Bevel*10, Color.red, 10f);
                    //Debug.DrawRay(curr, nextNormal * _meshParameter.Bevel*10, Color.green, 10f);

                    //uvDist = Mathf.Round(uvDist);
                }
                else //is a very acute or obtuse angle
                {
                    //Debug.Log("goofy");

                    //get bevels
                    Vector3 currBevel = contour.GetAverageFaceBevelVertexDirection(i);
                    Vector3 prevBevel = contour.GetPrevFaceBevelVertexDirection(i);
                    Vector3 nextBevel = contour.GetNextFaceBevelVertexDirection(i);
                    
                    // add pos with prev normal
                    // add pos with next normal
                    Vector3 prevNormal = contour.GetPrevFaceNormal(i);
                    Vector3 nextNormal = contour.GetNextFaceNormal(i);

                    contourAttributes.Add(new ContourVertexAttributes(curr, prevBevel, prevNormal, vDist, vDist));
                    contourAttributes.Add(new ContourVertexAttributes(curr, currBevel, prevNormal, vDist, vDist + Vector3.Distance(currBevel, prevBevel) * _meshParameter.Bevel));
                    contourAttributes.Add(new ContourVertexAttributes(curr, currBevel, nextNormal, vDist, vDist - Vector3.Distance(currBevel, nextBevel) * _meshParameter.Bevel));
                    contourAttributes.Add(new ContourVertexAttributes(curr, nextBevel, nextNormal, vDist, vDist));

                }
                vDist += (contour.GetNext(i) - curr).magnitude;
                //Debug.Log("UV Dist: " + uvDist);
            }

            return contourAttributes;
        }



        /// <summary>
        /// Create depth mesh attributes.
        /// </summary>
        /// <param name="meshAttribs"></param>
        /// <param name="attributes"></param>
        protected void AddSidesToMesh(ref MeshAttributes meshAttribs)
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
                    var bevelPosition = attr.VertexPosition + attr.BevelVertexDirection * _meshParameter.Bevel;
                    meshAttribs.Verticies.Add(bevelPosition);
                    meshAttribs.Verticies.Add(bevelPosition + Vector3.forward * _meshParameter.Depth);

                    // add norms
                    meshAttribs.Normals.Add(attr.Normal);
                    meshAttribs.Normals.Add(attr.Normal);

                    // add uvs
                    meshAttribs.UVs.Add(new Vector3(0.0f, attr.VDistanceBevel));
                    meshAttribs.UVs.Add(new Vector3(_meshParameter.Depth, attr.VDistanceBevel));
                    /*
                    Debug.DrawRay(attr.BevelVertexPosition, attr.Normal * 0.1f, Color.cyan, 10f);
                    Debug.DrawRay(attr.VertexPosition, Vector3.back * 0.1f, Color.yellow, 10f);
                    Debug.DrawLine(attr.VertexPosition, attr.BevelVertexPosition, Color.magenta, 10f);
                    */
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

#endregion glyphrest

#endregion METHODS
    }
}