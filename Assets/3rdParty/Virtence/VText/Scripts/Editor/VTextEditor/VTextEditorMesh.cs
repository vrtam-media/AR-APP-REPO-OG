// ----------------------------------------------------------------------
// File: 		VTextEditorMesh
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2014 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Virtence.VText;

namespace Virtence.VTextEditor
{
    /// <summary>
    /// this class handles all mesh associated aspects of the text (like depth, bevel, shadow-casting/receiving, etc)
    /// </summary>
    public class VTextEditorMesh : AbstractVTextEditorComponent
    {
        #region CONSTANTS

        /// <summary>
        /// the width of the labels in front of controls
        /// </summary>
        public const float LABEL_WIDTH = 100;

        /// <summary>
        /// the width of images in the help sections
        /// </summary>
        private const float HELP_IMAGE_WIDTH = 50.0f;

        /// <summary>
        /// the minimum width of an curve editor control
        /// </summary>
        private const float ANIMATION_CURVE_EDITOR_MIN_WIDTH = 100;

        #endregion CONSTANTS

        #region FIELDS
        private Texture _resetIcon;                             // the icon used for resetting the bending curves

        private SerializedProperty _meshParam;                  // the mesh parameter component of the vtext object

        private SerializedProperty _pDepth;                     // the depth of the text
        private SerializedProperty _pBevel;                     // the size of the bevel of the text
        private SerializedProperty _pBevelStyle;                // the bevel style
        private SerializedProperty _curveBevelProfile;          // the curve if the bevel style is set to "profile"

        private SerializedProperty _pGenerateTangents;
        private SerializedProperty _pHasBackface;               // should backfaces being created
        private SerializedProperty _pResolution;                // the quality of the tesselation

        private SerializedProperty _pUseFaceUVs;                // use face uv's or auto generated ones

        #region INFOFIELDS

        private AnimBool _showDepthInfo = new AnimBool();                       // show or hide the help for the depth parameter
        private AnimBool _showBevelInfo = new AnimBool();                       // show or hide the help for the bevel parameter
        private AnimBool _showBevelStyleInfo = new AnimBool();                  // show or hide the help for the bevel parameter
        private AnimBool _showBackfaceInfo = new AnimBool();                    // show or hide the help for the backface parameter
        private AnimBool _showQualityInfo = new AnimBool();                     // show or hide the help for the quality parameter
        private AnimBool _showTangentsInfo = new AnimBool();                    // show or hide the help for the tangents parameter
        private AnimBool _showUseFaceUVs = new AnimBool();                      // show or hide the help for the tangents parameter

        private Texture _depthHelpImage;                                        // the image which is shown in the depth help box
        private Texture _bevelHelpImage;                                        // the image which is shown in the bevel help box
        private Texture _bevelStyleHelpImage;                                   // the image which is shown in the bevel style help box
        private Texture _backfaceHelpImage;                                     // the image which is shown in the backface help box
        private Texture _qualityHelpImage;                                      // the image which is shown in the quality help box
        private Texture _tangentsHelpImage;                                     // the image which is shown in the tangents help box
        private Texture _useFaceUVsImage;                                       // the image which is shown in the tangents help box

        private Vector2 _depthInfoHelpTextScrollPosition = Vector2.zero;                // the scrollview position for the depth help text
        private Vector2 _bevelInfoHelpTextScrollPosition = Vector2.zero;                // the scrollview position for the bevel help text
        private Vector2 _bevelStyleInfoHelpTextScrollPosition = Vector2.zero;           // the scrollview position for the bevel help text
        private Vector2 _backfaceInfoHelpTextScrollPosition = Vector2.zero;             // the scrollview position for the backface help text
        private Vector2 _qualityInfoHelpTextScrollPosition = Vector2.zero;              // the scrollview position for the quality help text
        private Vector2 _tangentsInfoHelpTextScrollPosition = Vector2.zero;             // the scrollview position for the tangents help text
        private Vector2 _useFaceUVsTextScrollPosition = Vector2.zero;                   // the scrollview position for the user face uv's help text

        #endregion INFOFIELDS

        #endregion FIELDS

        #region CONSTRUCTORS

        public VTextEditorMesh(SerializedObject obj, UnityEditor.Editor currentEditor)
        {
            _meshParam = obj.FindProperty("MeshParameter");

            _pDepth = _meshParam.FindPropertyRelative("_depth");
            _pBevel = _meshParam.FindPropertyRelative("_bevel");
            _pBevelStyle = _meshParam.FindPropertyRelative("_bevelStyle");
            _curveBevelProfile = _meshParam.FindPropertyRelative("_bevelProfile");

            _pGenerateTangents = _meshParam.FindPropertyRelative("_generateTangents");
            _pHasBackface = _meshParam.FindPropertyRelative("_hasBackface");
            _pResolution = _meshParam.FindPropertyRelative("_resolution");
            _pUseFaceUVs = _meshParam.FindPropertyRelative("_useFaceUVs");

            // the images used for reset curves
            if (EditorGUIUtility.isProSkin)
            {
                _resetIcon = Resources.Load("Images/Icons/Help/icon_reset") as Texture;
            }
            else
            {
                _resetIcon = Resources.Load("Images/Icons/Help/icon_reset_dark") as Texture;
            }

            // the images in the help screens
            _depthHelpImage = Resources.Load("Images/Icons/Help/Letter_T_3D_SideMaterial") as Texture;
            _bevelHelpImage = Resources.Load("Images/Icons/Help/Letter_T_3D_BevelMaterial") as Texture;
            _bevelStyleHelpImage = Resources.Load("Images/Icons/Help/Letter_T_3D_BevelMaterial") as Texture;
            _backfaceHelpImage = Resources.Load("Images/Icons/Help/Letter_T_3D_FaceMaterial") as Texture;
            _qualityHelpImage = Resources.Load("Images/Icons/Help/text_quality") as Texture;
            _tangentsHelpImage = Resources.Load("Images/Icons/Help/Text_NormalMapping") as Texture;
            _useFaceUVsImage = Resources.Load("Images/Icons/Help/UV") as Texture;

            // add repaints if the animated values are changed
            _showDepthInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showBevelInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showBevelStyleInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showBackfaceInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showQualityInfo.valueChanged.AddListener(currentEditor.Repaint);
            _showTangentsInfo.valueChanged.AddListener(currentEditor.Repaint);
        }

        #endregion CONSTRUCTORS

        #region METHODS

        /// <summary>
        /// draw the ui for this component
        ///
        /// returns true if this aspect of the VText should be updated (mesh, layout, physics, etc)
        /// </summary>
        public override bool DrawUI()
        {
            bool updateMesh = false;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;

            #region DEPTH

            // depth
            GUILayout.BeginHorizontal();
            float nDepth = EditorGUILayout.FloatField("Depth:", _pDepth.floatValue);
            if (nDepth < 0.0f)
            {
                nDepth = 0.0f;
            }

            if (nDepth != _pDepth.floatValue)
            {
                _pDepth.floatValue = nDepth;
                updateMesh = true;
            }

            VTextEditorGUIHelper.DrawHelpButton(ref _showDepthInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showDepthInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Depth:") + "\n\n" +
                    "The <b>depth</b> defines the amount of extrusion of the font. \n" +
                    "Keep in mind that you can <b>bevel</b> your text only if a depth is set.";
                DrawHelpWindow(_depthHelpImage, txt, ref _depthInfoHelpTextScrollPosition, ref _showDepthInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion DEPTH

            #region BEVEL

            GUILayout.BeginHorizontal();
            float nBevel = Mathf.Clamp01(EditorGUILayout.FloatField("Bevel:", _pBevel.floatValue));
            if (nBevel != _pBevel.floatValue)
            {
                _pBevel.floatValue = nBevel;
                updateMesh = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showBevelInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showBevelInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Bevel:") + "\n\n" +
                    "The <b>bevel</b> is a smoothly rounded geometry between the front/back-faces of the text and it's side (see <b>depth</b>).\n" +
                    "You can set the bevel <b><color=#cc2222>only</color></b> if a <b>depth</b> is set.";
                DrawHelpWindow(_bevelHelpImage, txt, ref _bevelInfoHelpTextScrollPosition, ref _showBevelInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion BEVEL

            #region BEVELSTYLE
            GUILayout.BeginHorizontal();
            var originalBevelStyle = _pBevelStyle.enumValueIndex;
            EditorGUILayout.PropertyField(_pBevelStyle, new GUIContent("Bevel style:"));
            if (originalBevelStyle != _pBevelStyle.enumValueIndex)
            {
                updateMesh = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showBevelStyleInfo);
            GUILayout.EndHorizontal();

            #region BevelStyleProfile
            if (_pBevelStyle.enumValueIndex == (int)BevelStyle.Profile) {
                EditorGUILayout.BeginHorizontal();
                AnimationCurve curveBevelProfile = _curveBevelProfile.animationCurveValue;
                AnimationCurve animationCurveBevelProfile = EditorGUILayout.CurveField(new GUIContent("Bevel profile:", "The profile of the bevel"), curveBevelProfile, GUILayout.MinWidth(ANIMATION_CURVE_EDITOR_MIN_WIDTH));
                if (!animationCurveBevelProfile.Equals(curveBevelProfile))
                {
                    _curveBevelProfile.animationCurveValue = animationCurveBevelProfile;
                    updateMesh = true;
                }

                if (GUILayout.Button(new GUIContent(_resetIcon, "Reset curve"), GUILayout.Width(20), GUILayout.Height(VTextEditorGUIHelper.HELP_BUTTON_HEIGHT)))
                {
                    _curveBevelProfile.animationCurveValue = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
                    updateMesh = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            #endregion BEVELSTYLEPROFILE

            if (EditorGUILayout.BeginFadeGroup(_showBevelStyleInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Bevel styles:") + "\n\n" +
                    "Here you can set the way how the bevel sides are formed. \n" +
                    "The following modes are available:\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("Round: ") + "Round to the outside.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("Flat: ") + "Just flat bevels without smoothing them to any direction.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("Chiseled: ") + "Round the bevel to the inner side.\n\n" +
                    VTextEditorGUIHelper.ConvertStringToHelpWindowListItem("Profile: ") + "Define your own bevel style by using an animation curve.";
                DrawHelpWindow(_bevelStyleHelpImage, txt, ref _bevelStyleInfoHelpTextScrollPosition, ref _showBevelStyleInfo);
            }
            EditorGUILayout.EndFadeGroup();
            #endregion BEVELSTYLE

            #region BACKFACE

            GUILayout.BeginHorizontal();
            bool nBackface = EditorGUILayout.Toggle("Backface:", _pHasBackface.boolValue);
            if (nBackface != _pHasBackface.boolValue)
            {
                _pHasBackface.boolValue = nBackface;
                updateMesh = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showBackfaceInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showBackfaceInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Backface:") + "\n\n" +
                    "For performance reasons we normally do not create faces on the backside of the text " +
                    "because most of the texts are only visible from the front (menus, scores, etc).\n" +
                    "But in a couple of cases (for instance if you rotate your text) you want to see the backside too. " +
                    "Here you can enable the backfaces. The corresponding <b>bevels</b> will be created too.";
                DrawHelpWindow(_backfaceHelpImage, txt, ref _backfaceInfoHelpTextScrollPosition, ref _showBackfaceInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion BACKFACE

            #region QUALITY

            GUILayout.BeginHorizontal();
            float oldRes = 100f * ((_pResolution.floatValue - 0.0001f) * 101.010101f);
            float newRes = EditorGUILayout.Slider("Resolution:", oldRes, 0, 100);
            newRes = 0.0001f + 0.000099f * newRes;
            if (newRes != _pResolution.floatValue)
            {
                _pResolution.floatValue = newRes;
                updateMesh = true;
            }

            VTextEditorGUIHelper.DrawHelpButton(ref _showQualityInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showQualityInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Resolution:") + "\n\n" +
                    "With this parameter you can increase the number of triangles which are generated for this text.\n" +
                    "The idea is simple ... the higher the quality the smoother the geometry but the worse the performance.\n" +
                    "We already increase the number of triangles only for the curved parts of a letter.\n" +
                    "So here is your parameter to find a tradeoff which fits your needs. To see the results its a good idea to change " +
                    "the rendermode of the " + VTextEditorGUIHelper.ConvertStringToHelpWindowCategoryLink("Scene view") +
                    " to <b>Wireframe</b> or <b>Shaded wireframe</b>.";
                DrawHelpWindow(_qualityHelpImage, txt, ref _qualityInfoHelpTextScrollPosition, ref _showQualityInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion QUALITY

            #region USE_FACE_UV

            GUILayout.BeginHorizontal();
            bool nUseFaceUV = EditorGUILayout.Toggle("Use face UV's:", _pUseFaceUVs.boolValue);
            if (nUseFaceUV != _pUseFaceUVs.boolValue)
            {
                _pUseFaceUVs.boolValue = nUseFaceUV;
                updateMesh = true;
            }
            VTextEditorGUIHelper.DrawHelpButton(ref _showUseFaceUVs);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showUseFaceUVs.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Use FaceUV's:") + "\n\n" +
                    "Toggle between face UV's and auto gneratet UV's";
                DrawHelpWindow(_useFaceUVsImage, txt, ref _useFaceUVsTextScrollPosition, ref _showUseFaceUVs);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion USE_FACE_UV

            #region TANGENTS

            GUILayout.BeginHorizontal();
            bool nNeedTangents = EditorGUILayout.Toggle("Create tangents:", _pGenerateTangents.boolValue);
            if (nNeedTangents != _pGenerateTangents.boolValue)
            {
                _pGenerateTangents.boolValue = nNeedTangents;
                updateMesh = true;
            }

            VTextEditorGUIHelper.DrawHelpButton(ref _showTangentsInfo);
            GUILayout.EndHorizontal();

            if (EditorGUILayout.BeginFadeGroup(_showTangentsInfo.faded))
            {
                string txt = VTextEditorGUIHelper.ConvertStringToHelpWindowHeader("Create tangents:") + "\n\n" +
                    "Some shader (for instance normal map shaders) require tangents to work correctly. If you use such a shader for the text you should enable " +
                    "this parameter.";
                DrawHelpWindow(_tangentsHelpImage, txt, ref _tangentsInfoHelpTextScrollPosition, ref _showTangentsInfo);
            }
            EditorGUILayout.EndFadeGroup();

            #endregion TANGENTS

            return updateMesh;
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