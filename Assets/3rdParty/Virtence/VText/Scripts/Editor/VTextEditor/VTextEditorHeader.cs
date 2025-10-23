// ----------------------------------------------------------------------
// File: 		VTextEditorHeader
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2014 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------
using UnityEditor;
using UnityEngine;

/// <summary>
///
/// </summary>
public class VTextEditorHeader : AbstractVTextEditorComponent
{
    #region FIELDS

    private Texture _logoImage;                       // the logo image of the header

    #endregion FIELDS



    #region CONSTRUCTORS

    public VTextEditorHeader(SerializedObject obj, UnityEditor.Editor currentEditor)
    {
        _logoImage = Resources.Load("Images/VTextLogo") as Texture;
    }

    #endregion CONSTRUCTORS

    #region METHODS

    #region implemented abstract members of AbstractVTextEditorComponent

    public override bool DrawUI()
    {
        if (_logoImage != null)
        {
            float aspectRatio = (float)_logoImage.height / _logoImage.width;
            float desiredWidth = EditorGUIUtility.currentViewWidth;
            float desiredHeight = 30;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            TextAnchor currentAlignment = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            //GUILayout.Label(_logoImage, GUILayout.Width(desiredWidth), GUILayout.Height(desiredWidth * aspectRatio));
            GUILayout.Label(_logoImage, GUILayout.Width(desiredHeight / aspectRatio), GUILayout.Height(desiredHeight));
            GUI.skin.label.alignment = currentAlignment;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        return false;
    }

    #endregion implemented abstract members of AbstractVTextEditorComponent

    #endregion METHODS
}