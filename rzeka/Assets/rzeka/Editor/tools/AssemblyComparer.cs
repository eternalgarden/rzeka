using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using UnityEditorInternal;
using UnityEngine;

public class AssemblyComparer : MonoBehaviour
{
    [SerializeField] AssemblyDefinitionAsset ada;
    [SerializeField] TypePublicMethods tpm;

    [Button]
    void Compare()
    {
        tpm = new();

        Debug.Log($"<color=yellow>{ada.GetType()}</color>");
        Debug.Log(ada.name);
        string assemblyName = ada.name;

        Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
            .SingleOrDefault(assembly => assembly.GetName().Name == assemblyName);

        tpm.AssemblyFullName = assembly.FullName;

        var publicTypes = assembly
            .GetTypes()
            .Where(t => t.IsPublic)
            .ToArray();

        tpm.PublicTypes = new PublicTypes[publicTypes.Length];

        for (int i = 0; i < publicTypes.Length; i++)
        {
            var type = publicTypes[i];
            var tpk = tpm.PublicTypes[i] = new();

            tpk.TypeName = type.Name;

            tpk.PublicMemberNames = type
                .GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)
                .Select(m =>
                {
                    string asd = m.ToString();
                    asd = Format(asd);
                    // StringBuilder msb = new();
                    // msb.Append(m.ReturnType.Name + " ");
                    // msb.Append(m.ToString());

                    return asd;
                })
                .ToArray();
        }

        StringBuilder sb = new();

        foreach (var item in publicTypes)
        {
            sb.Append(item.Name + "\n");
        }

        Debug.Log(sb.ToString());
    }

    private string Format(string asd)
    {
        /* ⭐ ---- ---- */

        asd = asd.Replace("System.", "");
        asd = asd.Replace("Collections.", "");
        asd = asd.Replace("Threading.", "");
        asd = asd.Replace("Collections.Generic.", "");
        // asd = asd.Replace("System.", "");
        
        Regex rx = new Regex(@"`\d\[((\w|,)+)\]");
        Match m = rx.Match(asd);
        if (m.Success)
        {
            asd = rx.Replace(asd, $"<{m.Groups[1]}>");
        }
        return asd;
        
        /* ---- ---- 🌠 */
    }

    [Serializable]
    private class TypePublicMethods
    {
        public string AssemblyFullName;
        public PublicTypes[] PublicTypes;
    }


    [Serializable]
    public class PublicTypes
    {
        public string TypeName;
        public string[] PublicMemberNames;
    }
}
