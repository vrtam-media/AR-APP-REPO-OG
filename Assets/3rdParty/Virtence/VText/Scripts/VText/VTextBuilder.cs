using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Virtence.VText
{
    /// <summary>
    /// create 3D text
    /// </summary>
    internal class VTextBuilder
    {
        #region EXPOSED

        public event EventHandler<GenericEventArgs<string>> FontLoadingFailed;               // this is raised if the font loading failed

        #endregion EXPOSED

        #region FIELDS

        /// <summary>
        /// Transform of VText
        /// </summary>
        private Transform _root;

        /// <summary>
        /// Children of VText
        /// </summary>
        private readonly List<Transform> _children;

        /// <summary>
        /// Meshs of children.
        /// </summary>
        private IEnumerable<MeshFilter> _meshFilter;

        /// <summary>
        /// MeshRenderer of children
        /// </summary>
        private List<MeshRenderer> _renderers;

        /// <summary>
        /// Mesh params
        /// </summary>
        private VTextMeshParameter _meshParameter;

        /// <summary>
        /// Renderer params
        /// </summary>
        private VTextRenderParameter _renderingParameter;

        /// <summary>
        /// Layout params
        /// </summary>
        private VTextLayoutParameter _layout;

        /// <summary>
        /// the glyph builder
        /// </summary>
        private VTextGlyphBuilder _glyphBuilder = null;

        /// <summary>
        /// the last used font
        /// </summary>
        private IVFont _lastFont = null;

        #endregion FIELDS

        #region PROPERTIES

        //

        #endregion PROPERTIES

        #region CONSTRUCTOR

        internal VTextBuilder(Transform root,
                              VTextMeshParameter meshParameter,
                              VTextRenderParameter renderingParameter,
                              VTextLayoutParameter layout)
        {
            _root = root;
            _meshParameter = meshParameter;
            _renderingParameter = renderingParameter;
            _layout = layout;
            _children = new List<Transform>();
            for (int i = 0; i < root.childCount; ++i)
            {
                _children.Add(root.GetChild(i));
            }
        }

        #endregion CONSTRUCTOR

        #region METHODS

        /// <summary>
        /// try to set a default font
        /// </summary>
        internal void SetDefaultFont() {
            string directoryPath = Path.Combine(Application.persistentDataPath, "Fonts");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            DirectoryInfo di = new DirectoryInfo(directoryPath);
            if (di == null)
            {
                Debug.LogError("Folder not found: " + System.IO.Path.Combine(Application.persistentDataPath, "Fonts"));
                return;
            }

            FileInfo[] fiarray = di.GetFiles("*.*");
            foreach (FileInfo fi in fiarray)
            {
                if (fi.Extension == ".otf" || fi.Extension == ".ttf")
                {
                    _meshParameter.FontName = "";
                    _meshParameter.FontName = fi.Name;
                    break;
                }
            }
        }

        /// <summary>
        /// Rebuild all children
        /// </summary>
        internal void Rebuild(Action<bool> finished)
        {
			Action<IVFont> fontLoaded = (font) =>
			{

				ClearChildren();
				//Debug.Log("Rebuild");

				if (font != null)
				{
					// layout glyphs
					var layouter = new VTextLayouter(_root, _layout, _meshParameter.Depth);
					layouter.Layout(_meshParameter.Text, _children, font, false);

					font.GlyphMeshAttributesHash = new Dictionary<char, MeshAttributes>();

					// create glyphs
					_glyphBuilder = new VTextGlyphBuilder(_meshParameter, font);


					_meshFilter = _glyphBuilder.AddGlyphMeshFilters(_children, _layout.Size);

					// add mesh renderer
					var render = new VTextRenderer(_renderingParameter, _meshParameter);
					_renderers = render.AddRenderer(_children);
					finished?.Invoke(true);
				}
				else
				{
					Debug.LogWarning(string.Format("Please choose valid font: '{0}' ----> {1}", _meshParameter.FontName, font));
                    FontLoadingFailed?.Invoke(this, new GenericEventArgs<string>(_meshParameter.FontName));
                    SetDefaultFont();
					finished?.Invoke(false);
				}

#if UNITY_EDITOR				
				UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
				//Editor.Repaint();
#endif

			};
            VTextFontHash.FetchFont(_meshParameter.FontName, fontLoaded);
        }

        /// <summary>
        /// Modify children
        /// </summary>
        internal void Modify(Action<bool> finished)
        {
            Action<IVFont> fontLoaded = (font) => {

                ClearChildren();
                //Debug.Log("Modify");
                if (font != null)
                {
                    // layout glyphs
                    var layouter = new VTextLayouter(_root, _layout, _meshParameter.Depth);
                    layouter.Layout(_meshParameter.Text, _children, font, false);

                    // create glyphs
                    _glyphBuilder = new VTextGlyphBuilder(_meshParameter, font);                    

                    _meshFilter = _glyphBuilder.AddGlyphMeshFilters(_children, _layout.Size);

                    // add mesh renderer
                    var render = new VTextRenderer(_renderingParameter, _meshParameter);
                    _renderers = render.AddRenderer(_children);

                    finished?.Invoke(true);
                }
                else
                {
                    Debug.LogWarning(string.Format("Please choose valid font: '{0}' ----> {1}", _meshParameter.FontName, font));
					FontLoadingFailed?.Invoke(this, new GenericEventArgs<string>(_meshParameter.FontName));
					SetDefaultFont();
                    finished?.Invoke(false);
                }
            };

            VTextFontHash.FetchFont(_meshParameter.FontName, fontLoaded);
        }

        /// <summary>
        /// Destroy all children
        /// </summary>
        private void ClearChildren()
        {
            if (_root == null) {
                _children.Clear();
                return;
            }

            //Debug.Log("Killing the younglings.");
            for (int i = _root.childCount - 1; i >= 0; i--)
            {
                var go = _root.GetChild(i).gameObject;

                // kill mesh explicit
                var filter = go.GetComponent<MeshFilter>();
                if (filter != null)
                {
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(filter.sharedMesh);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(filter.sharedMesh);
                    }
                }

                var renderer = go.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    if (Application.isPlaying)
                    {
                        for (int j = 0; j < renderer.materials.Length; ++j)
                        {
                            GameObject.Destroy(renderer.materials[j]);
                        }
                    }
                }
                if (Application.isPlaying)
                {
                    GameObject.Destroy(go);
                }
                else
                {
                    GameObject.DestroyImmediate(go);
                }
            }

            _children.Clear();
        }

        #endregion METHODS
    }
}