/*
 * Chat.cs 
 * Lobby Chat where users can discuss post-game winning.
 * Controls logic driving the UI behind the lobby chat.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using PubNubAPI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

//Simple class used to pass information on PubNub Network.
public class JSONInformation
{
    public string username;
    public string text;
}

public class Chat : MonoBehaviour
{    
    ushort maxMessagesToDisplay = 10; //Initial Load-in of messages when first logging into network.
    private PubNub pubnub = PubNubConnection.pubnub;
    string channel = "chatchannel";

    //GameObjects
    Transform chatCanvas;
    Transform chatContainer;
    Transform chatScrollContent;
    TMPro.TMP_InputField chatInput;
    Texture2D profileTexture;

    Texture2D userTexture;

    //Avatars by Multiavatar: https://multiavatar.com/
    string profileGenerator = $"https://api.multiavatar.com/";

    private void Awake()
    {         
        //Setting Objects.
        chatCanvas = GameObject.Find("Chat").GetComponent<Canvas>().transform;
        chatInput = chatCanvas.Find("ChatInput").GetComponent<TMPro.TMP_InputField>();
        chatScrollContent = GetChild(chatCanvas, "ChatContent");
        chatContainer = GetChild(chatCanvas, "ChatEntryContainer");
        chatContainer.gameObject.SetActive(false); //Used as a template to create the other entries.
        Button chatBtn = chatCanvas.Find("SendChatButton").GetComponent<Button>();
        chatBtn.onClick.AddListener(SendChat);

        //Initialize PubNub Object
        PNConfiguration pnConfiguration = new PNConfiguration();
        pnConfiguration.PublishKey = PubNubConnection.PublishKey;
        pnConfiguration.SubscribeKey = PubNubConnection.SubscribeKey;
        PubNubConnection.UserID = SystemInfo.deviceUniqueIdentifier;
        pnConfiguration.UserId = PubNubConnection.UserID;
        pnConfiguration.LogVerbosity = PNLogVerbosity.BODY;
        PubNubConnection.pubnub = new PubNub(pnConfiguration);
        pubnub = PubNubConnection.pubnub;
     
        // Fetch the maxMessagesToDisplay messages sent on the given PubNub channel
        pubnub.FetchMessages()
            .Channels(new List<string> { channel })
            .Count(maxMessagesToDisplay)
            .Async((result, status) =>
            {
                if (status.Error)
                {
                    Debug.Log(string.Format(
                        " FetchMessages Error: {0} {1} {2}",
                        status.StatusCode, status.ErrorData, status.Category
                    ));
                }
                else
                {
                    foreach (KeyValuePair<string, List<PNMessageResult>> kvp in result.Channels)
                    {
                        foreach (PNMessageResult pnMessageResult in kvp.Value)
                        {
                            // Format data into readable format
                            JSONInformation chatmessage = JsonUtility.FromJson<JSONInformation>(pnMessageResult.Payload.ToString());

                            // Call the function to display the message in plain text
                            CreateChat(chatmessage);
                        }
                    }
                }
            });

        //Listen for any incoming messages on PubNub Network on channel. Receives any messages from other users and adds to lobby chat.
        pubnub.SubscribeCallback += (sender, e) => {
            SubscribeEventEventArgs mea = e as SubscribeEventEventArgs;
            if (mea.Status != null)
            {
                Debug.Log(string.Format(
                       " Subscribe Listening Error: {0} {1} {2}",
                       mea.Status.StatusCode, mea.Status.ErrorData, mea.Status.Category
                   ));
            }
            if (mea.MessageResult != null)
            {
                // Format data into a readable format
                JSONInformation chatmessage = JsonUtility.FromJson<JSONInformation>(mea.MessageResult.Payload.ToString());

                // Call the function to display the message in plain text
                CreateChat(chatmessage);
            }

            if (mea.PresenceEventResult != null)
            {
                Debug.Log("In Example, SubscribeCallback in presence" + mea.PresenceEventResult.Channel + mea.PresenceEventResult.Occupancy + mea.PresenceEventResult.Event);
            }
        };
        pubnub.Subscribe()
            .Channels(new List<string>() {
                channel
            })
            .WithPresence()
            .Execute();
    }

    // Create new chat objects based of the data received from PubNub
    void CreateChat(JSONInformation payLoad)
    {
        //Create new chat entry in the lobby chat.
        Transform duplciateContainer = Instantiate(chatContainer, chatScrollContent);
        RectTransform dupRectTransform = duplciateContainer.GetComponent<RectTransform>();
        RectTransform chatInputRect = chatInput.transform.GetComponent<RectTransform>();
        duplciateContainer.gameObject.SetActive(true);
        duplciateContainer.Find("EntryChat").GetComponent<TextMeshProUGUI>().text = $"{payLoad.username}: {payLoad.text}";
            
        string url = $"{profileGenerator}{payLoad.username}.png?apikey={GameConstants.AVATAR_API_KEY}";

        //Obtain and set the avatar profile picture for that entry.
        StartCoroutine(GetProfileImage(url, duplciateContainer));     
    }

    public void SendChat()
    {
        // When the user clicks the Submit button,
        // create a JSON object from input field input
        JSONInformation publishMessage = new JSONInformation();
        publishMessage.username = Player.Username;
        publishMessage.text = chatInput.text;
     
        string publishMessageToJSON = JsonUtility.ToJson(publishMessage);

        // Publish the JSON object to the assigned PubNub Channel
        pubnub.Publish()
            .Channel(channel)
            .Message(publishMessageToJSON)
            .Async((result, status) =>
            {
                if (status.Error)
                {
                    Debug.Log(status.Error);
                    Debug.Log(status.ErrorData.Info);
                }
                else
                {
                    Debug.Log(string.Format("Publish Timetoken: {0}", result.Timetoken));
                }
            });
        //Reset chat input.
        chatInput.text = "";
    }

    //Obtains the Specific object in the Hierarchy given the Parent Transform and Childname.
    public Transform GetChild(Transform parent, string childName)
    {
        Transform child = null;
        foreach(Transform ch in parent.transform.GetComponentsInChildren<Transform>())
        {
            if(ch.name.Equals(childName))
            {
                child = ch;
            
            }
        }

        return child;
    }

    //Obtains the texture from the given url.
    IEnumerator GetProfileImage(string url, Transform obj)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            obj.Find("EntryProfileImage").GetComponent<RawImage>().texture = DownloadHandlerTexture.GetContent(request);
        }
    }
}