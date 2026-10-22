/*
 * Leaderboard.cs: Controls the logic behind the Leaderboard in the Karting Game.
 * Subscribes to and listens for any incoming PubNub messages to update the leaderboard.
 * Controls logic behind the Leaderboard UI.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PubnubApi;
using PubnubApi.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Runtime.InteropServices;

//Helper Class Used to Store Information when passing in the PubNub Network.
public class MyClass
{
    public string username;
    public string score;
    public string refresh;
}

public class Leaderboard : MonoBehaviour
{ 
    private Transform leaderboardContainer;
    private Transform leaderboardEntry;
    private Pubnub pubnub = PubNubConnection.pubnub;
    private SubscribeCallbackListener listener = new SubscribeCallbackListener();
    private Button submitTime;

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void CompleteAction(string str);
#endif

    private void Awake()
    {
        //Seting-up Game Objects.
        submitTime = GameObject.Find("TimeSubmitButton").GetComponent<Button>();
        submitTime.onClick.AddListener(SubmitTime);
        leaderboardContainer = transform.Find("LeaderboardContainer");
        GameObject.Find("PlayerTimeText").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Player.Score.ToString()}s";

        //Re-Initializing due to Fire() method not being fired unless PubNub object reinitialized.
        PubNubConnection.UserID = SystemInfo.deviceUniqueIdentifier;
        PNConfiguration pnConfiguration = new PNConfiguration(new UserId(PubNubConnection.UserID));
        pnConfiguration.PublishKey = PubNubConnection.PublishKey;
        pnConfiguration.SubscribeKey = PubNubConnection.SubscribeKey;
        pnConfiguration.LogLevel = PubnubLogLevel.Debug;
        pnConfiguration.EnableHttp2 = false; //HTTP/2 transport requires Unity 6.5+.
        PubNubConnection.pubnub = PubnubUnityUtils.NewUnityPubnub(pnConfiguration, webGLBuildMode: Application.platform == RuntimePlatform.WebGLPlayer, unityLogging: true);
        pubnub = PubNubConnection.pubnub;

        StartCoroutine(new PubNubAccessManager().RequestToken(PubNubConnection.UserID, (token) =>
        {
            if (token != null)
            {
                pubnub.SetAuthToken(token);
            }
            AccessManagerComplete();
        }));

    }

    private void AccessManagerComplete()
    {
        //Leaderboard
        MyClass fireRefreshObject = new MyClass();
        fireRefreshObject.refresh = "new user refresh";
        string firerefreshobject = JsonUtility.ToJson(fireRefreshObject);

        // This will trigger the leaderboard to refresh so it will display for a new user. 
        pubnub.Fire() 
            .Channel("submit_score")
            .Message(firerefreshobject)
            .Execute((result, status) => {
                if (status.Error)
                {
                    Debug.Log(status.Error);
                    Debug.Log(status.ErrorData.Information);
                }
                else
                {
                    Debug.Log(string.Format("Fire Timetoken: {0}", result.Timetoken));
                }
            });

        //Listens for any incoming PubNub messages. Used to update the Leaderboard.
        listener.onStatus += (pn, status) => {
            Debug.Log(string.Format(
                    " FetchMessages Error: {0} {1} {2}",
                    status.StatusCode, status.ErrorData, status.Category
                ));
        };
        listener.onMessage += (pn, messageResult) => {
            {
                //Creating arrays from the JSON object containing the payload in order to parse data easier.
                JObject msg = messageResult.Message as JObject;
                if (msg == null)
                {
                    return;
                }
                string[] strArr = msg["username"].Select(item => (string)item).ToArray();
                string[] strScores = msg["score"].Select(item => (string)item).ToArray();

                //float templateHeight = 20f;
                for (int i = 0; i < strArr.Length; i++)
                {                
                    int rank = i + 1;

                    //Obtain the LeaderBoardEntry Row for that rank.
                    string entryText = "LeaderboardEntry" + rank;          
                    leaderboardEntry = leaderboardContainer.Find(entryText);

                    //Rank
                    leaderboardEntry.Find("EntryRankText").GetComponent<TMPro.TextMeshProUGUI>().text = rank.ToString();

                    //Name            
                    leaderboardEntry.Find("EntryUsernameText").GetComponent<TMPro.TextMeshProUGUI>().text = strArr[i];

                    //Time (in seconds)        
                    leaderboardEntry.Find("EntryTimeText").GetComponent<TMPro.TextMeshProUGUI>().text = strScores[i] + "s";
                }               
            }
        };
        listener.onPresence += (pn, presenceEventResult) => {
            Debug.Log("In Example, SusbcribeCallback in presence" + presenceEventResult.Channel + presenceEventResult.Occupancy + presenceEventResult.Event);
        };
        pubnub.AddListener(listener);

        //Subscribes to the specified channel on the PubNub network to receive information.
        pubnub.Subscribe<object>()
            .Channels(new string[] {
                "leaderboard_scores"
            })
            .WithPresence()
            .Execute();     
    }

    //PubNub instances now outlive the scene, so stop listening when the leaderboard is destroyed.
    private void OnDestroy()
    {
        if (pubnub != null)
        {
            pubnub.RemoveListener(listener);
            pubnub.UnsubscribeAll<object>();
        }
    }

    //Submits the time for the entered user in the PubNub Network.
    public void SubmitTime()
    {
        MyClass mc = new MyClass();
        mc.username = Player.Username;
        mc.score = Player.Score.ToString();
        string json = JsonUtility.ToJson(mc);

        pubnub.Publish()
            .Channel("submit_score")
            .Message(json)
            .Execute((result, status) => {
                if (!status.Error)
                {
                    Debug.Log(string.Format("Publish Timetoken: {0}", result.Timetoken));
                }
                else
                {
                    Debug.Log(status.Error);
                    Debug.Log(status.ErrorData.Information);
                }
            });
        submitTime.interactable = false; // Only allow submit time once.
#if UNITY_WEBGL
        CompleteAction("Submit your time");
#endif
    }
}