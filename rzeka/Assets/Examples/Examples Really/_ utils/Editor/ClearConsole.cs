using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rzeka.Examples
{
    public static class ClearConsole
    {
        [MenuItem("Tools/Clear Console %#c")] // CMD + SHIFT + C
        static void Clear()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}
