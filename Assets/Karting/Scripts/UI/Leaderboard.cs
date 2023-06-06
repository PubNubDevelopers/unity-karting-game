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
using PubNubAPI;
using Newtonsoft.Json;
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
    private PubNub pubnub = PubNubConnection.pubnub;
    private Button submitTime;

    [DllImport("__Internal")]
    private static extern void CompleteAction(string str);

    private void Awake()
    {
        //Seting-up Game Objects.
        submitTime = GameObject.Find("TimeSubmitButton").GetComponent<Button>();
        submitTime.onClick.AddListener(SubmitTime);
        leaderboardContainer = transform.Find("LeaderboardContainer");
        GameObject.Find("PlayerTimeText").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Player.Score.ToString()}s";

        //Re-Initializing due to Fire() method not being fired unless PubNub object reinitialized.
        PNConfiguration pnConfiguration = new PNConfiguration();
        pnConfiguration.PublishKey = PubNubConnection.PublishKey;
        pnConfiguration.SubscribeKey = PubNubConnection.SubscribeKey;
        PubNubConnection.UserID = SystemInfo.deviceUniqueIdentifier;
        pnConfiguration.UserId = PubNubConnection.UserID;
        pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
        PubNubConnection.pubnub = new PubNub(pnConfiguration);
        pubnub = PubNubConnection.pubnub;
       
        //Leaderboard
        MyClass fireRefreshObject = new MyClass();
        fireRefreshObject.refresh = "new user refresh";
        string firerefreshobject = JsonUtility.ToJson(fireRefreshObject);

        // This will trigger the leaderboard to refresh so it will display for a new user. 
        pubnub.Fire() 
            .Channel("submit_score")
            .Message(firerefreshobject)
            .Async((result, status) => {
                if (status.Error)
                {
                    Debug.Log(status.Error);
                    Debug.Log(status.ErrorData.Info);
                }
                else
                {
                    Debug.Log(string.Format("Fire Timetoken: {0}", result.Timetoken));
                }
            });

        //Listens for any incoming PubNub messages. Used to update the Leaderboard.
        pubnub.SubscribeCallback += (sender, e) => {
            SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;
            if (mea.Status != null)
            {
                Debug.Log(string.Format(
                        " FetchMessages Error: {0} {1} {2}",
                        mea.Status.StatusCode, mea.Status.ErrorData, mea.Status.Category
                    ));
            }
            if (mea.MessageResult != null)
            {             
                //Creating arrays from a dictionary containing the payload in order to parse data easier.
                Dictionary<string, object> msg = mea.MessageResult.Payload as Dictionary<string, object>;
                string[] strArr = msg["username"] as string[];
                string[] strScores = msg["score"] as string[];

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

            if (mea.PresenceEventResult != null)
            {
                Debug.Log("In Example, SusbcribeCallback in presence" + mea.PresenceEventResult.Channel + mea.PresenceEventResult.Occupancy + mea.PresenceEventResult.Event);
            }
        };

        //Subscribes to the specified channel on the PubNub network to receive information.
        pubnub.Subscribe()
            .Channels(new List<string>() {
                "leaderboard_scores"
            })
            .WithPresence()
            .Execute();     
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
            .Async((result, status) => {
                if (!status.Error)
                {
                    Debug.Log(string.Format("Publish Timetoken: {0}", result.Timetoken));
                }
                else
                {
                    Debug.Log(status.Error);
                    Debug.Log(status.ErrorData.Info);
                }
            });
        submitTime.interactable = false; // Only allow submit time once.

        CompleteAction("Submit your time");
    }
}