using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {

        // Should only take in and manipulate data, and then return said data
        // All assignment to the combiner and such should be done inside MeshCombinerSimple
        public class MB3_MeshCombinerSimpleSubCombinerLegacy
        {
            public static bool doUV2(ref MB_IMeshBakerSettings settings)
            {
                bool result = settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged || settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping || settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects;
                return result;
            }

            public static bool InstanceID2DGO(ref List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, int instanceID, out MB_DynamicGameObject dgoGameObject)
            {
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    if (mbDynamicObjectsInCombinedMesh[i].instanceID == instanceID)
                    {
                        dgoGameObject = mbDynamicObjectsInCombinedMesh[i];
                        return true;
                    }
                }

                dgoGameObject = null;
                return false;
            }

            public static void instance2Combined_MapAdd(ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map, GameObject gameObjectID, MB_DynamicGameObject dgo)
            {
                _instance2combined_map.Add(gameObjectID, dgo);
            }

            public static void instance2Combined_MapRemove(ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map, GameObject gameObjectID)
            {
                _instance2combined_map.Remove(gameObjectID);
            }

            public static void _copyUV2unchangedToSeparateRects(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref Vector2[] uv2s)
            {
                //combiner._copyUV2unchangedToSeparateRects();
                int uv2Padding = 16; //todo
                                     //todo meshSize
                List<Vector2> uv2AtlasSizes = new List<Vector2>();
                float minSize = 10e10f;
                float maxSize = 0f;
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    float zz = mbDynamicObjectsInCombinedMesh[i].meshSize.magnitude;
                    if (zz > maxSize) maxSize = zz;
                    if (zz < minSize) minSize = zz;
                }

                //normalize size so all values lie between these two values
                float MAX_UV_VAL = 1000f;
                float MIN_UV_VAL = 10f;
                float offset = 0;
                float scale = 1;
                if (maxSize - minSize > MAX_UV_VAL - MIN_UV_VAL)
                {
                    //need to compress the range. Scale until is MAX_UV_VAL - MIN_UV_VAL in size and shift
                    scale = (MAX_UV_VAL - MIN_UV_VAL) / (maxSize - minSize);
                    offset = MIN_UV_VAL - minSize * scale;
                }
                else
                {
                    scale = MAX_UV_VAL / maxSize;
                }
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {

                    float zz = mbDynamicObjectsInCombinedMesh[i].meshSize.magnitude;
                    zz = zz * scale + offset;
                    Vector2 sz = Vector2.one * zz;
                    uv2AtlasSizes.Add(sz);
                }

                //run texture packer on these rects
                MB2_TexturePacker tp = new MB2_TexturePackerRegular();
                tp.atlasMustBePowerOfTwo = false;
                AtlasPackingResult[] uv2Rects = tp.GetRects(uv2AtlasSizes, MB2_TexturePacker.MAX_ATLAS_SIZE, MB2_TexturePacker.MAX_ATLAS_SIZE, uv2Padding);
                //Debug.Assert(uv2Rects.Length == 1);
                //adjust UV2s
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    float minx, maxx, miny, maxy;
                    minx = maxx = uv2s[dgo.vertIdx].x;
                    miny = maxy = uv2s[dgo.vertIdx].y;
                    int endIdx = dgo.vertIdx + dgo.numVerts;
                    for (int j = dgo.vertIdx; j < endIdx; j++)
                    {
                        if (uv2s[j].x < minx) minx = uv2s[j].x;
                        if (uv2s[j].x > maxx) maxx = uv2s[j].x;
                        if (uv2s[j].y < miny) miny = uv2s[j].y;
                        if (uv2s[j].y > maxy) maxy = uv2s[j].y;
                    }
                    //  scale it to fit the rect
                    Rect r = uv2Rects[0].rects[i];
                    for (int j = dgo.vertIdx; j < endIdx; j++)
                    {
                        float width = maxx - minx;
                        float height = maxy - miny;
                        if (width == 0f) width = 1f;
                        if (height == 0f) height = 1f;
                        uv2s[j].x = ((uv2s[j].x - minx) / width) * r.width + r.x;
                        uv2s[j].y = ((uv2s[j].y - miny) / height) * r.height + r.y;
                    }
                }
            }

            public static void _copyAndAdjustUV2FromMesh(MB_IMeshBakerSettings settings, ref Vector2[] uv2s, MB_DynamicGameObject dgo, Mesh mesh, int vertsIdx, MeshChannelsCache meshChannelsCache, MB2_LogLevel LOG_LEVEL)
            {
                //combiner._copyAndAdjustUV2FromMesh(dgo, mesh, vertsIdx, meshChannelsCache);
                Vector2[] nuv2s = meshChannelsCache.GetUVChannel(2, mesh);

                if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
                { //has a lightmap
                  // the lightmapTilingOffset is always 1,1,0,0 for all objects
                  //lightMap index is always 1
                    if (nuv2s == null || nuv2s.Length == 0)
                    {
                        Vector2[] nuv = meshChannelsCache.GetUVChannel(0, mesh);
                        if (nuv != null && nuv.Length > 0)
                        {
                            nuv2s = nuv;
                        }
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogError("Mesh " + mesh + " has no uv2 or uvs. Generating garbage UVs. Every UV = .5, .5");
#endif
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + mesh + " didn't have uv2s. Generating uv2s.");
                            nuv2s = meshChannelsCache.GetUv2Modified(mesh);
                        }
                    }
                    Vector2 uvscale2;
                    Vector4 lightmapTilingOffset = dgo.lightmapTilingOffset;
                    Vector2 uvscale = new Vector2(lightmapTilingOffset.x, lightmapTilingOffset.y);
                    Vector2 uvoffset = new Vector2(lightmapTilingOffset.z, lightmapTilingOffset.w);
                    for (int j = 0; j < nuv2s.Length; j++)
                    {
                        uvscale2.x = uvscale.x * nuv2s[j].x;
                        uvscale2.y = uvscale.y * nuv2s[j].y;
                        uv2s[vertsIdx + j] = uvoffset + uvscale2;
                    }
                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("_copyAndAdjustUV2FromMesh copied and modify for preserve current lightmapping " + nuv2s.Length);
                }
                else
                {
                    if (nuv2s == null || nuv2s.Length == 0)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Mesh " + mesh + " has no uv2s. Generating garbage uv2s. Every UV = .5, .5");
#endif
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + mesh + " didn't have uv2s. Generating uv2s.");
                        if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects && (nuv2s == null || nuv2s.Length == 0))
                        {
                            Debug.LogError("Mesh " + mesh + " did not have a UV2 channel. Nothing to copy when trying to copy UV2 to separate rects. " +
                              "The combined mesh will not lightmap properly. Try using generate new uv2 layout.");
                        }
                        nuv2s = meshChannelsCache.GetUv2Modified(mesh);
                    }
                    nuv2s.CopyTo(uv2s, vertsIdx);
                    if (LOG_LEVEL >= MB2_LogLevel.trace)
                    {
                        Debug.Log("_copyAndAdjustUV2FromMesh copied without modifying " + nuv2s.Length);
                    }
                }
            }

            // Main for testing purposes (takes in a combiner object to grab real data)
            public static bool _CreateQuadMesh(
                ref MB3_MeshCombinerSingle combiner,
                ref int totalAddVerts,
                ref int totalDeleteVerts,
                ref int totalAddBlendShapes,
                ref int totalDeleteBlendShapes,
                ref int numResultMats,
                ref int[] totalAddSubmeshTris,
                ref int[] totalDeleteSubmeshTris,
                ref int[] _goToDelete,
                ref MB3_MeshCombinerSimpleBones boneProcessor,
                ref List<MB_DynamicGameObject> toAddDGOs,
                ref GameObject[] _goToAdd,
                ref MeshChannelsCache meshChannelCache,
                ref UVAdjuster_Atlas uvAdjuster,
                ref System.Diagnostics.Stopwatch sw
                )
            {
                // Grab all the stuff we'll end up needing from the combiner
                Vector3[] verts = combiner.verts;
                Vector3[] normals = combiner.normals;
                Vector4[] tangents = combiner.tangents;
                Vector2[] uvs = combiner.uvs;
                Vector2[] uv2s = combiner.uv2s;
                Vector2[] uv3s = combiner.uv3s;
                Vector2[] uv4s = combiner.uv4s;
                Vector2[] uv5s = combiner.uv5s;
                Vector2[] uv6s = combiner.uv6s;
                Vector2[] uv7s = combiner.uv7s;
                Vector2[] uv8s = combiner.uv8s;
                float[] uvsSliceIdx = combiner.uvsSliceIdx;
                Color[] colors = combiner.colors;
                MBBlendShape[] blendShapes = combiner.blendShapes;
                BoneWeight[] boneWeights = combiner.boneWeights;
                Matrix4x4[] bindPoses = combiner.bindPoses;
                Transform[] bones = combiner.bones;
                SerializableIntArray[] submeshTris = combiner.submeshTris;
                MB2_TextureBakeResults textureBakeResults = combiner.textureBakeResults;
                MB_IMeshBakerSettings settings = combiner.settings;
                MB2_LogLevel LOG_LEVEL = combiner.LOG_LEVEL;
                List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = combiner.mbDynamicObjectsInCombinedMesh;
                List<GameObject> objectsInCombinedMesh = combiner.objectsInCombinedMesh;
                Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map = combiner._instance2combined_map;

                bool success = CreateQuadMesh(ref totalAddVerts, ref totalDeleteVerts, ref totalAddBlendShapes, ref totalDeleteBlendShapes, ref numResultMats, ref totalAddSubmeshTris, ref totalDeleteSubmeshTris, ref _goToDelete, ref boneProcessor, ref toAddDGOs, ref _goToAdd, ref meshChannelCache, ref uvAdjuster, ref sw, ref textureBakeResults,
                    ref settings, ref LOG_LEVEL, ref mbDynamicObjectsInCombinedMesh, ref objectsInCombinedMesh, ref _instance2combined_map,
                    ref verts,
                    ref normals,
                    ref tangents,
                    ref uvs, ref uv2s, ref uv3s, ref uv4s, ref uv5s, ref uv6s, ref uv7s, ref uv8s,
                    ref uvsSliceIdx,
                    ref colors,
                    ref blendShapes,
                    ref boneWeights,
                    ref bindPoses,
                    ref bones,
                    ref submeshTris
                );

                if (!success) return false;

                // Now add to combiner
                combiner.verts = verts;
                combiner.normals = normals;
                combiner.tangents = tangents;
                combiner.uvs = uvs;
                combiner.uv2s = uv2s;
                combiner.uv3s = uv3s;
                combiner.uv4s = uv4s;
                combiner.uv5s = uv5s;
                combiner.uv6s = uv6s;
                combiner.uv7s = uv7s;
                combiner.uv8s = uv8s;
                combiner.uvsSliceIdx = uvsSliceIdx;
                combiner.colors = colors;
                combiner.blendShapes = blendShapes;
                combiner.boneWeights = boneWeights;
                combiner.bindPoses = bindPoses;
                combiner.bones = bones;
                combiner.submeshTris = submeshTris;

                return true;
            }

            // Actual main (suited for using dummy data)
            // We want to pass a bunch of arrays into this, have it do its work, and have the input arrays be holding the result of the work after
            // Thats why we pass them by 'ref'
            public static bool CreateQuadMesh(
                ref int totalAddVerts,
                ref int totalDeleteVerts,
                ref int totalAddBlendShapes,
                ref int totalDeleteBlendShapes,
                ref int numResultMats,
                ref int[] totalAddSubmeshTris,
                ref int[] totalDeleteSubmeshTris,
                ref int[] _goToDelete,
                ref MB3_MeshCombinerSimpleBones boneProcessor,
                ref List<MB_DynamicGameObject> toAddDGOs,
                ref GameObject[] _goToAdd,
                ref MeshChannelsCache meshChannelCache,
                ref UVAdjuster_Atlas uvAdjuster,
                ref System.Diagnostics.Stopwatch sw,
                ref MB2_TextureBakeResults textureBakeResults,
                ref MB_IMeshBakerSettings settings,
                ref MB2_LogLevel LOG_LEVEL,
                ref List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh,
                ref List<GameObject> objectsInCombinedMesh,
                ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map,
                ref Vector3[] verts,
                ref Vector3[] normals,
                ref Vector4[] tangents,
                ref Vector2[] uvs,
                ref Vector2[] uv2s,
                ref Vector2[] uv3s,
                ref Vector2[] uv4s,
                ref Vector2[] uv5s,
                ref Vector2[] uv6s,
                ref Vector2[] uv7s,
                ref Vector2[] uv8s,
                ref float[] uvsSliceIdx,
                ref Color[] colors,
                ref MBBlendShape[] blendShapes,
                ref BoneWeight[] boneWeights,
                ref Matrix4x4[] bindPoses,
                ref Transform[] bones,
                ref SerializableIntArray[] submeshTris
                )
            {
                // Buffers
                Vector3[] nverts;
                Vector3[] nnormals;
                Vector4[] ntangents;
                Vector2[] nuvs, nuv2s, nuv3s, nuv4s, nuv5s, nuv6s, nuv7s, nuv8s;
                float[] nuvsSliceIdx;
                Color[] ncolors;
                MBBlendShape[] nblendShapes;
                BoneWeight[] nboneWeights;
                Matrix4x4[] nbindPoses;
                Transform[] nbones;
                SerializableIntArray[] nsubmeshTris;

                // Initialize our buffers to the proper size
                bool setBuffersSuccess = SetBuffers(
                    ref verts,
                    ref blendShapes,
                    ref submeshTris,
                    ref textureBakeResults,
                    ref settings,
                    ref LOG_LEVEL,
                    ref totalAddVerts,
                    ref totalDeleteVerts,
                    ref totalAddBlendShapes,
                    ref totalDeleteBlendShapes,
                    ref numResultMats,
                    ref totalAddSubmeshTris,
                    ref totalDeleteSubmeshTris,
                    ref _goToDelete,
                    ref boneProcessor,
                    ref mbDynamicObjectsInCombinedMesh,
                    out nverts,
                    out nnormals,
                    out ntangents,
                    out nuvs, out nuv2s, out nuv3s, out nuv4s, out nuv5s, out nuv6s, out nuv7s, out nuv8s,
                    out nuvsSliceIdx,
                    out ncolors,
                    out nblendShapes,
                    out nboneWeights,
                    out nbindPoses,
                    out nbones,
                    out nsubmeshTris
                );

                if (!setBuffersSuccess) return false;

                // Now our buffers are created so fill them with data
                bool copyDataSuccess = CopyDataToBuffers(
                    verts,
                    normals,
                    tangents,
                    uvs, uv2s, uv3s, uv4s, uv5s, uv6s, uv7s, uv8s,
                    uvsSliceIdx,
                    colors,
                    blendShapes,
                    boneWeights,
                    bindPoses,
                    bones,
                    submeshTris,
                    ref textureBakeResults,
                    ref mbDynamicObjectsInCombinedMesh,
                    ref objectsInCombinedMesh,
                    ref settings,
                    ref LOG_LEVEL,
                    ref totalDeleteVerts,
                    ref numResultMats,
                    ref boneProcessor,
                    ref toAddDGOs,
                    ref _goToAdd,
                    ref meshChannelCache,
                    ref uvAdjuster,
                    ref sw,
                    ref _instance2combined_map,
                    ref nverts,
                    ref nnormals,
                    ref ntangents,
                    ref nuvs, ref nuv2s, ref nuv3s, ref nuv4s, ref nuv5s, ref nuv6s, ref nuv7s, ref nuv8s,
                    ref nuvsSliceIdx,
                    ref ncolors,
                    ref nblendShapes,
                    ref nboneWeights,
                    ref nbindPoses,
                    ref nbones,
                    ref nsubmeshTris
                );

                if (!copyDataSuccess) return false;

                // Now make sure all the updated data is assigned back to the input arrays
                verts = nverts;
                normals = nnormals;
                tangents = ntangents;
                uvs = nuvs;
                uv2s = nuv2s;
                uv3s = nuv3s;
                uv4s = nuv4s;
                uv5s = nuv5s;
                uv6s = nuv6s;
                uv7s = nuv7s;
                uv8s = nuv8s;
                uvsSliceIdx = nuvsSliceIdx;
                colors = ncolors;
                blendShapes = nblendShapes;
                boneWeights = nboneWeights;
                bindPoses = nbindPoses;
                bones = nbones;
                submeshTris = nsubmeshTris;

                return true;
            }
            
            // Creates empty buffers with the correct size
            public static bool SetBuffers(
                ref Vector3[] verts,
                ref MBBlendShape[] blendShapes,
                ref SerializableIntArray[] submeshTris,
                ref MB2_TextureBakeResults textureBakeResults,
                ref MB_IMeshBakerSettings settings,
                ref MB2_LogLevel LOG_LEVEL,
                ref int totalAddVerts,
                ref int totalDeleteVerts,
                ref int totalAddBlendShapes,
                ref int totalDeleteBlendShapes,
                ref int numResultMats,
                ref int[] totalAddSubmeshTris,
                ref int[] totalDeleteSubmeshTris,
                ref int[] _goToDelete,
                ref MB3_MeshCombinerSimpleBones boneProcessor,
                ref List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh,
                out Vector3[] nverts,
                out Vector3[] nnormals,
                out Vector4[] ntangents,
                out Vector2[] nuvs, out Vector2[] nuv2s, out Vector2[] nuv3s, out Vector2[] nuv4s, out Vector2[] nuv5s, out Vector2[] nuv6s, out Vector2[] nuv7s, out Vector2[] nuv8s,
                out float[] nuvsSliceIdx,
                out Color[] ncolors,
                out MBBlendShape[] nblendShapes,
                out BoneWeight[] nboneWeights,
                out Matrix4x4[] nbindPoses,
                out Transform[] nbones,
                out SerializableIntArray[] nsubmeshTris
                )
            {
                int newVertSize = verts.Length + totalAddVerts - totalDeleteVerts;
                int newBonesSize = boneProcessor.GetNewBonesLength();
                int[] newSubmeshTrisSize = new int[numResultMats];
                int newBlendShapeSize = 0;
                if (settings.doBlendShapes) newBlendShapeSize = blendShapes.Length + totalAddBlendShapes - totalDeleteBlendShapes;
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Verts adding:" + totalAddVerts + " deleting:" + totalDeleteVerts + " submeshes:" + newSubmeshTrisSize.Length + " bones:" + newBonesSize + " blendShapes:" + newBlendShapeSize);

                for (int i = 0; i < newSubmeshTrisSize.Length; i++)
                {
                    newSubmeshTrisSize[i] = submeshTris[i].data.Length + totalAddSubmeshTris[i] - totalDeleteSubmeshTris[i];
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("    submesh :" + i + " already contains:" + submeshTris[i].data.Length + " tris to be Added:" + totalAddSubmeshTris[i] + " tris to be Deleted:" + totalDeleteSubmeshTris[i]);
                }

                if (newVertSize >= MBVersion.MaxMeshVertexCount())
                {
                    Debug.LogError("Cannot add objects. Resulting mesh will have more than " + MBVersion.MaxMeshVertexCount() + " vertices. Try using a Multi-MeshBaker component. This will split the combined mesh into several meshes. You don't have to re-configure the MB2_TextureBaker. Just remove the MB2_MeshBaker component and add a MB2_MultiMeshBaker component.");
                    nverts = null;
                    nnormals = null;
                    ntangents = null;
                    nuvs = null;
                    nuv2s = null;
                    nuv3s = null;
                    nuv4s = null;
                    nuv5s = null;
                    nuv6s = null;
                    nuv7s = null;
                    nuv8s = null;
                    nuvsSliceIdx = null;
                    ncolors = null;
                    nblendShapes = null;
                    nboneWeights = null;
                    nbindPoses = null;
                    nbones = null;
                    nsubmeshTris = null;
                    return false;
                }

                nverts = new Vector3[newVertSize];
                nnormals = settings.doNorm ? new Vector3[newVertSize] : null;
                ntangents = settings.doTan ? new Vector4[newVertSize] : null;
                nuvs = settings.doUV ? new Vector2[newVertSize] : null;
                nuvsSliceIdx = settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray ? new float[newVertSize] : null;
                nuv3s = settings.doUV3 ? new Vector2[newVertSize] : null;
                nuv4s = settings.doUV4 ? new Vector2[newVertSize] : null;

                nuv5s = settings.doUV5 ? new Vector2[newVertSize] : null;
                nuv6s = settings.doUV6 ? new Vector2[newVertSize] : null;
                nuv7s = settings.doUV7 ? new Vector2[newVertSize] : null;
                nuv8s = settings.doUV8 ? new Vector2[newVertSize] : null;

                nuv2s = doUV2(ref settings) ? new Vector2[newVertSize] : null;
                ncolors = settings.doCol ? new Color[newVertSize] : null;
                nblendShapes = settings.doBlendShapes ? new MBBlendShape[newBlendShapeSize] : null;

                nboneWeights = new BoneWeight[newVertSize];
                nbindPoses = new Matrix4x4[newBonesSize];
                nbones = new Transform[newBonesSize];
                nsubmeshTris = new SerializableIntArray[numResultMats];

                for (int i = 0; i < nsubmeshTris.Length; i++)
                {
                    nsubmeshTris[i] = new SerializableIntArray(newSubmeshTrisSize[i]);
                }

                for (int i = 0; i < _goToDelete.Length; i++)
                {
                    MB_DynamicGameObject dgo = null;
                    InstanceID2DGO(ref mbDynamicObjectsInCombinedMesh, _goToDelete[i], out dgo);
                    if (dgo != null)
                    {
                        dgo._beingDeleted = true;
                    }
                }

                return true;
            }

            // Does some work on the data and returns the updated data inside the buffers
            // Data arrays (verts[] normals[] etc.) are not passed as ref here because we want all modifications to remain local to CopyDataToBuffers, modification to the actual data arrays should all be done in MeshCombinerSimple
            public static bool CopyDataToBuffers(
                Vector3[] verts,
                Vector3[] normals,
                Vector4[] tangents,
                Vector2[] uvs,
                Vector2[] uv2s,
                Vector2[] uv3s,
                Vector2[] uv4s,
                Vector2[] uv5s,
                Vector2[] uv6s,
                Vector2[] uv7s,
                Vector2[] uv8s,
                float[] uvsSliceIdx,
                Color[] colors,
                MBBlendShape[] blendShapes,
                BoneWeight[] boneWeights,
                Matrix4x4[] bindPoses,
                Transform[] bones,
                SerializableIntArray[] submeshTris,
                ref MB2_TextureBakeResults textureBakeResults,
                ref List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh,
                ref List<GameObject> objectsInCombinedMesh,
                ref MB_IMeshBakerSettings settings,
                ref MB2_LogLevel LOG_LEVEL,
                ref int totalDeleteVerts,
                ref int numResultMats,
                ref MB3_MeshCombinerSimpleBones boneProcessor,
                ref List<MB_DynamicGameObject> toAddDGOs,
                ref GameObject[] _goToAdd,
                ref MeshChannelsCache meshChannelCache,
                ref UVAdjuster_Atlas uvAdjuster,
                ref System.Diagnostics.Stopwatch sw,
                ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map,
                ref Vector3[] nverts,
                ref Vector3[] nnormals,
                ref Vector4[] ntangents,
                ref Vector2[] nuvs, ref Vector2[] nuv2s, ref Vector2[] nuv3s, ref Vector2[] nuv4s, ref Vector2[] nuv5s, ref Vector2[] nuv6s, ref Vector2[] nuv7s, ref Vector2[] nuv8s,
                ref float[] nuvsSliceIdx,
                ref Color[] ncolors,
                ref MBBlendShape[] nblendShapes,
                ref BoneWeight[] nboneWeights,
                ref Matrix4x4[] nbindPoses,
                ref Transform[] nbones,
                ref SerializableIntArray[] nsubmeshTris
                )
            {
                mbDynamicObjectsInCombinedMesh.Sort();

                //copy existing arrays to narrays gameobj by gameobj omitting deleted ones
                int targVidx = 0;
                int targBlendShapeIdx = 0;
                int[] targSubmeshTidx = new int[numResultMats];
                int triangleIdxAdjustment = 0;
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    if (!dgo._beingDeleted)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Copying obj in combined arrays idx:" + i, LOG_LEVEL);
                        Array.Copy(verts, dgo.vertIdx, nverts, targVidx, dgo.numVerts);
                        if (settings.doNorm) { Array.Copy(normals, dgo.vertIdx, nnormals, targVidx, dgo.numVerts); }
                        if (settings.doTan) { Array.Copy(tangents, dgo.vertIdx, ntangents, targVidx, dgo.numVerts); }
                        if (settings.doUV) { Array.Copy(uvs, dgo.vertIdx, nuvs, targVidx, dgo.numVerts); }
                        if (settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray) { Array.Copy(uvsSliceIdx, dgo.vertIdx, nuvsSliceIdx, targVidx, dgo.numVerts); }
                        if (settings.doUV3) { Array.Copy(uv3s, dgo.vertIdx, nuv3s, targVidx, dgo.numVerts); }
                        if (settings.doUV4) { Array.Copy(uv4s, dgo.vertIdx, nuv4s, targVidx, dgo.numVerts); }

                        if (settings.doUV5) { Array.Copy(uv5s, dgo.vertIdx, nuv5s, targVidx, dgo.numVerts); }
                        if (settings.doUV6) { Array.Copy(uv6s, dgo.vertIdx, nuv6s, targVidx, dgo.numVerts); }
                        if (settings.doUV7) { Array.Copy(uv7s, dgo.vertIdx, nuv7s, targVidx, dgo.numVerts); }
                        if (settings.doUV8) { Array.Copy(uv8s, dgo.vertIdx, nuv8s, targVidx, dgo.numVerts); }

                        if (doUV2(ref settings)) { Array.Copy(uv2s, dgo.vertIdx, nuv2s, targVidx, dgo.numVerts); }
                        if (settings.doCol) { Array.Copy(colors, dgo.vertIdx, ncolors, targVidx, dgo.numVerts); }
                        if (settings.doBlendShapes) { Array.Copy(blendShapes, dgo.blendShapeIdx, nblendShapes, targBlendShapeIdx, dgo.numBlendShapes); }
                        if (settings.renderType == MB_RenderType.skinnedMeshRenderer) { Array.Copy(boneWeights, dgo.vertIdx, nboneWeights, targVidx, dgo.numVerts); }

                        //adjust triangles, then copy them over
                        for (int subIdx = 0; subIdx < numResultMats; subIdx++)
                        {
                            int[] sTris = submeshTris[subIdx].data;
                            int sTriIdx = dgo.submeshTriIdxs[subIdx];
                            int sNumTris = dgo.submeshNumTris[subIdx];
                            if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("    Adjusting submesh triangles submesh:" + subIdx + " startIdx:" + sTriIdx + " num:" + sNumTris + " nsubmeshTris:" + nsubmeshTris.Length + " targSubmeshTidx:" + targSubmeshTidx.Length, LOG_LEVEL);
                            for (int j = sTriIdx; j < sTriIdx + sNumTris; j++)
                            {
                                sTris[j] = sTris[j] - triangleIdxAdjustment;
                            }
                            Array.Copy(sTris, sTriIdx, nsubmeshTris[subIdx].data, targSubmeshTidx[subIdx], sNumTris);
                        }

                        dgo.vertIdx = targVidx;
                        dgo.blendShapeIdx = targBlendShapeIdx;

                        for (int j = 0; j < targSubmeshTidx.Length; j++)
                        {
                            dgo.submeshTriIdxs[j] = targSubmeshTidx[j];
                            targSubmeshTidx[j] += dgo.submeshNumTris[j];
                        }
                        targBlendShapeIdx += dgo.numBlendShapes;
                        targVidx += dgo.numVerts;
                    }
                    else
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Not copying obj: " + i, LOG_LEVEL);
                        triangleIdxAdjustment += dgo.numVerts;
                    }
                }

                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    boneProcessor.CopyBonesWeAreKeepingToNewBonesArrayAndAdjustBWIndexes(nbones, nbindPoses, nboneWeights, totalDeleteVerts);
                }

                //remove objects we are deleting
                for (int i = mbDynamicObjectsInCombinedMesh.Count - 1; i >= 0; i--)
                {
                    if (mbDynamicObjectsInCombinedMesh[i]._beingDeleted)
                    {
                        instance2Combined_MapRemove(ref _instance2combined_map, mbDynamicObjectsInCombinedMesh[i].gameObject);
                        objectsInCombinedMesh.RemoveAt(i);
                        mbDynamicObjectsInCombinedMesh.RemoveAt(i);
                    }
                }

                verts = nverts;
                if (settings.doNorm) normals = nnormals;
                if (settings.doTan) tangents = ntangents;
                if (settings.doUV) uvs = nuvs;
                if (settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray) uvsSliceIdx = nuvsSliceIdx;
                if (settings.doUV3) uv3s = nuv3s;
                if (settings.doUV4) uv4s = nuv4s;

                if (settings.doUV5) uv5s = nuv5s;
                if (settings.doUV6) uv6s = nuv6s;
                if (settings.doUV7) uv7s = nuv7s;
                if (settings.doUV8) uv8s = nuv8s;

                if (doUV2(ref settings)) uv2s = nuv2s;
                if (settings.doCol) colors = ncolors;
                if (settings.doBlendShapes) blendShapes = nblendShapes;
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer) boneWeights = nboneWeights;
                int newBonesStartAtIdx = bones.Length - boneProcessor.GetNumBonesToDelete();
                bindPoses = nbindPoses;
                bones = nbones;
                submeshTris = nsubmeshTris;

                //insert the new bones into the bones array
                int bidx = 0;
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    foreach (BoneAndBindpose t in boneProcessor.GetBonesToAdd())
                    {
                        nbones[newBonesStartAtIdx + bidx] = t.bone;
                        nbindPoses[newBonesStartAtIdx + bidx] = t.bindPose;
                        bidx++;
                    }
                }

                //add new
                for (int i = 0; i < toAddDGOs.Count; i++)
                {
                    MB_DynamicGameObject dgo = toAddDGOs[i];
                    GameObject go = _goToAdd[i];
                    int vertsIdx = targVidx;
                    int blendShapeIdx = targBlendShapeIdx;
                    //				Profile.StartProfile("TestNewNorm");
                    Mesh mesh = MB_Utility.GetMesh(go);
                    Matrix4x4 l2wMat = go.transform.localToWorldMatrix;

                    // Similar to local2world but with translation removed and we are using the inverse transpose.
                    // We use this for normals and tangents because it handles scaling correctly.
                    Matrix4x4 l2wRotScale = l2wMat;
                    l2wRotScale[0, 3] = l2wRotScale[1, 3] = l2wRotScale[2, 3] = 0f;
                    l2wRotScale = l2wRotScale.inverse.transpose;

                    //can't modify the arrays we get from the cache because they will be modified multiple times if the same mesh is being added multiple times.
                    nverts = meshChannelCache.GetVertices(mesh);
                    Vector3[] nnorms = null;
                    Vector4[] ntangs = null;
                    if (settings.doNorm) nnorms = meshChannelCache.GetNormals(mesh);
                    if (settings.doTan) ntangs = meshChannelCache.GetTangents(mesh);
                    if (settings.renderType != MB_RenderType.skinnedMeshRenderer)
                    {
                        for (int j = 0; j < nverts.Length; j++)
                        {
                            int vIdx = vertsIdx + j;
                            verts[vertsIdx + j] = l2wMat.MultiplyPoint3x4(nverts[j]);
                            if (settings.doNorm)
                            {
                                normals[vIdx] = l2wRotScale.MultiplyPoint3x4(nnorms[j]).normalized;
                            }
                            if (settings.doTan)
                            {
                                float w = ntangs[j].w; //need to preserve the w value
                                tangents[vIdx] = l2wRotScale.MultiplyPoint3x4(((Vector3)ntangs[j])).normalized;
                                tangents[vIdx].w = w;
                            }
                        }
                    }
                    else
                    {
                        //for skinned meshes leave in bind pose
                        boneProcessor.CopyVertsNormsTansToBuffers(dgo, settings, vertsIdx, nnorms, ntangs, nverts, normals, tangents, verts);
                    }

                    int numTriSets = mesh.subMeshCount;
                    if (dgo.uvRects.Length < numTriSets)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Mesh " + dgo.name + " has more submeshes than materials");
                        numTriSets = dgo.uvRects.Length;
                    }
                    else if (dgo.uvRects.Length > numTriSets)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + dgo.name + " has fewer submeshes than materials");
                    }

                    if (settings.doUV)
                    {
                        uvAdjuster._copyAndAdjustUVsFromMesh(textureBakeResults, dgo, mesh, 0, vertsIdx, uvs, uvsSliceIdx, meshChannelCache);
                    }

                    if (doUV2(ref settings))
                    {
                        _copyAndAdjustUV2FromMesh(settings, ref uv2s, dgo, mesh, vertsIdx, meshChannelCache, LOG_LEVEL);
                    }

                    if (settings.doUV3)
                    {
                        nuv3s = meshChannelCache.GetUVChannel(3, mesh);
                        nuv3s.CopyTo(uv3s, vertsIdx);
                    }

                    if (settings.doUV4)
                    {
                        nuv4s = meshChannelCache.GetUVChannel(4, mesh);
                        nuv4s.CopyTo(uv4s, vertsIdx);
                    }

                    if (settings.doUV5)
                    {
                        nuv5s = meshChannelCache.GetUVChannel(5, mesh);
                        nuv5s.CopyTo(uv5s, vertsIdx);
                    }

                    if (settings.doUV6)
                    {
                        nuv6s = meshChannelCache.GetUVChannel(6, mesh);
                        nuv6s.CopyTo(uv6s, vertsIdx);
                    }

                    if (settings.doUV7)
                    {
                        nuv7s = meshChannelCache.GetUVChannel(7, mesh);
                        nuv7s.CopyTo(uv7s, vertsIdx);
                    }

                    if (settings.doUV8)
                    {
                        nuv8s = meshChannelCache.GetUVChannel(8, mesh);
                        nuv8s.CopyTo(uv8s, vertsIdx);
                    }

                    if (settings.doCol)
                    {
                        ncolors = meshChannelCache.GetColors(mesh);
                        ncolors.CopyTo(colors, vertsIdx);
                    }

                    if (settings.doBlendShapes)
                    {
                        nblendShapes = meshChannelCache.GetBlendShapes(mesh, dgo.instanceID, dgo.gameObject);
                        nblendShapes.CopyTo(blendShapes, blendShapeIdx);
                    }

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        Renderer r = MB_Utility.GetRenderer(go);
                        MB3_MeshCombinerSimpleBones.AddBonesToNewBonesArrayAndAdjustBWIndexes(bindPoses, dgo, r, vertsIdx, nbones, nboneWeights, meshChannelCache);
                    }

                    for (int combinedMeshIdx = 0; combinedMeshIdx < targSubmeshTidx.Length; combinedMeshIdx++)
                    {
                        dgo.submeshTriIdxs[combinedMeshIdx] = targSubmeshTidx[combinedMeshIdx];
                    }
                    for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++)
                    {
                        int[] sts = dgo._tmpSubmeshTris[j].data;
                        for (int k = 0; k < sts.Length; k++)
                        {
                            sts[k] = sts[k] + vertsIdx;
                        }
                        if (dgo.invertTriangles)
                        {
                            //need to reverse winding order
                            for (int k = 0; k < sts.Length; k += 3)
                            {
                                int tmp = sts[k];
                                sts[k] = sts[k + 1];
                                sts[k + 1] = tmp;
                            }
                        }
                        int combinedMeshIdx = dgo.targetSubmeshIdxs[j];
                        sts.CopyTo(submeshTris[combinedMeshIdx].data, targSubmeshTidx[combinedMeshIdx]);
                        dgo.submeshNumTris[combinedMeshIdx] += sts.Length;
                        targSubmeshTidx[combinedMeshIdx] += sts.Length;
                    }

                    dgo.vertIdx = targVidx;
                    dgo.blendShapeIdx = targBlendShapeIdx;

                    instance2Combined_MapAdd(ref _instance2combined_map, go, dgo);
                    objectsInCombinedMesh.Add(go);
                    mbDynamicObjectsInCombinedMesh.Add(dgo);

                    targVidx += nverts.Length;
                    if (settings.doBlendShapes)
                    {
                        targBlendShapeIdx += nblendShapes.Length;
                    }
                    for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++) dgo._tmpSubmeshTris[j] = null;
                    dgo._tmpSubmeshTris = null;
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Added to combined:" + dgo.name + " verts:" + nverts.Length + " bindPoses:" + nbindPoses.Length, LOG_LEVEL);
                }
                if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects)
                {
                    _copyUV2unchangedToSeparateRects(mbDynamicObjectsInCombinedMesh, ref uv2s);
                }

                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("===== _addToCombined completed. Verts in buffer: " + verts.Length + " time(ms): " + sw.ElapsedMilliseconds, LOG_LEVEL);
                // At this point in the original implementation verts[], normals[], tangents[] etc. would be holding all the new data
                // We want it to fill the buffers we created so we'll assign all the finished work back to nverts[], nnormals[], ntangents[] etc

                nverts = verts;
                nnormals = normals;
                ntangents = tangents;
                nuvs = uvs;
                nuv2s = uv2s;
                nuv3s = uv3s;
                nuv4s = uv4s;
                nuv5s = uv5s;
                nuv6s = uv6s;
                nuv7s = uv7s;
                nuv8s = uv8s;
                ncolors = colors;
                nboneWeights = boneWeights;
                nbones = bones;
                nbindPoses = bindPoses;
                nuvsSliceIdx = uvsSliceIdx;
                nblendShapes = blendShapes;
                nsubmeshTris = submeshTris;
                return true;
            }

            public static bool _addToCombined(
                MB3_MeshCombinerSingle c,
                int totalAddVerts,
                int totalDeleteVerts,
                MB3_MeshCombinerSimpleBones boneProcessor,
                int numResultMats,
                int totalAddBlendShapes,
                int totalDeleteBlendShapes,
                int[] totalAddSubmeshTris,
                int[] totalDeleteSubmeshTris,
                int[] _goToDelete,
                List<MB_DynamicGameObject> toAddDGOs,
                GameObject[] _goToAdd,
                MeshChannelsCache meshChannelCache,
                UVAdjuster_Atlas uvAdjuster,
                System.Diagnostics.Stopwatch sw
                )
            {
                /*
                ref Vector3[] verts = ref combiner.verts;
                ref Vector3[] normals = ref combiner.normals;
                ref Vector4[] tangents = ref combiner.tangents;
                ref Vector2[] uvs = ref combiner.uvs;
                ref Vector2[] uv2s = ref combiner.uv2s;
                ref Vector2[] uv3s = ref combiner.uv3s;
                ref Vector2[] uv4s = ref combiner.uv4s;
                ref Vector2[] uv5s = ref combiner.uv5s;
                ref Vector2[] uv6s = ref combiner.uv6s;
                ref Vector2[] uv7s = ref combiner.uv7s;
                ref Vector2[] uv8s = ref combiner.uv8s;
                ref Color[] colors = ref combiner.colors;
                ref BoneWeight[] boneWeights = ref combiner.boneWeights;
                ref Transform[] bones = ref combiner.bones;
                ref Matrix4x4[] bindPoses = ref combiner.bindPoses;
                ref float[] uvsSliceIdx = ref combiner.uvsSliceIdx;
                */
                MB_IMeshBakerSettings settings = c.settings;
                //ref MBBlendShape[] blendShapes = ref combiner.blendShapes;
                MB2_LogLevel LOG_LEVEL = c.LOG_LEVEL;

                //ref SerializableIntArray[] submeshTris = ref combiner.submeshTris;
                
                MB2_TextureBakeResults textureBakeResults = c.textureBakeResults;
                List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = c.mbDynamicObjectsInCombinedMesh;
                List<GameObject> objectsInCombinedMesh = c.objectsInCombinedMesh;
                Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map = c._instance2combined_map;

                //STEP 2 to allocate new buffers and copy everything over
                int newVertSize = c.verts.Length + totalAddVerts - totalDeleteVerts;
                int newBonesSize = boneProcessor.GetNewBonesLength();
                int[] newSubmeshTrisSize = new int[numResultMats];
                int newBlendShapeSize = 0;
                if (settings.doBlendShapes) newBlendShapeSize = c.blendShapes.Length + totalAddBlendShapes - totalDeleteBlendShapes;
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Verts adding:" + totalAddVerts + " deleting:" + totalDeleteVerts + " submeshes:" + newSubmeshTrisSize.Length + " bones:" + newBonesSize + " blendShapes:" + newBlendShapeSize);

                for (int i = 0; i < newSubmeshTrisSize.Length; i++)
                {
                    newSubmeshTrisSize[i] = c.submeshTris[i].data.Length + totalAddSubmeshTris[i] - totalDeleteSubmeshTris[i];
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("    submesh :" + i + " already contains:" + c.submeshTris[i].data.Length + " tris to be Added:" + totalAddSubmeshTris[i] + " tris to be Deleted:" + totalDeleteSubmeshTris[i]);
                }

                if (newVertSize >= MBVersion.MaxMeshVertexCount())
                {
                    Debug.LogError("Cannot add objects. Resulting mesh will have more than " + MBVersion.MaxMeshVertexCount() + " vertices. Try using a Multi-MeshBaker component. This will split the combined mesh into several meshes. You don't have to re-configure the MB2_TextureBaker. Just remove the MB2_MeshBaker component and add a MB2_MultiMeshBaker component.");
                    return false;
                }

                Vector3[] nnormals = null;
                Vector4[] ntangents = null;
                Vector2[] nuvs = null, nuv2s = null, nuv3s = null, nuv4s = null, nuv5s = null, nuv6s = null, nuv7s = null, nuv8s = null;
                float[] nuvsSliceIdx = null;
                Color[] ncolors = null;
                MBBlendShape[] nblendShapes = null;
                Vector3[] nverts = new Vector3[newVertSize];

                if (settings.doNorm) nnormals = new Vector3[newVertSize];
                if (settings.doTan) ntangents = new Vector4[newVertSize];
                if (settings.doUV) nuvs = new Vector2[newVertSize];
                if (settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray) nuvsSliceIdx = new float[newVertSize];
                if (settings.doUV3) nuv3s = new Vector2[newVertSize];
                if (settings.doUV4) nuv4s = new Vector2[newVertSize];

                if (settings.doUV5) nuv5s = new Vector2[newVertSize];
                if (settings.doUV6) nuv6s = new Vector2[newVertSize];
                if (settings.doUV7) nuv7s = new Vector2[newVertSize];
                if (settings.doUV8) nuv8s = new Vector2[newVertSize];

                if (doUV2(ref settings))
                {
                    nuv2s = new Vector2[newVertSize];
                }
                if (settings.doCol) ncolors = new Color[newVertSize];
                if (settings.doBlendShapes) nblendShapes = new MBBlendShape[newBlendShapeSize];

                BoneWeight[] nboneWeights = new BoneWeight[newVertSize];
                Matrix4x4[] nbindPoses = new Matrix4x4[newBonesSize];
                Transform[] nbones = new Transform[newBonesSize];
                SerializableIntArray[] nsubmeshTris = new SerializableIntArray[numResultMats];

                for (int i = 0; i < nsubmeshTris.Length; i++)
                {
                    nsubmeshTris[i] = new SerializableIntArray(newSubmeshTrisSize[i]);
                }

                for (int i = 0; i < _goToDelete.Length; i++)
                {
                    MB_DynamicGameObject dgo = null;
                    InstanceID2DGO(ref mbDynamicObjectsInCombinedMesh, _goToDelete[i], out dgo);
                    if (dgo != null)
                    {
                        dgo._beingDeleted = true;
                    }
                }

                mbDynamicObjectsInCombinedMesh.Sort();

                //copy existing arrays to narrays gameobj by gameobj omitting deleted ones
                int targVidx = 0;
                int targBlendShapeIdx = 0;
                int[] targSubmeshTidx = new int[numResultMats];
                int triangleIdxAdjustment = 0;
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    if (!dgo._beingDeleted)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Copying obj in combined arrays idx:" + i, LOG_LEVEL);
                        Array.Copy(c.verts, dgo.vertIdx, nverts, targVidx, dgo.numVerts);
                        if (settings.doNorm) { Array.Copy(c.normals, dgo.vertIdx, nnormals, targVidx, dgo.numVerts); }
                        if (settings.doTan) { Array.Copy(c.tangents, dgo.vertIdx, ntangents, targVidx, dgo.numVerts); }
                        if (settings.doUV) { Array.Copy(c.uvs, dgo.vertIdx, nuvs, targVidx, dgo.numVerts); }
                        if (settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray) { Array.Copy(c.uvsSliceIdx, dgo.vertIdx, nuvsSliceIdx, targVidx, dgo.numVerts); }
                        if (settings.doUV3) { Array.Copy(c.uv3s, dgo.vertIdx, nuv3s, targVidx, dgo.numVerts); }
                        if (settings.doUV4) { Array.Copy(c.uv4s, dgo.vertIdx, nuv4s, targVidx, dgo.numVerts); }

                        if (settings.doUV5) { Array.Copy(c.uv5s, dgo.vertIdx, nuv5s, targVidx, dgo.numVerts); }
                        if (settings.doUV6) { Array.Copy(c.uv6s, dgo.vertIdx, nuv6s, targVidx, dgo.numVerts); }
                        if (settings.doUV7) { Array.Copy(c.uv7s, dgo.vertIdx, nuv7s, targVidx, dgo.numVerts); }
                        if (settings.doUV8) { Array.Copy(c.uv8s, dgo.vertIdx, nuv8s, targVidx, dgo.numVerts); }

                        if (doUV2(ref settings)) { Array.Copy(c.uv2s, dgo.vertIdx, nuv2s, targVidx, dgo.numVerts); }
                        if (settings.doCol) { Array.Copy(c.colors, dgo.vertIdx, ncolors, targVidx, dgo.numVerts); }
                        if (settings.doBlendShapes) { Array.Copy(c.blendShapes, dgo.blendShapeIdx, nblendShapes, targBlendShapeIdx, dgo.numBlendShapes); }
                        if (settings.renderType == MB_RenderType.skinnedMeshRenderer) { Array.Copy(c.boneWeights, dgo.vertIdx, nboneWeights, targVidx, dgo.numVerts); }

                        //adjust triangles, then copy them over
                        for (int subIdx = 0; subIdx < numResultMats; subIdx++)
                        {
                            int[] sTris = c.submeshTris[subIdx].data;
                            int sTriIdx = dgo.submeshTriIdxs[subIdx];
                            int sNumTris = dgo.submeshNumTris[subIdx];
                            if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("    Adjusting submesh triangles submesh:" + subIdx + " startIdx:" + sTriIdx + " num:" + sNumTris + " nsubmeshTris:" + nsubmeshTris.Length + " targSubmeshTidx:" + targSubmeshTidx.Length, LOG_LEVEL);
                            for (int j = sTriIdx; j < sTriIdx + sNumTris; j++)
                            {
                                sTris[j] = sTris[j] - triangleIdxAdjustment;
                            }
                            Array.Copy(sTris, sTriIdx, nsubmeshTris[subIdx].data, targSubmeshTidx[subIdx], sNumTris);
                        }

                        dgo.vertIdx = targVidx;
                        dgo.blendShapeIdx = targBlendShapeIdx;

                        for (int j = 0; j < targSubmeshTidx.Length; j++)
                        {
                            dgo.submeshTriIdxs[j] = targSubmeshTidx[j];
                            targSubmeshTidx[j] += dgo.submeshNumTris[j];
                        }
                        targBlendShapeIdx += dgo.numBlendShapes;
                        targVidx += dgo.numVerts;
                    }
                    else
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Not copying obj: " + i, LOG_LEVEL);
                        triangleIdxAdjustment += dgo.numVerts;
                    }
                }

                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    boneProcessor.CopyBonesWeAreKeepingToNewBonesArrayAndAdjustBWIndexes(nbones, nbindPoses, nboneWeights, totalDeleteVerts);
                }

                //remove objects we are deleting
                for (int i = mbDynamicObjectsInCombinedMesh.Count - 1; i >= 0; i--)
                {
                    if (mbDynamicObjectsInCombinedMesh[i]._beingDeleted)
                    {
                        instance2Combined_MapRemove(ref _instance2combined_map, mbDynamicObjectsInCombinedMesh[i].gameObject);
                        objectsInCombinedMesh.RemoveAt(i);
                        mbDynamicObjectsInCombinedMesh.RemoveAt(i);
                    }
                }

                c.verts = nverts;
                if (settings.doNorm) c.normals = nnormals;
                if (settings.doTan) c.tangents = ntangents;
                if (settings.doUV) c.uvs = nuvs;
                if (settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray) c.uvsSliceIdx = nuvsSliceIdx;
                if (settings.doUV3) c.uv3s = nuv3s;
                if (settings.doUV4) c.uv4s = nuv4s;

                if (settings.doUV5) c.uv5s = nuv5s;
                if (settings.doUV6) c.uv6s = nuv6s;
                if (settings.doUV7) c.uv7s = nuv7s;
                if (settings.doUV8) c.uv8s = nuv8s;

                if (doUV2(ref settings)) c.uv2s = nuv2s;
                if (settings.doCol) c.colors = ncolors;
                if (settings.doBlendShapes) c.blendShapes = nblendShapes;
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer) c.boneWeights = nboneWeights;
                int newBonesStartAtIdx = c.bones.Length - boneProcessor.GetNumBonesToDelete();
                c.bindPoses = nbindPoses;
                c.bones = nbones;
                c.submeshTris = nsubmeshTris;

                //insert the new bones into the bones array
                int bidx = 0;
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    foreach (BoneAndBindpose t in boneProcessor.GetBonesToAdd())
                    {
                        nbones[newBonesStartAtIdx + bidx] = t.bone;
                        nbindPoses[newBonesStartAtIdx + bidx] = t.bindPose;
                        bidx++;
                    }
                }

                //add new
                for (int i = 0; i < toAddDGOs.Count; i++)
                {
                    MB_DynamicGameObject dgo = toAddDGOs[i];
                    GameObject go = _goToAdd[i];
                    int vertsIdx = targVidx;
                    int blendShapeIdx = targBlendShapeIdx;
                    //				Profile.StartProfile("TestNewNorm");
                    Mesh mesh = MB_Utility.GetMesh(go);
                    Matrix4x4 l2wMat = go.transform.localToWorldMatrix;

                    // Similar to local2world but with translation removed and we are using the inverse transpose.
                    // We use this for normals and tangents because it handles scaling correctly.
                    Matrix4x4 l2wRotScale = l2wMat;
                    l2wRotScale[0, 3] = l2wRotScale[1, 3] = l2wRotScale[2, 3] = 0f;
                    l2wRotScale = l2wRotScale.inverse.transpose;

                    //can't modify the arrays we get from the cache because they will be modified multiple times if the same mesh is being added multiple times.
                    nverts = meshChannelCache.GetVertices(mesh);
                    Vector3[] nnorms = null;
                    Vector4[] ntangs = null;
                    if (settings.doNorm) nnorms = meshChannelCache.GetNormals(mesh);
                    if (settings.doTan) ntangs = meshChannelCache.GetTangents(mesh);
                    if (settings.renderType != MB_RenderType.skinnedMeshRenderer)
                    {
                        for (int j = 0; j < nverts.Length; j++)
                        {
                            int vIdx = vertsIdx + j;
                            c.verts[vertsIdx + j] = l2wMat.MultiplyPoint3x4(nverts[j]);
                            if (settings.doNorm)
                            {
                                c.normals[vIdx] = l2wRotScale.MultiplyPoint3x4(nnorms[j]).normalized;
                            }
                            if (settings.doTan)
                            {
                                float w = ntangs[j].w; //need to preserve the w value
                                c.tangents[vIdx] = l2wRotScale.MultiplyPoint3x4(((Vector3)ntangs[j])).normalized;
                                c.tangents[vIdx].w = w;
                            }
                        }
                    }
                    else
                    {
                        //for skinned meshes leave in bind pose
                        boneProcessor.CopyVertsNormsTansToBuffers(dgo, settings, vertsIdx, nnorms, ntangs, nverts, c.normals, c.tangents, c.verts);
                    }

                    int numTriSets = mesh.subMeshCount;
                    if (dgo.uvRects.Length < numTriSets)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Mesh " + dgo.name + " has more submeshes than materials");
                        numTriSets = dgo.uvRects.Length;
                    }
                    else if (dgo.uvRects.Length > numTriSets)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + dgo.name + " has fewer submeshes than materials");
                    }

                    if (settings.doUV)
                    {
                        uvAdjuster._copyAndAdjustUVsFromMesh(textureBakeResults, dgo, mesh, 0, vertsIdx, c.uvs, c.uvsSliceIdx, meshChannelCache);
                    }

                    if (doUV2(ref settings))
                    {
                        _copyAndAdjustUV2FromMesh(settings,ref c.uv2s, dgo, mesh, vertsIdx, meshChannelCache, LOG_LEVEL);
                    }

                    if (settings.doUV3)
                    {
                        nuv3s = meshChannelCache.GetUVChannel(3, mesh);
                        nuv3s.CopyTo(c.uv3s, vertsIdx);
                    }

                    if (settings.doUV4)
                    {
                        nuv4s = meshChannelCache.GetUVChannel(4, mesh);
                        nuv4s.CopyTo(c.uv4s, vertsIdx);
                    }

                    if (settings.doUV5)
                    {
                        nuv5s = meshChannelCache.GetUVChannel(5, mesh);
                        nuv5s.CopyTo(c.uv5s, vertsIdx);
                    }

                    if (settings.doUV6)
                    {
                        nuv6s = meshChannelCache.GetUVChannel(6, mesh);
                        nuv6s.CopyTo(c.uv6s, vertsIdx);
                    }

                    if (settings.doUV7)
                    {
                        nuv7s = meshChannelCache.GetUVChannel(7, mesh);
                        nuv7s.CopyTo(c.uv7s, vertsIdx);
                    }

                    if (settings.doUV8)
                    {
                        nuv8s = meshChannelCache.GetUVChannel(8, mesh);
                        nuv8s.CopyTo(c.uv8s, vertsIdx);
                    }

                    if (settings.doCol)
                    {
                        ncolors = meshChannelCache.GetColors(mesh);
                        ncolors.CopyTo(c.colors, vertsIdx);
                    }

                    if (settings.doBlendShapes)
                    {
                        nblendShapes = meshChannelCache.GetBlendShapes(mesh, dgo.instanceID, dgo.gameObject);
                        nblendShapes.CopyTo(c.blendShapes, blendShapeIdx);
                    }

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        Renderer r = MB_Utility.GetRenderer(go);
                        MB3_MeshCombinerSimpleBones.AddBonesToNewBonesArrayAndAdjustBWIndexes(c.bindPoses, dgo, r, vertsIdx, nbones, nboneWeights, meshChannelCache);
                    }

                    for (int combinedMeshIdx = 0; combinedMeshIdx < targSubmeshTidx.Length; combinedMeshIdx++)
                    {
                        dgo.submeshTriIdxs[combinedMeshIdx] = targSubmeshTidx[combinedMeshIdx];
                    }
                    for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++)
                    {
                        int[] sts = dgo._tmpSubmeshTris[j].data;
                        for (int k = 0; k < sts.Length; k++)
                        {
                            sts[k] = sts[k] + vertsIdx;
                        }
                        if (dgo.invertTriangles)
                        {
                            //need to reverse winding order
                            for (int k = 0; k < sts.Length; k += 3)
                            {
                                int tmp = sts[k];
                                sts[k] = sts[k + 1];
                                sts[k + 1] = tmp;
                            }
                        }
                        int combinedMeshIdx = dgo.targetSubmeshIdxs[j];
                        sts.CopyTo(c.submeshTris[combinedMeshIdx].data, targSubmeshTidx[combinedMeshIdx]);
                        dgo.submeshNumTris[combinedMeshIdx] += sts.Length;
                        targSubmeshTidx[combinedMeshIdx] += sts.Length;
                    }

                    dgo.vertIdx = targVidx;
                    dgo.blendShapeIdx = targBlendShapeIdx;

                    instance2Combined_MapAdd(ref _instance2combined_map, go, dgo);
                    objectsInCombinedMesh.Add(go);
                    mbDynamicObjectsInCombinedMesh.Add(dgo);

                    targVidx += nverts.Length;
                    if (settings.doBlendShapes)
                    {
                        targBlendShapeIdx += nblendShapes.Length;
                    }
                    for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++) dgo._tmpSubmeshTris[j] = null;
                    dgo._tmpSubmeshTris = null;
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Added to combined:" + dgo.name + " verts:" + nverts.Length + " bindPoses:" + nbindPoses.Length, LOG_LEVEL);
                }
                if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects)
                {
                    _copyUV2unchangedToSeparateRects(c.mbDynamicObjectsInCombinedMesh, ref c.uv2s);
                }

                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("===== _addToCombined completed. Verts in buffer: " + c.verts.Length + " time(ms): " + sw.ElapsedMilliseconds, LOG_LEVEL);
                return true;
            }


            public static bool _updateGameObject(MB3_MeshCombinerSingle combiner, Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map, GameObject go, bool updateVertices, bool updateNormals, bool updateTangents,
                                            bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                            bool updateColors, bool updateSkinningInfo,
                                            MeshChannelsCache meshChannelCache, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache,
                                            OrderedDictionary sourceMats2submeshIdx_map, UVAdjuster_Atlas uVAdjuster, MB2_LogLevel LOG_LEVEL)
            {
                MB_IMeshBakerSettings settings = combiner.settings;
                MB_DynamicGameObject dgo = null;
                if (!_instance2combined_map.TryGetValue(go, out dgo))
                {
                    Debug.LogError("Object " + go.name + " has not been added");
                    return false;
                }
                Mesh mesh = MB_Utility.GetMesh(go);
                if (dgo.numVerts != mesh.vertexCount)
                {
                    Debug.LogError("Object " + go.name + " source mesh has been modified since being added. To update it must have the same number of verts");
                    return false;
                }

                if (settings.doUV && updateUV)
                {
                    // updating UVs is a bit more complicated because most likely the user has changed
                    // the material on the source mesh which is why they are calling update. We need to
                    // find the UV rect for this.

                    Material[] sharedMaterials = MB_Utility.GetGOMaterials(go);
                    if (!uVAdjuster.MapSharedMaterialsToAtlasRects(sharedMaterials, true, mesh, meshChannelCache, meshAnalysisResultsCache, sourceMats2submeshIdx_map, go, dgo))
                    {
                        return false;
                    }

                    uVAdjuster._copyAndAdjustUVsFromMesh(combiner.textureBakeResults, dgo, mesh, 0, dgo.vertIdx, combiner.uvs, combiner.uvsSliceIdx, meshChannelCache);
                }
                if (doUV2(ref settings) && updateUV2) _copyAndAdjustUV2FromMesh(settings, ref combiner.uv2s, dgo, mesh, dgo.vertIdx, meshChannelCache, LOG_LEVEL);
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer && updateSkinningInfo)
                {
                    Renderer r = MB_Utility.GetRenderer(go);
                    MB3_MeshCombinerSimpleBones.UpdateBWIndexes(dgo, r, combiner._getBones(r, dgo.isSkinnedMeshWithBones), combiner.bones, combiner.boneWeights, meshChannelCache);
                }

                //now do verts, norms, tangents, colors and uv1
                Matrix4x4 l2wMat = go.transform.localToWorldMatrix;

                // We use the inverse transpose for normals and tangents because it handles scaling of normals the same way that 
                // The shaders do.
                Matrix4x4 l2wRotScale = l2wMat;
                l2wRotScale[0, 3] = l2wRotScale[1, 3] = l2wRotScale[2, 3] = 0f;
                l2wRotScale = l2wRotScale.inverse.transpose;
                if (updateVertices)
                {
                    Vector3[] nverts = meshChannelCache.GetVertices(mesh);
                    for (int j = 0; j < nverts.Length; j++)
                    {
                        combiner.verts[dgo.vertIdx + j] = l2wMat.MultiplyPoint3x4(nverts[j]);
                    }
                }
                l2wMat[0, 3] = l2wMat[1, 3] = l2wMat[2, 3] = 0f;
                if (settings.doNorm && updateNormals)
                {
                    Vector3[] nnorms = meshChannelCache.GetNormals(mesh);
                    for (int j = 0; j < nnorms.Length; j++)
                    {
                        int vIdx = dgo.vertIdx + j;
                        combiner.normals[vIdx] = l2wRotScale.MultiplyPoint3x4(nnorms[j]).normalized;
                    }
                }
                if (settings.doTan && updateTangents)
                {
                    Vector4[] ntangs = meshChannelCache.GetTangents(mesh);
                    for (int j = 0; j < ntangs.Length; j++)
                    {
                        int vIdx = dgo.vertIdx + j;
                        float w = ntangs[j].w; //need to preserve the w value
                        combiner.tangents[vIdx] = l2wRotScale.MultiplyPoint3x4(((Vector3)ntangs[j])).normalized;
                        combiner.tangents[vIdx].w = w;
                    }
                }

                if (settings.doCol && updateColors)
                {
                    Color[] ncolors = meshChannelCache.GetColors(mesh);
                    for (int j = 0; j < ncolors.Length; j++) combiner.colors[dgo.vertIdx + j] = ncolors[j];
                }

                if (settings.doUV3 && updateUV3)
                {
                    Vector2[] nuv3 = meshChannelCache.GetUVChannel(3, mesh);
                    for (int j = 0; j < nuv3.Length; j++) combiner.uv3s[dgo.vertIdx + j] = nuv3[j];
                }

                if (settings.doUV4 && updateUV4)
                {
                    Vector2[] nuv4 = meshChannelCache.GetUVChannel(4, mesh);
                    for (int j = 0; j < nuv4.Length; j++) combiner.uv4s[dgo.vertIdx + j] = nuv4[j];
                }

                if (settings.doUV5 && updateUV5)
                {
                    Vector2[] nuv5 = meshChannelCache.GetUVChannel(5, mesh);
                    for (int j = 0; j < nuv5.Length; j++) combiner.uv5s[dgo.vertIdx + j] = nuv5[j];
                }

                if (settings.doUV6 && updateUV6)
                {
                    Vector2[] nuv6 = meshChannelCache.GetUVChannel(6, mesh);
                    for (int j = 0; j < nuv6.Length; j++) combiner.uv6s[dgo.vertIdx + j] = nuv6[j];
                }

                if (settings.doUV7 && updateUV7)
                {
                    Vector2[] nuv7 = meshChannelCache.GetUVChannel(7, mesh);
                    for (int j = 0; j < nuv7.Length; j++) combiner.uv7s[dgo.vertIdx + j] = nuv7[j];
                }

                if (settings.doUV8 && updateUV8)
                {
                    Vector2[] nuv8 = meshChannelCache.GetUVChannel(8, mesh);
                    for (int j = 0; j < nuv8.Length; j++) combiner.uv8s[dgo.vertIdx + j] = nuv8[j];
                }

                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    ((SkinnedMeshRenderer)combiner.targetRenderer).sharedMesh = null;
                    ((SkinnedMeshRenderer)combiner.targetRenderer).sharedMesh = combiner._mesh;
                }

                return true;
            }


            public static void Apply(MB3_MeshCombinerSingle combiner, GenerateUV2Delegate uv2GenerationMethod)
            {
                MB_IMeshBakerSettings settings = combiner.settings;
                bool doBones = false;
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer) doBones = true;
                Apply(combiner, true, true, settings.doNorm, settings.doTan,
                    settings.doUV, doUV2(ref settings), settings.doUV3, settings.doUV4, settings.doUV5, settings.doUV6, settings.doUV7, settings.doUV8,
                    settings.doCol, doBones, settings.doBlendShapes, uv2GenerationMethod);
            }

            public static void Apply(MB3_MeshCombinerSingle combiner, bool triangles,
                          bool vertices,
                          bool normals,
                          bool tangents,
                          bool uvs,
                          bool uv2,
                          bool uv3,
                          bool uv4,
                          bool colors,
                          bool bones = false,
                          bool blendShapesFlag = false,
                          GenerateUV2Delegate uv2GenerationMethod = null)
            {
                Apply(combiner, triangles, vertices, normals, tangents,
                    uvs, uv2, uv3, uv4,
                    false, false, false, false,
                    colors, bones, blendShapesFlag, uv2GenerationMethod);
            }

            public static void Apply(MB3_MeshCombinerSingle combiner, bool triangles,
                         bool vertices,
                         bool normals,
                         bool tangents,
                         bool uvs,
                         bool uv2,
                         bool uv3,
                         bool uv4,
                         bool uv5,
                         bool uv6,
                         bool uv7,
                         bool uv8,
                         bool colors,
                         bool bones = false,
                         bool blendShapesFlag = false,
                         GenerateUV2Delegate uv2GenerationMethod = null)
            {
                MB2_LogLevel LOG_LEVEL = combiner.LOG_LEVEL;
                MB2_ValidationLevel _validationLevel = combiner._validationLevel;
                MB_IMeshBakerSettings settings = combiner.settings;

                Mesh _mesh = combiner._mesh;
                Vector3[] verts = combiner.verts;
                Renderer targetRenderer = combiner.targetRenderer;
                MB2_TextureBakeResults _textureBakeResults = combiner._textureBakeResults;
                MB2_TextureBakeResults textureBakeResults = combiner.textureBakeResults;

                System.Diagnostics.Stopwatch sw = null;
                if (LOG_LEVEL >= MB2_LogLevel.debug)
                {
                    sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                }
                if (_validationLevel >= MB2_ValidationLevel.quick && !combiner.ValidateTargRendererAndMeshAndResultSceneObj()) return;
                if (_mesh != null)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.trace)
                    {
                        Debug.Log(String.Format("Apply called tri={0} vert={1} norm={2} tan={3} uv={4} col={5} uv3={6} uv4={7} uv2={8} bone={9} blendShape{10} meshID={11}",
                            triangles, vertices, normals, tangents, uvs, colors, uv3, uv4, uv2, bones, blendShapesFlag, _mesh.GetInstanceID()));
                    }
                    if (triangles || _mesh.vertexCount != verts.Length)
                    {
                        bool justClearTriangles = triangles && !vertices && !normals && !tangents && !uvs && !colors && !uv3 && !uv4 && !uv2 && !bones;
                        MBVersion.SetMeshIndexFormatAndClearMesh(_mesh, verts.Length, vertices, justClearTriangles);
                    }

                    if (vertices)
                    {
                        Vector3[] verts2Write = verts;
                        if (verts.Length > 0)
                        {
                            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                            {
                                targetRenderer.transform.position = Vector3.zero;
                            }
                            else if (settings.pivotLocationType == MB_MeshPivotLocation.worldOrigin)
                            {
                                targetRenderer.transform.position = Vector3.zero;
                            }
                            else if (settings.pivotLocationType == MB_MeshPivotLocation.boundsCenter)
                            {

                                Vector3 max = verts[0], min = verts[0];
                                for (int i = 1; i < verts.Length; i++)
                                {
                                    Vector3 v = verts[i];
                                    if (max.x < v.x) max.x = v.x;
                                    if (max.y < v.y) max.y = v.y;
                                    if (max.z < v.z) max.z = v.z;
                                    if (min.x > v.x) min.x = v.x;
                                    if (min.y > v.y) min.y = v.y;
                                    if (min.z > v.z) min.z = v.z;
                                }

                                Vector3 center = (max + min) / 2f;

                                verts2Write = new Vector3[verts.Length];
                                for (int i = 0; i < verts.Length; i++)
                                {
                                    verts2Write[i] = verts[i] - center;
                                }

                                targetRenderer.transform.position = center;
                            }
                            else if (settings.pivotLocationType == MB_MeshPivotLocation.customLocation)
                            {
                                Vector3 center = settings.pivotLocation;
                                for (int i = 0; i < verts.Length; i++)
                                {
                                    verts2Write[i] = verts[i] - center;
                                }

                                targetRenderer.transform.position = center;
                            }
                        }

                        _mesh.vertices = verts2Write;
                    }
                    if (triangles && _textureBakeResults)
                    {
                        if (_textureBakeResults == null)
                        {
                            Debug.LogError("Texture Bake Result was not set.");
                        }
                        else
                        {
                            SerializableIntArray[] submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(combiner);

                            //submeshes with zero length tris cause error messages. must exclude these
                            int numNonZero = _mesh.subMeshCount = _numNonZeroLengthSubmeshTris(submeshTrisToUse);// submeshTrisToUse.Length;
                            int submeshIdx = 0;
                            for (int i = 0; i < submeshTrisToUse.Length; i++)
                            {
                                if (submeshTrisToUse[i].data.Length != 0)
                                {
                                    _mesh.SetTriangles(submeshTrisToUse[i].data, submeshIdx);
                                    submeshIdx++;
                                }
                            }

                            _updateMaterialsOnTargetRenderer(combiner.textureBakeResults, combiner.targetRenderer, submeshTrisToUse, numNonZero);
                        }
                    }
                    if (normals)
                    {
                        if (settings.doNorm)
                        {
                            _mesh.normals = combiner.normals;
                        }
                        else { Debug.LogError("normal flag was set in Apply but MeshBaker didn't generate normals"); }
                    }

                    if (tangents)
                    {
                        if (settings.doTan) { _mesh.tangents = combiner.tangents; }
                        else { Debug.LogError("tangent flag was set in Apply but MeshBaker didn't generate tangents"); }
                    }
                    if (colors)
                    {
                        if (settings.doCol)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                _mesh.colors = combiner.colors;
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_colors(settings, textureBakeResults, _mesh, combiner.colors, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("color flag was set in Apply but MeshBaker didn't generate colors"); }
                    }
                    if (uvs)
                    {
                        if (settings.doUV)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                _mesh.uv = combiner.uvs;
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV0(0, settings, textureBakeResults, _mesh, combiner.uvs, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("uv flag was set in Apply but MeshBaker didn't generate uvs"); }
                    }
                    if (uv2)
                    {
                        if (doUV2(ref settings))
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                _mesh.uv2 = combiner.uv2s;
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV2(2, settings, textureBakeResults, _mesh, combiner.uv2s, combiner.uvsSliceIdx);
                            }

                        }
                        else { Debug.LogError("uv2 flag was set in Apply but lightmapping option was set to " + settings.lightmapOption); }
                    }
                    if (uv3)
                    {
                        if (settings.doUV3)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                MBVersion.MeshAssignUVChannel(3, _mesh, combiner.uv3s);
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV3(3, settings, textureBakeResults, _mesh, combiner.uv3s, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("uv3 flag was set in Apply but MeshBaker didn't generate uv3s"); }
                    }

                    if (uv4)
                    {
                        if (settings.doUV4)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                MBVersion.MeshAssignUVChannel(4, _mesh, combiner.uv4s);
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV4(4, settings, textureBakeResults, _mesh, combiner.uv4s, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("uv4 flag was set in Apply but MeshBaker didn't generate uv4s"); }
                    }

                    if (uv5)
                    {
                        if (settings.doUV5)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                MBVersion.MeshAssignUVChannel(5, _mesh, combiner.uv5s);
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV5(5, settings, textureBakeResults, _mesh, combiner.uv5s, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("uv5 flag was set in Apply but MeshBaker didn't generate uv5s"); }
                    }

                    if (uv6)
                    {
                        if (settings.doUV6)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                MBVersion.MeshAssignUVChannel(6, _mesh, combiner.uv6s);
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV6(6, settings, textureBakeResults, _mesh, combiner.uv6s, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("uv6 flag was set in Apply but MeshBaker didn't generate uv6s"); }
                    }

                    if (uv7)
                    {
                        if (settings.doUV7)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                MBVersion.MeshAssignUVChannel(7, _mesh, combiner.uv7s);
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV7(7, settings, textureBakeResults, _mesh, combiner.uv7s, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("uv7 flag was set in Apply but MeshBaker didn't generate uv7s"); }
                    }

                    if (uv8)
                    {
                        if (settings.doUV8)
                        {
                            if (settings.assignToMeshCustomizer == null)
                            {
                                MBVersion.MeshAssignUVChannel(8, _mesh, combiner.uv8s);
                            }
                            else
                            {
                                settings.assignToMeshCustomizer.meshAssign_UV8(8, settings, textureBakeResults, _mesh, combiner.uv8s, combiner.uvsSliceIdx);
                            }
                        }
                        else { Debug.LogError("uv8 flag was set in Apply but MeshBaker didn't generate uv8s"); }
                    }

                    bool do_generate_new_UV2_layout = false;
                    if (settings.renderType != MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
                    {
                        if (uv2GenerationMethod != null)
                        {
                            uv2GenerationMethod(_mesh, settings.uv2UnwrappingParamsHardAngle, settings.uv2UnwrappingParamsPackMargin);
                            if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("generating new UV2 layout for the combined mesh ");
                        }
                        else
                        {
                            Debug.LogError("No GenerateUV2Delegate method was supplied. UV2 cannot be generated.");
                        }
                        do_generate_new_UV2_layout = true;
                    }
                    else if (settings.renderType == MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("UV2 cannot be generated for SkinnedMeshRenderer objects.");
                    }
                    if (settings.renderType != MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout && do_generate_new_UV2_layout == false)
                    {
                        Debug.LogError("Failed to generate new UV2 layout. Only works in editor.");
                    }

                    if (bones)
                    {
                        _mesh.bindposes = combiner.bindPoses;
                        _mesh.boneWeights = combiner.boneWeights;
                    }
                    if (blendShapesFlag)
                    {
                        if (settings.smrMergeBlendShapesWithSameNames)
                        {
                            ApplyBlendShapeFramesToMeshAndBuildMap_MergeBlendShapesWithTheSameName(combiner);
                        }
                        else
                        {
                            ApplyBlendShapeFramesToMeshAndBuildMap(combiner);
                        }
                    }
                    if (triangles || vertices)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("recalculating bounds on mesh.");
                        _mesh.RecalculateBounds();
                    }
                    if (settings.optimizeAfterBake && !Application.isPlaying)
                    {
                        MBVersion.OptimizeMesh(_mesh);
                    }

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        if (verts.Length == 0)
                        {
                            //disable mesh renderer to avoid skinning warning
                            targetRenderer.enabled = false;
                        }
                        else
                        {
                            targetRenderer.enabled = true;
                        }

                        //needed so that updating local bounds will take affect
                        bool uwos = ((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen;
                        ((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen = true;
                        ((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen = uwos;

                        // Needed because it appears that the SkinnedMeshRenderer caches stuff when the mesh is assigned.
                        // It updates its cache on assignment. In 2019.4.28+ it appears that a check was added so that if the same mesh is assigned to the SMR then the update is skipped. 
                        // Generates errors (and mesh is invisible): d3d11: buffer size can not be zero
                        ((SkinnedMeshRenderer)targetRenderer).sharedMesh = null;
                        ((SkinnedMeshRenderer)targetRenderer).sharedMesh = _mesh;
                    }

                }
                else
                {
                    Debug.LogError("Need to add objects to this meshbaker before calling Apply or ApplyAll");
                }
                if (LOG_LEVEL >= MB2_LogLevel.debug)
                {
                    Debug.Log("Apply Complete time: " + sw.ElapsedMilliseconds + " vertices: " + _mesh.vertexCount);
                }
            }

            public static void ApplyShowHide(MB3_MeshCombinerSingle combiner)
            {
                MB_IMeshBakerSettings settings = combiner.settings;
                Mesh _mesh = combiner._mesh;
                Renderer targetRenderer = combiner.targetRenderer;
                if (_mesh != null)
                {
                    if (settings.renderType == MB_RenderType.meshRenderer)
                    {
                        //for MeshRenderer meshes this is needed for adding. It breaks skinnedMeshRenderers
                        MBVersion.MeshClear(_mesh, true);
                        _mesh.vertices = combiner.verts;
                    }
                    SerializableIntArray[] submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(combiner);
                    if (combiner.textureBakeResults.doMultiMaterial)
                    {
                        //submeshes with zero length tris cause error messages. must exclude these
                        int numNonZero = _mesh.subMeshCount = _numNonZeroLengthSubmeshTris(submeshTrisToUse);// submeshTrisToUse.Length;
                        int submeshIdx = 0;
                        for (int i = 0; i < submeshTrisToUse.Length; i++)
                        {
                            if (submeshTrisToUse[i].data.Length != 0)
                            {
                                _mesh.SetTriangles(submeshTrisToUse[i].data, submeshIdx);
                                submeshIdx++;
                            }
                        }
                        _updateMaterialsOnTargetRenderer(combiner.textureBakeResults, combiner.targetRenderer, submeshTrisToUse, numNonZero);
                    }
                    else
                    {
                        _mesh.triangles = submeshTrisToUse[0].data;
                    }

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        if (combiner.verts.Length == 0)
                        {
                            //disable mesh renderer to avoid skinning warning
                            targetRenderer.enabled = false;
                        }
                        else
                        {
                            targetRenderer.enabled = true;
                        }
                        //needed so that updating local bounds will take affect
                        bool uwos = ((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen;
                        ((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen = true;
                        ((SkinnedMeshRenderer)targetRenderer).updateWhenOffscreen = uwos;

                        ((SkinnedMeshRenderer)targetRenderer).sharedMesh = null;
                        ((SkinnedMeshRenderer)targetRenderer).sharedMesh = _mesh;
                    }
                    if (combiner.LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("ApplyShowHide");
                }
                else
                {
                    Debug.LogError("Need to add objects to this meshbaker before calling ApplyShowHide");
                }
            }
        }

        static int _numNonZeroLengthSubmeshTris(SerializableIntArray[] subTris)
        {
            int num = 0;
            for (int i = 0; i < subTris.Length; i++) { if (subTris[i].data.Length > 0) num++; }
            return num;
        }

        private static void _updateMaterialsOnTargetRenderer(MB2_TextureBakeResults textureBakeResults, Renderer targetRenderer, SerializableIntArray[] subTris, int numNonZeroLengthSubmeshTris)
        {
            //zero length triangle arrays in mesh cause errors. have excluded these sumbeshes so must exclude these materials
            if (subTris.Length != textureBakeResults.NumResultMaterials()) Debug.LogError("Mismatch between number of submeshes and number of result materials");
            Material[] resMats = new Material[numNonZeroLengthSubmeshTris];
            int submeshIdx = 0;
            for (int i = 0; i < subTris.Length; i++)
            {
                if (subTris[i].data.Length > 0)
                {
                    resMats[submeshIdx] = textureBakeResults.GetCombinedMaterialForSubmesh(i);
                    submeshIdx++;
                }
            }
            targetRenderer.materials = resMats;
        }

        public static SerializableIntArray[] GetSubmeshTrisWithShowHideApplied(MB3_MeshCombinerSingle combiner)
        {
            SerializableIntArray[] submeshTris = combiner.submeshTris;
            List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = combiner.mbDynamicObjectsInCombinedMesh;
            bool containsHiddenObjects = false;
            for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
            {
                if (mbDynamicObjectsInCombinedMesh[i].show == false)
                {
                    containsHiddenObjects = true;
                    break;
                }
            }
            if (containsHiddenObjects)
            {
                int[] newLengths = new int[submeshTris.Length];
                SerializableIntArray[] newSubmeshTris = new SerializableIntArray[submeshTris.Length];
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    if (dgo.show)
                    {
                        for (int j = 0; j < dgo.submeshNumTris.Length; j++)
                        {
                            newLengths[j] += dgo.submeshNumTris[j];
                        }
                    }
                }
                for (int i = 0; i < newSubmeshTris.Length; i++)
                {
                    newSubmeshTris[i] = new SerializableIntArray(newLengths[i]);
                }
                int[] idx = new int[newSubmeshTris.Length];
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    if (dgo.show)
                    {
                        for (int j = 0; j < submeshTris.Length; j++)
                        { //for each submesh
                            int[] triIdxs = submeshTris[j].data;
                            int startIdx = dgo.submeshTriIdxs[j];
                            int endIdx = startIdx + dgo.submeshNumTris[j];
                            for (int k = startIdx; k < endIdx; k++)
                            {
                                newSubmeshTris[j].data[idx[j]] = triIdxs[k];
                                idx[j] = idx[j] + 1;
                            }
                        }
                    }
                }
                return newSubmeshTris;
            }
            else
            {
                return submeshTris;
            }
        }
    }
}
