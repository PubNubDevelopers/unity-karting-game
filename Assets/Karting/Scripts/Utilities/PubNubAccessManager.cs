using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class PubNubAccessManager
{
    private static readonly HttpClient client = new HttpClient();

    public PubNubAccessManager()
	{
	}

	public async Task<string> RequestToken(string UserId)
	{
        try
        {
            string TOKEN_SERVER = "https://devrel-demos-access-manager.netlify.app/.netlify/functions/api/unitykartracer";
            var values = new Dictionary<string, string>
            {
                {
                    "UUID", UserId
                }
            };
            var content = new StringContent(JsonConvert.SerializeObject(values), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(TOKEN_SERVER + "/grant", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseDynamic = JsonConvert.DeserializeObject<dynamic>(responseString);
                var body = responseDynamic.body;
                var token = responseDynamic.body.token;
                return Convert.ToString(token);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error setting Access manager token for DevRel demo");
            Debug.Log(e);
            return null;
        }
        return null;
    }
}

