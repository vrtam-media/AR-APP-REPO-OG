// ----------------------------------------------------------------------
// File: 			WebRequestUtilities
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2019 Virtence GmbH. All rights reserved
// Author:       	Michael Bißmann (michael.bissmann@virtence.com)
// ----------------------------------------------------------------------

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Virtence.Common.Utilities
{
	[UnityEngine.Scripting.Preserve]
	public class WebRequestUtilities
    {
        public static IEnumerator SendRequest(UnityWebRequest request, System.Action<UnityWebRequest> startedCallback, System.Action<UnityWebRequest> progressCallback, System.Action<bool, UnityWebRequest> finishedCallback, bool disposeAfterFinishedEvent = true)
        {
            request.SendWebRequest();
            startedCallback?.Invoke(request);

            while (!request.isDone)
            {
                progressCallback?.Invoke(request);
                yield return null;
            }

            if (request.isNetworkError || request.isHttpError)
            {
                //Debug.LogError("SendRequest failed: \n" + request.url + "\n\n " + request.error);
                finishedCallback?.Invoke(false, request);
                yield break;
            }

            finishedCallback?.Invoke(true, request);

            if (disposeAfterFinishedEvent)
            {
                //Debug.Log("WebRequestUtils.SendRequest: Dispose request!");
                request.Dispose();
            }
        }

        public static void PrintDownloadProgress(UnityWebRequest request)
        {
            Debug.Log("DownloadProgress: " + request.downloadProgress + " downloaded: " + request.downloadedBytes);
        }
        public static void PrintUploadProgress(UnityWebRequest request)
        {
            Debug.Log("UploadProgress: " + request.uploadProgress + " uploaded: " + request.uploadedBytes);
        }
    }
}