/*
 * $Id: VTextEditor.cs 174 2015-03-16 09:55:03Z dirk $
 *
 * Virtence VFont package
 * Copyright 2014 .. 2016 by Virtence GmbH
 * http://www.virtence.com
 *
 */

using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

#if UNITY_5
using UnityEngine.Rendering;
#endif

namespace Virtence.VText.LEGACY.VTextEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Virtence.VText.LEGACY.VTextInterface))]
    public class VTextInterfaceEditor : UnityEditor.Editor
    {
		#region CONSTANTS
		#endregion CONSTANTS

		#region FIELDS
		private Virtence.VText.LEGACY.VTextInterface _target;               // the vtext interface target
		#endregion FIELDS

		#region METHODS

		[MenuItem("GameObject/Virtence/Legacy/Convert old VText to new version ...")]
		private static void UpdateToNewVersion()
		{
			VTextInterface[] _oldVTexts = FindObjectsOfType<VTextInterface>();

			if (_oldVTexts != null && _oldVTexts.Length > 0)
			{
				VTextInterfaceToVTextConverter converter = new VTextInterfaceToVTextConverter();
				foreach (VTextInterface vi in _oldVTexts)
				{
					converter.DoConvert(vi);
				}

				if (!Application.isPlaying)
				{
					UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
				}
			}
		}

		private void OnEnable()
        {
            _target = target as Virtence.VText.LEGACY.VTextInterface;
        }

        public override void OnInspectorGUI()
        {
			VTextInterfaceEditorUtilities.DrawBorderedText("LEGACY - THIS DOES NOT WORK ANYMORE");

			if (GUILayout.Button("Update to new VText", GUILayout.Width(200), GUILayout.Height(30)))
			{				
				new VTextInterfaceToVTextConverter().DoConvert(_target);
			}

			DrawDefaultInspector();
        }

        #endregion METHODS

        #region EVENTHANDLERS
        #endregion EVENTHANDLERS
    }
}