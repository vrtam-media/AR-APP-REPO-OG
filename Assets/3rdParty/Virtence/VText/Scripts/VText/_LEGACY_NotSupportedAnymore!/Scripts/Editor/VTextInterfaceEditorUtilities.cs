// ----------------------------------------------------------------------
// File: 			VTextInterfaceEditorUtilities
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2017 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace Virtence.VText.LEGACY.VTextEditor
{
	/// <summary>
	/// 
	/// </summary>
	public static class VTextInterfaceEditorUtilities 
	{
		#region EVENTS
		#endregion // EVENTS


		#region CONSTANTS

		#endregion // CONSTANTS


		#region FIELDS
		#endregion // FIELDS


		#region PROPERTIES
		#endregion // PROPERTIES


		#region CONSTRUCTORS
		#endregion // CONSTRUCTORS


		#region METHODS
		/// <summary>
		/// convert a hex string (without the "#") to a color32 fully opaque
		/// </summary>
		/// <returns>The to color.</returns>
		/// <param name="c">C.</param>
		/// <param name="hex">Hex.</param>
		public static Color HexToColor(this Color c, string hex)
		{
			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			return new Color32(r, g, b, 255);
		}

		/// <summary>
		/// draws an image with a border which is colored depending on the UnityEditor skin (personal (light) or professional (dark))
		/// draws the image with the specified width and calculates its height to keep the correct aspect ratio
		/// </summary>
		/// <param name="image">Image.</param>
		/// <param name="width">Width.</param>
		public static void DrawBorderedImage(Texture image, float width)
		{
			Color c = GUI.backgroundColor;
			if (EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color().HexToColor("ffffff");
			}
			else
			{
				GUI.backgroundColor = new Color().HexToColor("ffffff");
			}

			EditorGUILayout.BeginVertical("box");
			float aspectRatio = (float)image.height / image.width;
			EditorGUILayout.LabelField(new GUIContent(image), GUILayout.Width(width), GUILayout.Height(width * aspectRatio));
			EditorGUILayout.EndVertical();
			GUI.backgroundColor = c;
		}

		/// <summary>
		/// draws an image with a border which is colored depending on the UnityEditor skin (personal (light) or professional (dark))
		/// draws the image with the specified width and calculates its height to keep the correct aspect ratio
		/// </summary>
		/// <param name="image">Image.</param>
		/// <param name="width">Width.</param>
		public static void DrawBorderedText(string label)
		{
			Color c = GUI.backgroundColor;
			if (EditorGUIUtility.isProSkin)
			{
				GUI.backgroundColor = new Color().HexToColor("ffffff");
			}
			else
			{
				GUI.backgroundColor = new Color().HexToColor("ffffff");
			}

			EditorGUILayout.BeginVertical("box");

			TextAnchor currentAlignment = GUI.skin.label.alignment;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;			
			EditorGUILayout.LabelField(label, GUI.skin.label);
			EditorGUILayout.EndVertical();
			GUI.backgroundColor = c;

			GUI.skin.label.alignment = currentAlignment;
		}
		#endregion // METHODS


		#region EVENT HANDLERS
		#endregion // EVENT HANDLERS
	}
}