// ----------------------------------------------------------------------
// File: 		VTextEditorStyle
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2014 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Virtence.VTextEditor
{
    /// <summary>
    /// this class handles all editor stuff for the style of the text (font, materials, etc)
    /// </summary>
    public class VTextEditorStyle : AbstractVTextEditorComponent
    {
        #region CONSTANTS

        /// <summary>
        /// the width of the labels in front of controls
        /// </summary>
        private const float LABEL_WIDTH = 100;

        /// <summary>
        /// the width of images in the help sections
        /// </summary>
        private const float HELP_IMAGE_WIDTH = 50.0f;

        /// <summary>
        /// the height of the scroll view which shows the help text (in the help regions)
        /// </summary>
        private const float HELP_SCROLLVIEW_HEIGHT = 60.0f;

        #endregion CONSTANTS

        #region FIELDS

        private SerializedProperty _layoutParam;                // the layout parameter component of the vtext object
        private SerializedProperty _rendererParam;              // the renderer parameter component of the vtext object

        private SerializedProperty _pFontname;                  // the font name of the mesh
        private SerializedProperty _lSize;                      // the size of the text
        private SerializedProperty _renderParamsModified;       // the flag for modified renderparams

        private SerializedProperty _materials;                  // the style parameter component of the vtext object (materials etc)
        private SerializedProperty _pShadowCast;                // the shadow casting mode (in Unity 5 and higher)
        private SerializedProperty _pReceiveShadows;            // should the mesh receive shadows?
#if UNITY_2018_3_OR_NEWER
        private SerializedProperty _pLightProbeUsage;           // Unity 2018.3 uses more options for LightProbes
#else
        private SerializedProperty _pUseLightProbes;            // do we use light probes for the mesh?
#endif

        private SerializedProperty _pApplyLayerSettings;        // should the lettes have the same layer as the VText parent object?

        private VText.VText _vtext;                 // the reference to the used vtext interface

        private string[] _fontNames;                    // all available font names (including "none" at index 0)
        private int _currentFontIndex;                  // the current selected font index

        #region INFOFIELDS

        private AnimBool _showFontInfo = new AnimBool();                        // show or hide the font info
        private AnimBool _showSizeInfo = new AnimBool();                        // show or hide the size info
        private AnimBool _showFaceMaterialInfo = new AnimBool();                // show or hide the face material info
        private AnimBool _showSideMaterialInfo = new AnimBool();                // show or hide the side material info
        private AnimBool _showBevelMaterialInfo = new AnimBool();               // show or hide the bevel material info
        private AnimBool _showShadowCastInfo = new AnimBool();                  // show or hide the help for the shadow casting mode parameter (Unity5 and above)
        private AnimBool _showShadowCastInfoV4 = new AnimBool();                // show or hide the help for the shadow casting mode parameter (Unity4 - Unity5)
        private AnimBool _showReceiveShadowInfo = new AnimBool();               // show or hide the help for the receive shadow parameter
        private AnimBool _showLightProbesInfo = new AnimBool();                 // show or hide the help for the lightprobes parameter
        private AnimBool _showApplyLayerInfo = new AnimBool();                  // show or hide the help for the apply layer parameter

        private Texture _fontInfoHelpImage;                                     // the image which is shown in the font info help box
        private Texture _sizeInfoHelpImage;                                     // the image which is shown in the size help box
        private Texture _faceMaterialHelpImage;                                 // the image which is shown in the face material help box
        private Texture _sideMaterialHelpImage;                                 // the image which is shown in the side material help box
        private Texture _bevelMaterialHelpImage;                                // the image which is shown in the bevel material help box
        private Texture _shadowCastHelpImage;                                   // the image which is shown in the shadow casting mode help box
        private Texture _receiveShadowHelpImage;                                // the image which is shown in the receive shadow help box
        private Texture _lightProbesHelpImage;                                  // the image which is shown in the use lightprobes help box
        private Texture _applyLayerHelpImage;                                   // the image which is shown in the apply layer help box

        private Vector2 _fontInfoHelpTextScrollPosition = Vector2.zero;                 // the scrollview position for the font help text
        private Vector2 _sizeInfoHelpTextScrollPosition = Vector2.zero;                 // the scrollview position for the size help text
        private Vector2 _faceMaterialHelpTextScrollPosition = Vector2.zero;             // the scrollview position for the face material help text
        private Vector2 _sideMaterialHelpTextScrollPosition = Vector2.zero;             // the scrollview position for the side material help text
        private Vector2 _bevelMaterialHelpTextScrollPosition = Vector2.zero;            // the scrollview position for the bevel material help text
        private Vector2 _shadowCastInfoHelpTextScrollPosition = Vector2.zero;           // the scrollview position for the shadow casting mode help text
        private Vector2 _receiveShadowInfoHelpTextScrollPosition = Vector2.zero;        // the scrollview position for the receive shadow help text
        private Vector2 _lightProbesInfoHelpTextScrollPosition = Vector2.zero;          // the scrollview position for the use lightprobes help text
        private Vector2 _applyLayerInfoHelpTextScrollPosition = Vector2.zero;           // the scrollview position for the apply layer help text

        #endregion INFOFIELDS

        #endregion FIELDS

        #region CONSTRUCTORS

        public VTextEditorStyle(SerializedObject obj, UnityEditor.Editor currentEditor)
        {
            _vtext = obj.targetObject as VText.VText;

            _layoutParam = obj.FindProperty("LayoutParameter");
            _rendererParam = obj.FindProperty("RenderParameter");
            _materials = _rendererParam.FindPropertyRelative("_materials");

            _lSize = _layoutParam.FindPropertyRelative("_size");
            _renderParamsModified = _rendererParam.FindPropertyRelative("_modified");

            _pFontname = obj.FindProperty("MeshParameter").FindPropertyRelative("_fontName");

            _pShadowCast = _rendererParam.FindPropertyRelative("_shadowCastMode");
            _pReceiveShadows = _rendererParam.FindPropertyRelative("_receiveShadows");
#if UNITY_2018_3_OR_NEWER
            _pLightProbeUsage = _rendererParam.FindPropertyRelative("_lightProbeUsage");
#else
            _pUseLightProbes = _rendererParam.FindPropertyRelative("_useLightProbes");
#endif

            _pApplyLayerSettings = _rendererParam.FindPropertyRelative("_applyLayerSettings");

            // the images in the help screens
            _fontInfoHelpImage = Resources.Load("Images/Icons/Help/FontImage") as Texture;
            _sizeInfoHelpImage = Resources.Load("Images/Icons/Help/text_fontSize") as Texture;
            _faceMaterialHelpImage = Resources.Load("Images/Icons/Help/Letter_T_3D_FaceMaterial") as Texture;
            _bevelMaterialHelpImage = Resources.Load("Images/Icons/Help/Letter_T_3D_BevelMaterial") as Texture;
            _sideMaterialHelpImage = Resources.Load("Images/Icons/Help/Letter_T_3D_SideMaterial") as Texture;
            _shadowCastHelpImage = Resources.Load("Images/Icons/Help/Letter_DropShadow") as Texture;
            _receiveShadowHelpImage = Resources.Load("Images/Icons/Help/Letter_ReceiveShadow") as Texture;
            _lightProbesHelpImage = Resources.Load("Images/Icons/Help/Letter_M_3D_LightProbes") as Texture;
            _applyLayerHelpImage = Resources.Load("Images/Icons/Help/Letter_ApplyLayer") as Texture;

            // add repaints if the animated values are changed
            _showFontInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showSizeInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showFaceMaterialInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showSideMaterialInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showBevelMaterialInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showShadowCastInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showShadowCastInfoV4.valueChanged.AddListener(currentEditor.Repaint);
            _showReceiveShadowInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showLightProbesInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showApplyLayerInfo.valueChanged.AddListener(currentEditor.Repaint);

            SetupDefaultFont();
        }

        #endregion CONSTRUCTORS

        #region METHODS

        /// <summary>
        /// Setups the default font.
        /// </summary>
        public void SetupDefaultFont()
        {
            FillFonts(_vtext.MeshParameter.FontName);
            if (_fontNames.Length > 1 && _pFontname.stringValue == "")
            {
                _currentFontIndex = 1;

				_vtext.MeshParameter.FontName = _fontNames[_currentFontIndex];
				_pFontname.stringValue = _fontNames[_currentFontIndex];
				_vtext.MeshParameter.CheckClearRebuild();
			}
        }

        /// <summary>
        /// draw the ui for this component
        ///
        /// returns true if this aspect of the VText should be updated (mesh, layout, physics, etc)
        /// </summary>
        public override bool DrawUI()
        {
            bool rebuildMesh = false;

            EditorGUIUtility.labelWidth = LABEL_WIDTH;

            #region FONT
            EditorGUILayout.BeginHorizontal();
            FillFonts(_vtext.MeshParameter.FontName);
            int fc = EditorGUILayout.Popup("Font:", _currentFontIndex, _fontNames);

            if (fc != _currentFontIndex)
            {
                // Debug.Log("fontChoice " + fc);
                _currentFontIndex = fc;
                if (fc > 0)
                {
                    _vtext.MeshParameter.FontName = _fontNames[fc];
                    _pFontname.stringValue = _fontNames[fc];
                }
                else
                {
                    _vtext.MeshParameter.FontName = string.Empty;
                    _pFontname.stringValue = string.Empty;
                }
                rebuildMesh = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showFontInfo);
            EditorGUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showFontInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Font:") + "\n\n" +
                    "Here you can select the font which is used to generate the 3D Text.\n" +
                    "You can simply add additional fonts (TrueType or OTF fonts) by copying it into the" +
                    "<b><i>Assets/StreamingAssets/Fonts</i></b> folder.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowWarning("Be careful") + " when deleting or renaming fonts " +
                        "in this folder. If the fonts you are deleting or renaming are in use then the corresponding VText objects set their " +
                        "<b>Font</b> parameter to <i>'None'</i>. This will result in a not visible text the next time you rebuild the VText object (for " +
                        "instance at the start of the game).";

                DrawHelpWindow(_fontInfoHelpImage, txt, ref _fontInfoHelpTextScrollPosition, ref _showFontInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion FONT

            #region SIZE

            GUILayout.BeginHorizontal();
            float nSize = EditorGUILayout.FloatField(new GUIContent("Size:", "The size of the text"), _lSize.floatValue);
            if (nSize != _lSize.floatValue)
            {
                _lSize.floatValue = nSize;
                rebuildMesh = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showSizeInfo);
            EditorGUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showSizeInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Size:") + "\n\n" +
                    "This parameter defines the size of the generated text in meters.";

                DrawHelpWindow(_sizeInfoHelpImage, txt, ref _sizeInfoHelpTextScrollPosition, ref _showSizeInfo);
            }
            EditorGUILayout.EndFadeGroup();

			#endregion SIZE

			#region MATERIALS

			// face material
            GUILayout.BeginHorizontal();
			//Object faceMat = _materials.GetArrayElementAtIndex(0).objectReferenceValue;
			//Object nFaceMat = EditorGUILayout.ObjectField("Face material:", faceMat, typeof(Material), false);
			//if (nFaceMat != faceMat)
			//{
			//    _materials.GetArrayElementAtIndex(0).objectReferenceValue = nFaceMat;
			//    _renderParamsModified.boolValue = true;
			//}

			Object faceMat = _vtext.RenderParameter.Materials[0];
			Object nFaceMat = EditorGUILayout.ObjectField("Face material:", faceMat, typeof(Material), false);
			if (nFaceMat != faceMat)
			{
			    _vtext.RenderParameter.Materials[0] = nFaceMat as Material;
				_materials.GetArrayElementAtIndex(0).objectReferenceValue = nFaceMat;
			    _renderParamsModified.boolValue = true;
			}


			VTextEditorGUIHelper.DrawHelpButton(ref _showFaceMaterialInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showFaceMaterialInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Face material:") + "\n\n" +
                    "The material here is used for the front- and (if defined) backfaces of the generated 3D text.";
                DrawHelpWindow(_faceMaterialHelpImage, txt, ref _faceMaterialHelpTextScrollPosition, ref _showFaceMaterialInfo);
            }
            EditorGUILayout.EndFadeGroup();

			// side material
			GUILayout.BeginHorizontal();
			//Object sideMat = _materials.GetArrayElementAtIndex(1).objectReferenceValue;
			//Object nSideMat = EditorGUILayout.ObjectField("Side material:", sideMat, typeof(Material), false);
			//if (nSideMat != sideMat)
			//{
			//    _materials.GetArrayElementAtIndex(1).objectReferenceValue = nSideMat;
			//    _renderParamsModified.boolValue = true;
			//}

			Object sideMat = _vtext.RenderParameter.Materials[1];
			Object nSideMat = EditorGUILayout.ObjectField("Side material:", sideMat, typeof(Material), false);
			if (nSideMat != sideMat)
			{
				_vtext.RenderParameter.Materials[1] = nSideMat as Material;
				_materials.GetArrayElementAtIndex(1).objectReferenceValue = nSideMat;
				_renderParamsModified.boolValue = true;
			}

			VTextEditorGUIHelper.DrawHelpButton(ref _showSideMaterialInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showSideMaterialInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Side material:") + "\n\n" +
                    "The material here is used for the sides of the generated 3D text if it has any <b>depth</b>. You can change the depth of the 3D text in the " +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowCategoryLink("Mesh") + " category.";
                DrawHelpWindow(_sideMaterialHelpImage, txt, ref _sideMaterialHelpTextScrollPosition, ref _showSideMaterialInfo);
            }
            EditorGUILayout.EndFadeGroup();

            // bevel material
            GUILayout.BeginHorizontal();
			//Object currentBevelMaterial = _materials.GetArrayElementAtIndex(2).objectReferenceValue;
			//Object newBevelMaterial = EditorGUILayout.ObjectField("Bevel material:", currentBevelMaterial, typeof(Material), false);
			//if (newBevelMaterial != currentBevelMaterial)
			//{
			//    _materials.GetArrayElementAtIndex(2).objectReferenceValue = newBevelMaterial;
			//    _renderParamsModified.boolValue = true;
			//}

			Object currentBevelMaterial = _vtext.RenderParameter.Materials[2];
			Object newBevelMaterial = EditorGUILayout.ObjectField("Bevel material:", currentBevelMaterial, typeof(Material), false);
			if (newBevelMaterial != currentBevelMaterial)
			{
				_vtext.RenderParameter.Materials[2]= newBevelMaterial as Material;
				_materials.GetArrayElementAtIndex(2).objectReferenceValue = newBevelMaterial;
				_renderParamsModified.boolValue = true;
			}

			VTextEditorGUIHelper.DrawHelpButton(ref _showBevelMaterialInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showBevelMaterialInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Bevel material:") + "\n\n" +
                "The material here is used for the <b>bevel</b> of the generated 3D text. The bevel is the smoothly rounded part between the front/back-side of " +
                    "the 3D-Text and its sides. You can change the size of the <b>bevel</b> in the " + VTextEditorGUIHelper.ConvertStringToHelpWindowCategoryLink("Mesh") +
                    " category. It will only be available if the 3D-Text has a <b>depth</b>. You can change the <b>depth</b> of the 3D text " +
                    "in the " + VTextEditorGUIHelper.ConvertStringToHelpWindowCategoryLink("Mesh") + " category too.";

                DrawHelpWindow(_bevelMaterialHelpImage, txt, ref _bevelMaterialHelpTextScrollPosition, ref _showBevelMaterialInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion MATERIALS

            #region SHADOW CASTING MODE (UNITY 5 AND ABOVE)

            GUILayout.BeginHorizontal();
            var oShadowCast = _pShadowCast.enumValueIndex;
            EditorGUILayout.PropertyField(_pShadowCast, new GUIContent("Cast shadows:"));
            if (oShadowCast != _pShadowCast.enumValueIndex)
            {
                _renderParamsModified.boolValue = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showShadowCastInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showShadowCastInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Cast shadow modes:") + "\n\n" +
                    "Here you can set the way how Unity renders shadows for the generated text. \n" +
                    "The following modes are available:\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("OFF: ") + "No shadows are cast from this text.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("ON: ") + "Shadows are cast from this text.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("TwoSided: ") + "Shadows are cast from this text, treating it as two-sided. This way you can see the shadows also from inside a letter. " +
                    "Normally you don't need this.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("ShadowsOnly: ") + "The text will cast shadows, but is invisible otherwise in the scene.";
                DrawHelpWindow(_shadowCastHelpImage, txt, ref _shadowCastInfoHelpTextScrollPosition, ref _showShadowCastInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion SHADOW CASTING MODE (UNITY 5 AND ABOVE)

            #region RECEIVE SHADOWS

            GUILayout.BeginHorizontal();
            bool nReceiveShadows = EditorGUILayout.Toggle("Receive shadows:", _pReceiveShadows.boolValue);
            if (nReceiveShadows != _pReceiveShadows.boolValue)
            {
                _pReceiveShadows.boolValue = nReceiveShadows;
                _renderParamsModified.boolValue = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showReceiveShadowInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showReceiveShadowInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Receive shadows:") + "\n\n" +
                    "Enable this if you want your text to receive shadows from other objects.";
                DrawHelpWindow(_receiveShadowHelpImage, txt, ref _receiveShadowInfoHelpTextScrollPosition, ref _showReceiveShadowInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion RECEIVE SHADOWS

            #region LIGHTPROBES

#if UNITY_2018_3_OR_NEWER

            GUILayout.BeginHorizontal();
            var oLightProbeUsage = _pLightProbeUsage.enumValueIndex;
            EditorGUILayout.PropertyField(_pLightProbeUsage, new GUIContent("Light Probe Usage:"));
            if (oLightProbeUsage != _pLightProbeUsage.enumValueIndex)
            {
                _renderParamsModified.boolValue = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showLightProbesInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showLightProbesInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Light Probe Usage:") + "\n\n" +
                    "Here you can set the way how Unity uses Light Probes. \n" +
                    "The following modes are available:\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("OFF: ") + "No Light Probes in use.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("Blend Probes: ") + "Blending with Light Probes\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("Proxy Volumes: ") + "Use Proxy Volumes. Overrides could be used.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("Custom Provided: ") + "Use your own.";
                DrawHelpWindow(_lightProbesHelpImage, txt, ref _lightProbesInfoHelpTextScrollPosition, ref _showLightProbesInfo);
            }
            EditorGUILayout.EndFadeGroup();

#else
        GUILayout.BeginHorizontal();
        bool nUseLightProbes = EditorGUILayout.Toggle("Use lightprobes:", _pUseLightProbes.boolValue);
        if (nUseLightProbes != _pUseLightProbes.boolValue)
        {
            _pUseLightProbes.boolValue = nUseLightProbes;
            _renderParamsModified.boolValue = true;
        }

        VTextEditorGUIHelper.DrawHelpButton(ref _showLightProbesInfo);
        GUILayout.EndHorizontal();

        if (EditorGUILayout.BeginFadeGroup(_showLightProbesInfo.faded))
        {
            string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Use lightprobes:") + "\n\n" +
                "If you are using Unity's lightprobes and want them to affect the text then you should enable this parameter.";
            DrawHelpWindow(_lightProbesHelpImage, txt, ref _lightProbesInfoHelpTextScrollPosition, ref _showLightProbesInfo);
        }
        EditorGUILayout.EndFadeGroup();
#endif

            #endregion LIGHTPROBES

            #region APPLY LAYER
            GUILayout.BeginHorizontal();
            bool nApplyLayer = EditorGUILayout.Toggle("Apply layer to letters:", _pApplyLayerSettings.boolValue);
            if (nApplyLayer != _pApplyLayerSettings.boolValue)
            {
                _pApplyLayerSettings.boolValue = nApplyLayer;
                _renderParamsModified.boolValue = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showApplyLayerInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showApplyLayerInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Apply layer:") + "\n\n" +
                    "Enable this if you want to have each letter have the same layer like the VText main object.";
                DrawHelpWindow(_applyLayerHelpImage, txt, ref _applyLayerInfoHelpTextScrollPosition, ref _showApplyLayerInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion APPLY LAYER

            return rebuildMesh;
        }

        protected void AppendFontname(string fn)
        {
            string[] nfn = new string[_fontNames.Length + 1];
            for (int k = 0; k < _fontNames.Length; k++)
            {
                nfn[k] = _fontNames[k];
            }
            nfn[_fontNames.Length] = fn;
            _fontNames = nfn;
        }

        protected void FillFonts(string oldname)
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "Fonts");
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }
            DirectoryInfo di = new DirectoryInfo(directoryPath);
            if (di == null) {
                Debug.LogError("Folder not found: " + System.IO.Path.Combine(Application.persistentDataPath, "Fonts"));
                return;
            }
            FileInfo[] fiarray = di.GetFiles("*.*");
            _fontNames = new string[] { "(none)" };
            int fc = 0;
            // fontChoice = 0;

            foreach (FileInfo fi in fiarray)
            {
                // Debug.Log(fi.Name + " ext: " + fi.Extension);
                if (".ttf" == fi.Extension)
                {
                    if (oldname == fi.Name)
                    {
                        fc = _fontNames.Length;
                    }
                    AppendFontname(fi.Name);
                }
                else if (".otf" == fi.Extension)
                {
                    if (oldname == fi.Name)
                    {
                        fc = _fontNames.Length;
                    }
                    AppendFontname(fi.Name);
                }
            }
            if (fc != _currentFontIndex)
            {
                _currentFontIndex = fc;
            }
            // Debug.Log(fontnames);
        }

        #region HELP WINDOWS

        /// <summary>
        /// Draws the help window with the specified parameters
        /// </summary>
        private void DrawHelpWindow(Texture helpImage, string helpText, ref Vector2 helpTextScrollbarPosition, ref AnimBool showHelpWindowVariable)
        {
            int currentIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            // the image
            VTextEditorGUIHelper.DrawBorderedImage(helpImage, HELP_IMAGE_WIDTH);
            float imgHeight = (float)helpImage.height / helpImage.width * HELP_IMAGE_WIDTH;
            float borderOffset = 6.0f;      // there is a 3-pixel space to each side when put the image into a border (like we do)

            // the help text
            helpTextScrollbarPosition = GUILayout.BeginScrollView(helpTextScrollbarPosition, "box", GUILayout.Height(imgHeight + borderOffset));
            EditorGUILayout.LabelField(helpText, VTextEditorGUIHelper.HelpTextStyle);
            GUILayout.EndScrollView();

            // close button
            if (GUILayout.Button(new GUIContent("x", "Close help"), GUILayout.ExpandWidth(false)))
            {
                showHelpWindowVariable.target = false;
            }
            GUILayout.Space(5);     // space 5 pixel
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel = currentIndent;
        }

        #endregion HELP WINDOWS

        #endregion METHODS
    }
}