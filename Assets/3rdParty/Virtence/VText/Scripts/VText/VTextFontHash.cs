using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Virtence.Common.Utilities;

namespace Virtence.VText
{
    /// <summary>
    /// Font caching.
    /// </summary>
    public class VTextFontHash
    {
        #region FIELDS

        /// <summary>
        /// container of used fonts
        /// </summary>
        private static Hashtable _fonts;

        #endregion FIELDS

        #region METHOD

        /// <summary>
        /// Refresh the Font Hash. Copys all Fonts to persitient data path.
        /// (Call not recommended; used for debug Font Hash)
        /// </summary>
        public static void RefreshFontHash()
        {
            Debug.Log("Clear VText font hash");
            _fonts = new Hashtable();
            
            // check_delete target folder
            string targetFolder = Path.Combine(Application.persistentDataPath, "Fonts");
            if (Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder, true);
            }
            Directory.CreateDirectory(targetFolder);

            // initialize BSA
            BetterStreamingAssets.Initialize();

            // get ttf and otf files
            string[] ttfs = BetterStreamingAssets.GetFiles("Fonts", "*.ttf", SearchOption.TopDirectoryOnly);
            string[] otfs = BetterStreamingAssets.GetFiles("Fonts", "*.otf", SearchOption.TopDirectoryOnly);
            foreach (var ttf in ttfs)
            {
                //Debug.Log(ttf);
                File.WriteAllBytes(Path.Combine(Application.persistentDataPath, ttf), BetterStreamingAssets.ReadAllBytes(ttf));
            }
            foreach(var otf in otfs)
            {
                //Debug.Log(otf);
                File.WriteAllBytes(Path.Combine(Application.persistentDataPath, otf), BetterStreamingAssets.ReadAllBytes(otf));
            }
        }

        /// <summary>
        /// Returns a list of all available font file names.
        /// </summary>
        /// <returns>List with all available fonts.</returns>
        public static List<string> GetAvailableFonts()
        {
            var di = new DirectoryInfo(Path.Combine(Application.persistentDataPath, "Fonts"));
            FileInfo[] fiarray = di.GetFiles("*.*");
            List<string> result = new List<string>();

            foreach (var fi in fiarray)
            {
                if (!fi.Name.StartsWith(".") && (fi.Extension.ToUpper() == ".TTF" || fi.Extension.ToUpper() == ".OTF"))
                {
                    result.Add(fi.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// Calls the desired font from font hash or try to load it.
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="resultHandler"></param>
        public static void FetchFont(string fontName, Action<IVFont> resultHandler)
		{
			if (string.IsNullOrEmpty(fontName))
			{
				resultHandler?.Invoke(null);
			}

			if (_fonts == null)
			{
				RefreshFontHash();
			}

			Action<bool> loadFontHandler = (success) =>
			{
				resultHandler?.Invoke(_fonts[fontName] as IVFont);
			};

            if (!_fonts.Contains(fontName))
            {
                TryAddingFont(fontName, loadFontHandler);
            }
            else
			{
				IVFont fnt = (IVFont) _fonts[fontName];
				resultHandler?.Invoke(fnt);
			}
        }


        /// <summary>
        /// Load non-existing Typefont by searching for it in streaming assets
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="resultHandler"></param>
        private static void TryAddingFont(string fontName, Action<bool> resultHandler)
		{
			var filePath = Path.Combine(Application.streamingAssetsPath, "Fonts", fontName);			

			if (!(filePath.Contains("://") || filePath.Contains(":///")))
			{
				filePath = "file://" + filePath;
			}

			//Debug.Log("Try to open font: " + filePath);

			UnityWebRequest webRequest = new UnityWebRequest(filePath);
			webRequest.downloadHandler = new DownloadHandlerBuffer();

			Action<bool, UnityWebRequest> finishedHandler = (success, request) =>
			{
				if (success)
				{
					Stream result = new MemoryStream(request.downloadHandler.data);
                    var font = new Virtence.OpenTypeCS.OpenType().LoadFromStream(result);

                    if (font != null)
					{
						if (!_fonts.Contains(fontName))
						{
							//Debug.Log("Add font: " + fontName);
							_fonts.Add(fontName, new VFont(font));
						}
					}
				}

				resultHandler?.Invoke(success);
			};

			StaticCoroutine.Instance.StartCoroutine(WebRequestUtilities.SendRequest(webRequest, null, null, finishedHandler));
		}

        #endregion METHOD
    }
}