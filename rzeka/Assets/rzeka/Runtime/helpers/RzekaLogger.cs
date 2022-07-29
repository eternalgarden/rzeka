using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class RzekaLogger
{
    public static void Log(Exception ex)
    {
        // https://answers.unity.com/questions/41403/open-script-editor-ide-to-a-particular-linefunctio.html
        Regex regex = new Regex(@"Assets.+\.cs:\d+");
        var matches = regex.Matches(ex.StackTrace);
        StringBuilder sb = new StringBuilder();
        foreach (var item in matches)
        {
            sb.AppendLine($"{item.ToString()}");
        }
        Debug.LogError($"Message: {ex.Message}\nStack Trace: {sb.ToString()}");

    }
}
