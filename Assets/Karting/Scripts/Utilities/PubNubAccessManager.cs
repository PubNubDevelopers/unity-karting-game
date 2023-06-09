using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class PubNubAccessManagerResponseToken
{
    public string Token { get; set; }
    public int TTL { get; set; }
}

public class PubNubAccessManagerResponse
{
    public PubNubAccessManagerResponseToken Body { get; set; }
}

public class PubNubAccessManager
{
    public PubNubAccessManager()
    {
    }

    public IEnumerator RequestToken(string UserId, Action<string> callback)
    {
        string TOKEN_SERVER = "https://devrel-demos-access-manager.netlify.app/.netlify/functions/api/unitykartracer";
        var values = new Dictionary<string, string>
            {
                {
                    "UUID", UserId
                }
            };
        var json = JsonConvert.SerializeObject(values);
        var bytes = Encoding.UTF8.GetBytes(json);
        using (var webRequest = new UnityWebRequest(TOKEN_SERVER + "/grant", "POST"))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(bytes);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("accept", "application/json");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                callback(null);
            }
            else
            {
                try
                {
                    PubNubAccessManagerResponse responseBody = JsonConvert.DeserializeObject<PubNubAccessManagerResponse>(webRequest.downloadHandler.text);
                    string token = responseBody.Body.Token;
                    callback(token);
                }
                catch (Exception)
                {
                    callback(null);
                }
            }
        }
    }
}

