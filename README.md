PubNub Prix Unity Application
===================================

Welcome to PubNub Prix, PubNub's Unity racing game integrated with PubNub.

<img src="/Data/Screen Shot 2022-09-27 at 7.42.37 PM.png"/>

This is a racing game built using [PubNub's real-time network Unity API](https://www.pubnub.com/docs/sdks/unity). The base game is Unity's [Karting Microgame](https://learn.unity.com/project/karting-template), which is intended for Unity developers to learn Unity and enhance.

The objective of the game is to complete three checkpoints and reach the finish line. Players enter a username, play the game, and if they win the game, they can submit and view their time and username in a leaderboard that updates in real-time, as well as chat with others in a Lobby Chat.

## Features

* Functional Unity racing game, where users can drive their kart/character around a track, while avoiding obstacles and trying to obtain the fastest time.
* Player's can enter in a username used as their identity when submitting their score or chatting with others in the lobby chat.
* Leaderboard that updates in real-time.
* Lobby Chat where players can chat after finishing a race. A random profile picture is obtained through [MultiAvatar's API](https://multiavatar.com/) for the player based on that player's username.
* Simple profanity filter to filter profanity from the entered username and lobby chat. Replaces profanity with "*" characters.

## Demo & Tutorial
Try the [application!](https://www.pubnub.com/demos/unity-pubnubprix/)
[Walkthrough](https://www.pubnub.com/tutorials/unity-pubnubprix/) how the application was created.

## Installing / Getting started
If you would like to build, run, and expand upon this application yourself, please follow the sections below. There is also a detailed [tutorial](https://www.pubnub.com/tutorials/unity-pubnubprix/) on how this applciation was built located on PubNub's website.

### Requirements
- [Unity](https://store.unity.com/products/unity-personal)
- [Git](https://www.atlassian.com/git/tutorials/install-git)
- [Visual Studio](https://visualstudio.microsoft.com/vs/community/)
- [PubNub Account](#pubnub-account) (*Free*)

<a href="https://dashboard.pubnub.com/signup">
	<img alt="PubNub Signup" src="https://i.imgur.com/og5DDjf.png" width=260 height=97/>
</a>

### Get Your PubNub Keys
1. Sign in to your [PubNub Dashboard](https://admin.pubnub.com/). You are now in the Admin Portal.
2. Go to Apps using the left-hand side navigation.
3. Click the Create New App button in the top-right of the Portal.
4. Give your app a name.
5. Click Create.
6. Click your new app to open its settings.
7. When you create a new app, the first set of keys are generated automatically. However, a single app can have as many keysets as you like. PubNub recommends that you create separate keysets for production and test environments.
8. Click on the keyset.
9. Enable three features for the keyset to be able to connect to the PubNub Network: Presence, Stream Controller, and Message Persistence.
10. Enable Presence by clicking on the slider to turn it on. A pop-up will require that you enter in “ENABLE”. Enter in “ENABLE” in all caps and then press the “Enable” button. Enable the "Generate Leave on TCP FIN or RST checkbox", which generates leave events when clients close their connection (used to track occupancy and remove non-connected clients in app). You can leave the rest of the default settings.
11. Enable Stream Controller by clicking on the slider to turn it on if it is already not enabled. You can leave the default settings.
12. Click on save changes.
13. Save the Publish and Subscribe Keys.

### Set-up Leaderboard Function
The leaderboard entries (username and time) are stored, sorted, and sent back to users using PubNub's Functions feature. Set this up by:
1. Click on the Functions tab on the left hand side of the portal.
2. Select your App where you would like to enable the Module.
3. Click on Create New Module.
4. Give the module a name.
5. Enter a description of what the Module is doing.
6. Select the keyset you created earlier to add the Module. Click create.
7. Click the Create New Function button to create a Function, which allows you to transform, reroute, augment, filter and even aggregate data for subsequent use (think of them as functions/methods for classes).
8. Give the function a name.
9. Select the After Publish or Fire event type.
10. Enter the channel name that you had when you called the pubnub.Fire function in Leaderboard.cs in Step five. For this tutorial, it is submit_score.
11. Click the Create button.
12. You will be brought to the Function overview page, where you can change settings, test, and even monitor the Function when it is interacting with your game.
13. In the middle of the screen there should be automatically generated JavaScript code. This is a sample "Hello World" function to showcase an example of how a function would work. You can enter your own JavaScript code to have this Function process this data for your keyset.
14. Enter in the following Function code:

```
// This Function stores a random number in the KV store and retrieves it to augment the message
// For testing: you can use the following test payload: {"foo":"bar"}

// Below is the code with inline explanations

// Declare the Function with the export syntax. The incoming message is called request

export default (request) => {
    const db = require("kvstore");
    const pubnub = require("pubnub");
    var json = JSON.parse(request.message);
    console.log(json);
    let { username, score } = json;
    var scorearrayprevious = [];
    var scorearraynew = [];
    var usernamearraynew = [];
    var usernamearrayprevious = [];

    //db.removeItem("data"); //reset the block
    db.get("data").then((value) => {
        if(value){
            console.log("value", value);
            let i = 0;
            value.score.some(item => {
                console.log("hello", item, score);
                if(parseFloat(item) > parseFloat(score) || (parseFloat(item) == 0 && score.length > 0)){ //Parse into float since variables are currently strings
                    //Score
                    scorearraynew = value.score.slice(0, i);
                    scorearrayprevious = value.score.slice(i, value.score.length);
                    console.log("values", scorearraynew, scorearrayprevious);
                    scorearraynew.push(score);
                    var newScoreList = scorearraynew.concat(scorearrayprevious);
                    newScoreList.splice(-1,1);
                    
                    //Username
                    usernamearrayprevious = value.username.slice(0, i);
                    usernamearraynew = value.username.slice(i, value.score.length);
                    console.log("values", usernamearrayprevious, usernamearraynew);
                    usernamearrayprevious.push(username);
                    var newUsername = usernamearrayprevious.concat(usernamearraynew);
                    newUsername.splice(-1,1);
                    
                    value.score = newScoreList;
                    value.username = newUsername;

                    db.set("data", value);
                    
                    return true; //break out of the loop using Array.prototype.some by returning true
               }
                i++;
            });
            pubnub.publish({
                "channel": "leaderboard_scores",
                "message": value
            }).then((publishResponse) => {
                console.log("publish response", publishResponse);
            });
        } else {
          //Initial Data
            db.set("data", {
                "username":["---","---","---","---","---","---","---","---","---","---"], 
                "score":["0","0","0","0","0","0","0","0","0","0"]});
        }
    });
    return request.ok();
};
```
15. Press the Save button to save the Function,
16. Click on Restart Module to run the module.
17. If you want to wipe the scores from the KV store, un-comment out the db.removeItem("data") call, which resets the scores, save the module, and restart the module.

### Building and Running
1. Clone the GitHub repository.

	```bash
	git clone https://github.com/PubNubDevelopers/unity-karting-game.git
	```
2. Switch to the leaderboard-chat branch.
  ```bash
  git checkout leaderboard-chat
  ```
  
3. Open the project in Unity.
4. In Assets > Karting > Scripts > GameConstants, replace your publish and subscribe keys with the PUBNUB_PUBLIC_KEY and PUBNUB_SUBSCRIBE_KEY constants at the bottom of the file.
5. Run the game by File > Builds Settings > Build & Run.
6. Enter a username. Press the play button.
7. Play the game by controlling the kart with WASD or with the arrow keys.
8. Once you've won the game, submit your score by clicking on the submit time button.
9. View leaderboard updates in real-time.
10. Chat with others in the lobby chat.

## Links
- Demo Link: https://www.pubnub.com/demos/unity-pubnubprix/
- Tutorial Link: https://www.pubnub.com/tutorials/unity-pubnubprix/

## License
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```
