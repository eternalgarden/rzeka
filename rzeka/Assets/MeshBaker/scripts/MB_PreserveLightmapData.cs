using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigitalOpus.MB.Core;



/// <summary>
/// This script is used to fix a bug where lightmapping on combined meshes 
/// is not preserved after reloading a scene or entering gamemode. This problem 
/// was caused by the lightmap index and scale offset data being lost. This script 
/// sets the lightmap index and scale offset back to where it should be so that lightmapping 
/// is actually preserved. 
/// </summary>

[ExecuteInEditMode]
public class MB_PreserveLightmapData : MonoBehaviour
{
    public int lightmapIndex;
    public Vector4 lightmapScaleOffset;
    void Awake()
    {
        MeshRenderer curRend = GetComponent<MeshRenderer>();
        if (curRend == null)
        {
            Debug.LogError("The MB_PreserveLightmapData script must be on a GameObject with a MeshRenderer. There was no MeshRenderer on object: " + name);
            return;
        }
        if (curRend.lightmapIndex != -1)
        {
            lightmapIndex = curRend.lightmapIndex;
        }        
        if (curRend.lightmapIndex == -1)
        {
            curRend.lightmapIndex = lightmapIndex;
        }             
        lightmapScaleOffset = new Vector4(1, 1, 0, 0);
        curRend.lightmapScaleOffset = lightmapScaleOffset;
    }
}
