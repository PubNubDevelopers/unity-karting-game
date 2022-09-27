using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class ProfanityFilter
{
    private static List<string> _words;

    //Replaces the entered text with "*" if profanity is detected.
    public static string ReplaceProfanity(string text)
    {       
        foreach (string profanity in _words)
        {
            if (text.ToLower().Contains(profanity))
            {
                text = text.ToLower().Replace(profanity, new string('*', profanity.Length));
            }
        }
        
        return text;
    }
    
    //Obtains the texture from the given url.
    public static IEnumerator ReadFile()
    {     
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(GameConstants.PROFANITY_FILE_LOCATION);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            byte[] results = request.downloadHandler.data;
            string str = System.Text.Encoding.Default.GetString(results);
            //Regex regex = new Regex(str.Replace('\n', '|'));
            string[] wordArr = str.Split('\n');
            _words = wordArr.OfType<string>().ToList();
            
        }
    }
    
    /*
    //Determines if the entered word is considered profanity.
    public static bool IsProfanity(string word)
    {     
        //Determines if the word is in the list of words via LINQ.
        bool b = _words.Any(w=>word.ToLower().Contains(w));

        return b;
    }
    */
}