// ----------------------------------------------------------------------
// File: 			VTextAutomaticInstaller
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Silvio Lange (silvio.lange@virtence.com)
// ----------------------------------------------------------------------

using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Virtence.VTextEditor
{
	/// <summary>
	/// 
	/// </summary>
	public class VTextAutomaticInstaller : AssetPostprocessor
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
        /// 
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
	    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
            return;

			var inPackages = importedAssets.Any(path => path.StartsWith("Packages/")) ||
				deletedAssets.Any(path => path.StartsWith("Packages/")) ||
				movedAssets.Any(path => path.StartsWith("Packages/")) ||
				movedFromAssetPaths.Any(path => path.StartsWith("Packages/"));

			if (inPackages)
			{                
                InitializeOnLoad();
            }
            VTextSetup.Init();
        }


        /// <summary>
        /// 
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            return;

            var listRequest = Client.List(true);
             
            while (!listRequest.IsCompleted)
                Thread.Sleep(100);

            if (listRequest.Error != null)
            {
                Debug.Log("Error: " + listRequest.Error.message);
                return;
            }

            var packages = listRequest.Result; 
            var text = new StringBuilder("Packages:\n");
            foreach (var package in packages)
            {
                if (package.source == PackageSource.Registry)
                    text.AppendLine($"{package.name}: {package.version} [{package.resolvedPath}]");
            }
            Debug.Log(text.ToString());
        }

        #endregion // METHODS

        #region EVENT HANDLERS
        #endregion // EVENT HANDLERS
    }
}
