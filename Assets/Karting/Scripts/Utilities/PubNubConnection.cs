/*
 * PubNubConnection.cs: Static class used to track the state of the PubNub connection across all scenes.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PubNubAPI;

public static class PubNubConnection
{
    //Persist the PubNub object across scenes
    private static string _userID;
    public static PubNub pubnub;

    public static string PublishKey
    {
        get { return GameConstants.PUBNUB_PUBLISH_KEY; }
    }

    public static string SubscribeKey
    {
        get { return GameConstants.PUBNUB_SUBSCRIBE_KEY; }
    }

    public static string UserID
    {
        get { return _userID; }
        set { _userID = value; }
    }
}