/*
 * Player.cs: Used to track Player state across all scenes.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using PubNubAPI;
using UnityEngine;

public static class Player
{
    public static double Score { get; set; }
    public static string Username { get; set; }
}
