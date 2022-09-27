using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;

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
                text = text.Replace(profanity, new string('*', profanity.Length));
            }
        }
        return text;
    }

    //Reads the file contents and converts to a List via LINQ.
    public static void ReadFile(string file)
    {
        string[] wordArr = System.IO.File.ReadAllLines(file);
        _words = wordArr.OfType<string>().ToList();
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