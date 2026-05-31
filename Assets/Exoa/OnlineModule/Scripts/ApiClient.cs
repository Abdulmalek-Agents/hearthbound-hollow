using Exoa.Designer;
using Exoa.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Exoa.API
{
    /// <summary>
    /// API client is mainly responible for making the HTTP call to the API backend.
    /// </summary>
    public class ApiClient
    {
        public static bool isFailing;
        public static bool isFailed;
        public static float failingStartTime;
        public static float apiMaxFailureTime = 60f;

        public bool debug = true;
        public bool allowRetries = true;
        private int retriesCount = 0;
        private int retryInterval = 2;

        private string url;
        private Method method;
        private Dictionary<string, string> queryParams;
        private string postBody;
        private Dictionary<string, string> headerParams;
        private Dictionary<string, string> formParams;
        private string[] authSettings;
        private Action<UnityWebRequest, string> successCallback = null;
        private byte[] fileContent = null;
        private Action<UnityWebRequest, string> errorCallback = null;
        private bool failOnSuccessFalse;
        private bool failOnEmptyResponse;

        private readonly Dictionary<string, string> _defaultHeaderMap = new Dictionary<string, string>();


        public enum Method { GET, POST, PUT, PATCH, DELETE };

        public ApiClient()
        {
#if LOG_ONLINE
            debug = true;
#else
            debug = false;
#endif
        }

        public void CallApi(MonoBehaviour mb,
                            string pUrl,
                            Method pMethod,
                            Action<UnityWebRequest, string> pCallback = null,
                            Action<UnityWebRequest, string> pErrorCallback = null,
                            Dictionary<string, string> pFormParams = null,
                            Dictionary<string, string> pQueryParams = null,
                            string pPostBody = null,
                            Dictionary<string, string> pHeaderParams = null,
                            string[] pAuthSettings = null,
                            byte[] pFileContent = null,
                            bool pFailOnSuccessFalse = true,
                            bool pFailOnEmptyResponse = true)
        {

            url = pUrl;
            method = pMethod;
            queryParams = pQueryParams;
            postBody = pPostBody;
            headerParams = pHeaderParams;
            formParams = pFormParams;
            authSettings = pAuthSettings;
            successCallback = pCallback;
            fileContent = pFileContent;
            errorCallback = pErrorCallback;
            failOnSuccessFalse = pFailOnSuccessFalse;
            failOnEmptyResponse = pFailOnEmptyResponse;

            UnityWebRequest r = BakeRequest();
            mb.StartCoroutine(RequestCorout(r, successCallback, errorCallback));
        }

        private UnityWebRequest BakeRequest()
        {
            if (url.Contains("?") == false)
                url += "?";

            UnityWebRequest r = new UnityWebRequest(url, method.ToString());
            r.useHttpContinue = false;

            r.downloadHandler = new DownloadHandlerBuffer();

            List<IMultipartFormSection> form = new List<IMultipartFormSection>();

            string verbose = "";

            if (fileContent != null)
            {
                form.Add(new MultipartFormFileSection("file", fileContent));
            }

            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    verbose += "\n Adding queryParams: " + param.Key + "=" + param.Value;
                    if (method == Method.GET)
                    {
                        r.url += param.Key + "=" + param.Value + "&";
                    }
                    else
                    {
                        form.Add(new MultipartFormDataSection(param.Key, param.Value));
                    }
                }
            }
            // add form parameter, if any
            if (formParams != null)
            {
                foreach (var param in formParams)
                {
                    verbose += "\n Adding formParams: " + param.Key + "=" + param.Value;
                    if (method == Method.GET)
                    {
                        r.url += param.Key + "=" + param.Value + "&";
                    }
                    else
                    {
                        form.Add(new MultipartFormDataSection(param.Key, param.Value));
                    }
                }
            }

            if (postBody != null) // http body (model) parameter
            {
                verbose += "\n Adding postBody: " + postBody;
                byte[] bodyRaw2 = Encoding.UTF8.GetBytes(postBody);
                r.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw2);
            }
            if (form.Count > 0)
            {
                verbose += "\n Converting to post request";
                r = UnityWebRequest.Post(url, form);
            }

            // add default header, if any
            foreach (var defaultHeader in _defaultHeaderMap)
            {
                verbose += "\n Adding defaultHeader " + defaultHeader.Key + "=" + defaultHeader.Value;
                r.SetRequestHeader(defaultHeader.Key, defaultHeader.Value);
            }
            // add header parameter, if any
            if (headerParams != null)
            {
                foreach (var param in headerParams)
                {
                    verbose += "\n Adding headerParams " + param.Key + "=" + param.Value;
                    r.SetRequestHeader(param.Key, param.Value);
                }
            }
            r.SetRequestHeader("Accept", "application/json");
            //r.SetRequestHeader("Content-Type", "application/json");

            if (debug)
                HDLogger.Log("Calling " + method.ToString() + " " + r.url + verbose, HDLogger.LogCategory.Online);

            return r;
        }


        public IEnumerator RequestCorout(UnityWebRequest r, Action<UnityWebRequest, string> successCallback, Action<UnityWebRequest, string> errorCallback)
        {
            using (r)
            {
                // Request and wait for the desired page.
                yield return r.SendWebRequest();

                while (r.downloadHandler == null)
                {
                    yield return new WaitForSeconds(1f);
                    if (debug) HDLogger.Log("Waiting for download handler", HDLogger.LogCategory.Online);
                }
                if (r.downloadHandler != null && debug)
                {
                    //if (debug) HDLogger.Log("Result data:" + r.downloadHandler.data);
                    //if (debug) HDLogger.Log("Result text:" + r.downloadHandler.text);

                }


                if (r.isNetworkError || r.isHttpError)
                {
                    HDLogger.LogError("Error: " + r.error + " " + r.url, HDLogger.LogCategory.Online);
                    if (!isFailing)
                    {
                        isFailing = true;
                        failingStartTime = Time.time;
                    }
                    if (!allowRetries)
                    {
                        errorCallback?.Invoke(r, r.error);
                    }
                    else if (!isFailed && CanRetry())
                    {
                        retriesCount++;
                        LogFailure();

                        if (debug) HDLogger.Log("Waiting " + retryInterval + "s before trying again...", HDLogger.LogCategory.Online);

                        yield return new WaitForSeconds(retryInterval);
                        if (debug) HDLogger.Log("Retrying to call: " + r.method + " " + r.url, HDLogger.LogCategory.Online);
                        UnityWebRequest r2 = BakeRequest();
                        yield return RequestCorout(r2, successCallback, errorCallback);
                    }
                    else if (!isFailed && !CanRetry())
                    {
                        StopAPI();
                        errorCallback?.Invoke(r, r.error);
                    }
                }
                else
                {
                    isFailing = isFailing = false;
                    //if (debug) HDLogger.Log("Request sent successfully!", HDLogger.LogCategory.Online);

                    var data = r.downloadHandler.data;
                    string text = "";
                    try
                    {
                        if (data[0] == 0x1f && data[1] == 0x8b)
                        {
                            if (debug) HDLogger.Log("Unzipping content", HDLogger.LogCategory.Online);
                            text = Unzip(data);
                        }
                        else
                        {
                            // plane data
                            text = r.downloadHandler.text;
                        }
                    }
                    catch (Exception)
                    {
                        text = r.downloadHandler.text;
                    }
                    //if (debug) HDLogger.Log("text:" + text, HDLogger.LogCategory.Online);
                    if (CheckValidResponse(r, text, out string errorMsg))
                        successCallback?.Invoke(r, text);
                    else
                        errorCallback?.Invoke(r, errorMsg);
                }
            }
        }

        public class BasicResponse
        {
            public bool success;
            public string error;
        }

        public bool CheckValidResponse(UnityWebRequest request, string response, out string errorMsg)
        {
            //Debug.Log("CheckValidResponse failOnSuccessFalse:" + failOnSuccessFalse + " response:" + response + " contains success:" + response.Contains("success"));
            errorMsg = null;
            if (((int)request.responseCode) >= 400 || ((int)request.responseCode) == 0)
            {
                errorMsg = "Error: " + request.responseCode + " " + request.error + " : " + request.downloadHandler.text;
                HDLogger.LogError("CheckValidResponse " + errorMsg, HDLogger.LogCategory.Online);
                return false;
            }
            if (string.IsNullOrEmpty(response) && failOnEmptyResponse)
            {
                errorMsg = "Error: Response from API call is empty";
                HDLogger.LogError("CheckValidResponse " + errorMsg, HDLogger.LogCategory.Online);
                return false;
            }
            if (failOnSuccessFalse && response.Contains("success"))
            {
                BasicResponse res = JsonConvert.DeserializeObject<BasicResponse>(response);
                if (res.success == false)
                {
                    errorMsg = "Error: API call failed (success:false):" + res.error;
                    HDLogger.LogError("CheckValidResponse " + errorMsg, HDLogger.LogCategory.Online);
                    return false;
                }
            }
            return true;

        }



        public void LogFailure()
        {
            if (debug) HDLogger.Log("API call have been failing for " + (Time.time - failingStartTime) + "s", HDLogger.LogCategory.Online);

        }

        public void StopAPI()
        {
            if (debug) HDLogger.Log("API Calls Failed after " + (Time.time - failingStartTime) + "s", HDLogger.LogCategory.Online);

            isFailed = true;
        }

        public static bool CanRetry()
        {
            return failingStartTime > Time.time - apiMaxFailureTime;
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        /// <summary>
        /// Add default header.
        /// </summary>
        /// <param name="key">Header field name.</param>
        /// <param name="value">Header field value.</param>
        /// <returns></returns>
        public void AddDefaultHeader(string key, string value)
        {
            _defaultHeaderMap.Add(key, value);
        }


        /// <summary>
        /// Serialize an object into JSON string.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>JSON string.</returns>
        public string Serialize(object obj)
        {
            try
            {
                return obj != null ? JsonConvert.SerializeObject(obj) : null;
            }
            catch (Exception e)
            {
                throw new Exception("Error 500:" + e.Message);
            }
        }


        /// <summary>
        /// Encode string in base64 format.
        /// </summary>
        /// <param name="text">string to be encoded.</param>
        /// <returns>Encoded string.</returns>
        public static string Base64Encode(string text)
        {
            var textByte = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textByte);
        }

        /// <summary>
        /// Dynamically cast the object into target type.
        /// </summary>
        /// <param name="fromObject">Object to be casted</param>
        /// <param name="toObject">Target type</param>
        /// <returns>Casted object</returns>
        public static System.Object ConvertType(System.Object fromObject, Type toObject)
        {
            return Convert.ChangeType(fromObject, toObject);
        }

    }
}
