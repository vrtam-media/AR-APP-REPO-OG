using System;
using System.Collections.Generic;
using UnityEngine;

namespace Virtence.VText
{
    /// <summary>
    /// Manages MeshRenderers of VText
    /// </summary>
    internal class VTextRenderer
    {
        #region EXPOSED

        //

        #endregion EXPOSED

        #region FIELDS

        /// <summary>
        /// Renderer params
        /// </summary>
        private VTextRenderParameter _renderParams;

        /// <summary>
        /// Mesh params. Needed for determin bevel and depth.
        /// </summary>
        private VTextMeshParameter _meshParams;

        /// <summary>
        /// The Renderer components of all childrean
        /// </summary>
        private List<MeshRenderer> _renderers;
        #endregion FIELDS

        #region PROPERTIES

        //

        #endregion PROPERTIES

        #region CONSTRUCTOR

        public VTextRenderer(VTextRenderParameter renderParams, VTextMeshParameter meshParams)
        {
            _renderParams = renderParams;
            _meshParams = meshParams;
            _renderers = new List<MeshRenderer>();
        }

        public VTextRenderer(VTextRenderParameter renderParams, VTextMeshParameter meshParams, List<MeshRenderer> renderers)
        {
            _renderParams = renderParams;
            _meshParams = meshParams;
            _renderers = renderers;
        }

        #endregion CONSTRUCTOR

        #region METHODS
        /// <summary>
        /// Add MeshRenderer Component to all given Transforms
        /// </summary>
        /// <param name="glyphs"></param>
        /// <returns></returns>
        internal List<MeshRenderer> AddRenderer(List<Transform> glyphs)
        {
            _renderers = new List<MeshRenderer>(glyphs.Count);
            // refresh children
            foreach (var t in glyphs)
            {
                var renderer = t.gameObject.AddComponent<MeshRenderer>();
                _renderers.Add(renderer);
            }
            Refresh();
            return _renderers;
        }

        /// <summary>
        /// Refresh MeshRenderer Parameter on given glyphs
        /// </summary>
        /// <param name="glyphs"></param>
        internal void UpdateRenderer(List<MeshRenderer> glyphs)
        {
            _renderers = glyphs;
            Refresh();
        }

        /// <summary>
        /// Assign all RenderParameter to _renderers
        /// </summary>
        private void Refresh()
        {
            List<Material> freshMats = new List<Material>(3);
            freshMats.Add(_renderParams.Materials[0]);
            if (_meshParams.Bevel > 0.0f)
            {
                if (_meshParams.Depth > 0.0f) // bevel with depth
                {
                    freshMats.Add(_renderParams.Materials[2]);
                    freshMats.Add(_renderParams.Materials[1]);
                }
                else // bevel without depth
                {
                    freshMats.Add(_renderParams.Materials[2]);
                }
            }
            else if (_meshParams.Depth > 0.0f) // depth without bevel
            {
                freshMats.Add(_renderParams.Materials[1]);
            }
            
            foreach (var renderer in _renderers)
            {
                renderer.shadowCastingMode = _renderParams.ShadowCastMode;
                renderer.receiveShadows = _renderParams.ReceiveShadows;
#if UNITY_2018_3_OR_NEWER
                renderer.lightProbeUsage = _renderParams.LightProbeUsage;
#else
                renderer.useLightProbes = _render.UseLightProbes;
#endif
                renderer.materials = freshMats.ToArray();

                // apply layer settings 
                if (_renderParams.ApplyLayerSettings) {
                    if (renderer.transform.parent != null) {
                        renderer.gameObject.layer = renderer.transform.parent.gameObject.layer;
                    }
                }
            }
        }
        #endregion METHODS
    }
}