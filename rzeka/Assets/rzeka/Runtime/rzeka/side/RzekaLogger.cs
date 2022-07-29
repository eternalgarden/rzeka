using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class RzekaLogger
{
    public static void Log(Exception ex)
    {
        Regex regex = new Regex(@"Assets.+\.cs:\d+");
        var matches = regex.Matches(ex.StackTrace);
        foreach (var item in matches)
        {
            Debug.Log(item.ToString());
        }
    }
}
