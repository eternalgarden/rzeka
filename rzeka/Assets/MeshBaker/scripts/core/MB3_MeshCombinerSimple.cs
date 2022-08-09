using UnityEngine;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;

namespace DigitalOpus.MB.Core
{
    /// <summary>
    /// Manages a single combined mesh.This class is the core of the mesh combining API.
    /// 
    /// It is not a component so it can be can be instantiated and used like a normal c sharp class.
    /// </summary>
    [System.Serializable]
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
        public override MB2_TextureBakeResults textureBakeResults
        {
            set
            {
                if (mbDynamicObjectsInCombinedMesh.Count > 0 && _textureBakeResults != value && _textureBakeResults != null)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("If Texture Bake Result is changed then objects currently in combined mesh may be invalid.");
                }
                _textureBakeResults = value;
            }
        }

        public override MB_RenderType renderType
        {
            set
            {
                if (value == MB_RenderType.skinnedMeshRenderer && _renderType == MB_RenderType.meshRenderer)
                {
                    if (boneWeights.Length != verts.Length) Debug.LogError("Can't set the render type to SkinnedMeshRenderer without clearing the mesh first. Try deleteing the CombinedMesh scene object.");
                }
                _renderType = value;
            }
        }

        public override GameObject resultSceneObject
        {
            set
            {
                if (_resultSceneObject != value && _resultSceneObject != null)
                {
                    _targetRenderer = null;
                    if (_mesh != null && LOG_LEVEL >= MB2_LogLevel.warn)
                    {
                        Debug.LogWarning("Result Scene Object was changed when this mesh baker component had a reference to a mesh. If mesh is being used by another object make sure to reset the mesh to none before baking to avoid overwriting the other mesh.");
                    }
                }
                _resultSceneObject = value;
            }
        }

        //this contains object instances that have been added to the combined mesh through AddDelete
        [SerializeField]
        protected List<GameObject> objectsInCombinedMesh = new List<GameObject>();

        [SerializeField]
        int lightmapIndex = -1;

        [SerializeField]
        List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = new List<MB_DynamicGameObject>();
        Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map = new Dictionary<GameObject, MB_DynamicGameObject>();

        [SerializeField]
        Vector3[] verts = new Vector3[0];
        [SerializeField]
        Vector3[] normals = new Vector3[0];
        [SerializeField]
        Vector4[] tangents = new Vector4[0];
        [SerializeField]
        Vector2[] uvs = new Vector2[0];
        [SerializeField]
        float[] uvsSliceIdx = new float[0];
        [SerializeField]
        Vector2[] uv2s = new Vector2[0];
        [SerializeField]
        Vector2[] uv3s = new Vector2[0];
        [SerializeField]
        Vector2[] uv4s = new Vector2[0];

        [SerializeField]
        Vector2[] uv5s = new Vector2[0];
        [SerializeField]
        Vector2[] uv6s = new Vector2[0];
        [SerializeField]
        Vector2[] uv7s = new Vector2[0];
        [SerializeField]
        Vector2[] uv8s = new Vector2[0];

        [SerializeField]
        Color[] colors = new Color[0];
        [SerializeField]
        Matrix4x4[] bindPoses = new Matrix4x4[0];
        [SerializeField]
        Transform[] bones = new Transform[0];
        [SerializeField]
        internal MBBlendShape[] blendShapes = new MBBlendShape[0];
        [SerializeField]
        //these blend shapes are not cleared they are used to build the src to combined blend shape map
        internal MBBlendShape[] blendShapesInCombined = new MBBlendShape[0];

        [SerializeField]
        SerializableIntArray[] submeshTris = new SerializableIntArray[0];

        [SerializeField]
        MeshCreationConditions _meshBirth = MeshCreationConditions.NoMesh;

        [SerializeField]
        Mesh _mesh;

        //unity won't serialize these
        BoneWeight[] boneWeights = new BoneWeight[0];

        //used if user passes null in as parameter to AddOrDelete
        GameObject[] empty = new GameObject[0];
        int[] emptyIDs = new int[0];
        
        MB_DynamicGameObject instance2Combined_MapGet(GameObject gameObjectID)
        {
            return _instance2combined_map[gameObjectID];
        }

        void instance2Combined_MapAdd(GameObject gameObjectID, MB_DynamicGameObject dgo)
        {
            _instance2combined_map.Add(gameObjectID, dgo);
        }

        void instance2Combined_MapRemove(GameObject gameObjectID)
        {
            _instance2combined_map.Remove(gameObjectID);
        }

        bool instance2Combined_MapTryGetValue(GameObject gameObjectID, out MB_DynamicGameObject dgo)
        {
            return _instance2combined_map.TryGetValue(gameObjectID, out dgo);
        }

        int instance2Combined_MapCount()
        {
            return _instance2combined_map.Count;
        }

        void instance2Combined_MapClear()
        {
            _instance2combined_map.Clear();
        }

        bool instance2Combined_MapContainsKey(GameObject gameObjectID)
        {
            return _instance2combined_map.ContainsKey(gameObjectID);
        }

        bool InstanceID2DGO(int instanceID, out MB_DynamicGameObject dgoGameObject)
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

        public override int GetNumObjectsInCombined()
        {
            return mbDynamicObjectsInCombinedMesh.Count;
        }

        public override List<GameObject> GetObjectsInCombined()
        {
            List<GameObject> outObs = new List<GameObject>();
            outObs.AddRange(objectsInCombinedMesh);
            return outObs;
        }

        public Mesh GetMesh()
        {
            if (_mesh == null)
            {
                _mesh = NewMesh();
            }
            return _mesh;
        }

        public void SetMesh(Mesh m)
        {
            if (m == null)
            {
                _meshBirth = MeshCreationConditions.AssignedByUser;
            }
            else
            {
                _meshBirth = MeshCreationConditions.NoMesh;
            }
           
            _mesh = m;
        }

        public Transform[] GetBones()
        {
            return bones;
        }

        public override int GetLightmapIndex()
        {
            if (settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout || settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
            {
                return lightmapIndex;
            }
            else {
                return -1;
            }
        }

        public override int GetNumVerticesFor(GameObject go)
        {
            return GetNumVerticesFor(go.GetInstanceID());
        }

        public override int GetNumVerticesFor(int instanceID)
        {
            MB_DynamicGameObject dgo = null;
            InstanceID2DGO(instanceID, out dgo);
            if (dgo != null)
            {
                return dgo.numVerts;
            }
            else {
                return -1;
            }
        }

        bool _Initialize(int numResultMats)
        {
            if (mbDynamicObjectsInCombinedMesh.Count == 0)
            {
                lightmapIndex = -1;
            }

            if (_mesh == null)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("_initialize Creating new Mesh");
                _mesh = GetMesh();
            }

            if (instance2Combined_MapCount() != mbDynamicObjectsInCombinedMesh.Count)
            {
                //build the instance2Combined map
                instance2Combined_MapClear();
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    if (mbDynamicObjectsInCombinedMesh[i] != null)
                    {
                        if (mbDynamicObjectsInCombinedMesh[i].gameObject == null)
                        {
                            Debug.LogError("This MeshBaker contains information from a previous bake that is incomlete. It may have been baked by a previous version of Mesh Baker. If you are trying to update/modify a previously baked combined mesh. Try doing the original bake.");
                            return false;
                        }

                        instance2Combined_MapAdd(mbDynamicObjectsInCombinedMesh[i].gameObject, mbDynamicObjectsInCombinedMesh[i]);
                    }
                }
                //BoneWeights are not serialized get from combined mesh
                boneWeights = _mesh.boneWeights;
            }

            if (objectsInCombinedMesh.Count == 0)
            {
                if (submeshTris.Length != numResultMats)
                {
                    submeshTris = new SerializableIntArray[numResultMats];
                    for (int i = 0; i < submeshTris.Length; i++) submeshTris[i] = new SerializableIntArray(0);
                }
            }

            //MeshBaker was baked using old system that had duplicated bones. Upgrade to new system
            //need to build indexesOfBonesUsed maps for dgos
            if (mbDynamicObjectsInCombinedMesh.Count > 0 &&
                mbDynamicObjectsInCombinedMesh[0].indexesOfBonesUsed.Length == 0 &&
                settings.renderType == MB_RenderType.skinnedMeshRenderer &&
                boneWeights.Length > 0)
            {

                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    HashSet<int> idxsOfBonesUsed = new HashSet<int>();
                    for (int j = dgo.vertIdx; j < dgo.vertIdx + dgo.numVerts; j++)
                    {
                        if (boneWeights[j].weight0 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex0);
                        if (boneWeights[j].weight1 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex1);
                        if (boneWeights[j].weight2 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex2);
                        if (boneWeights[j].weight3 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex3);
                    }
                    dgo.indexesOfBonesUsed = new int[idxsOfBonesUsed.Count];
                    idxsOfBonesUsed.CopyTo(dgo.indexesOfBonesUsed);
                }
                if (LOG_LEVEL >= MB2_LogLevel.debug)
                    Debug.Log("Baker used old systems that duplicated bones. Upgrading to new system by building indexesOfBonesUsed");
            }
			if (LOG_LEVEL >= MB2_LogLevel.trace) {
				Debug.Log (String.Format ("_initialize numObjsInCombined={0}", mbDynamicObjectsInCombinedMesh.Count));
			}

            return true;
        }

        bool _collectMaterialTriangles(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map)
        {
            //everything here applies to the source object being added
            int numTriMeshes = m.subMeshCount;
            if (sharedMaterials.Length < numTriMeshes) numTriMeshes = sharedMaterials.Length;
            dgo._tmpSubmeshTris = new SerializableIntArray[numTriMeshes];
            dgo.targetSubmeshIdxs = new int[numTriMeshes];
            for (int i = 0; i < numTriMeshes; i++)
            {
                if (_textureBakeResults.doMultiMaterial || _textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray)
                {
                    if (!sourceMats2submeshIdx_map.Contains(sharedMaterials[i]))
                    {
                        Debug.LogError("Object " + dgo.name + " has a material that was not found in the result materials maping. " + sharedMaterials[i]);
                        return false;
                    }
                    dgo.targetSubmeshIdxs[i] = (int)sourceMats2submeshIdx_map[sharedMaterials[i]];
                }
                else {
                    dgo.targetSubmeshIdxs[i] = 0;
                }
                dgo._tmpSubmeshTris[i] = new SerializableIntArray();
                dgo._tmpSubmeshTris[i].data = m.GetTriangles(i);
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Collecting triangles for: " + dgo.name + " submesh:" + i + " maps to submesh:" + dgo.targetSubmeshIdxs[i] + " added:" + dgo._tmpSubmeshTris[i].data.Length, LOG_LEVEL);
            }
            return true;
        }

        // if adding many copies of the same mesh want to cache obUVsResults
        bool _collectOutOfBoundsUVRects2(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResults, MeshChannelsCache meshChannelCache)
        {
            if (_textureBakeResults == null)
            {
                Debug.LogError("Need to bake textures into combined material");
                return false;
            }

            MB_Utility.MeshAnalysisResult[] res;
            if (!meshAnalysisResults.TryGetValue(m.GetInstanceID(), out res))
            {
                // Process the mesh and cache the result.
                int numSrcSubMeshes = m.subMeshCount;
                res = new MB_Utility.MeshAnalysisResult[numSrcSubMeshes];
                Vector2[] uvs = meshChannelCache.GetUv0Raw(m);
                for (int submeshIdx = 0; submeshIdx < numSrcSubMeshes; submeshIdx++)
                {
                    MB_Utility.hasOutOfBoundsUVs(uvs, m, ref res[submeshIdx], submeshIdx);
                }

                meshAnalysisResults.Add(m.GetInstanceID(), res);
            }
            
            int numUsedSrcSubMeshes = sharedMaterials.Length;
            if (numUsedSrcSubMeshes > m.subMeshCount) numUsedSrcSubMeshes = m.subMeshCount;
            dgo.obUVRects = new Rect[numUsedSrcSubMeshes];
            
            // We might have fewer sharedMaterials than submeshes in the mesh.
            for (int submeshIdx = 0; submeshIdx < numUsedSrcSubMeshes; submeshIdx++)
            {
                int idxInResultMats = dgo.targetSubmeshIdxs[submeshIdx];
                if (_textureBakeResults.GetConsiderMeshUVs(idxInResultMats, sharedMaterials[submeshIdx]))
                {
                    dgo.obUVRects[submeshIdx] = res[submeshIdx].uvRect;
                }
            }

            return true;
        }

        bool _validateTextureBakeResults()
        {
            if (_textureBakeResults == null)
            {
                Debug.LogError("Texture Bake Results is null. Can't combine meshes.");
                return false;
            }
            if (_textureBakeResults.materialsAndUVRects == null || _textureBakeResults.materialsAndUVRects.Length == 0)
            {
                Debug.LogError("Texture Bake Results has no materials in material to sourceUVRect map. Try baking materials. Can't combine meshes. " +
                    "If you are trying to combine meshes without combining materials, try removing the Texture Bake Result.");
                return false;
            }

            if (_textureBakeResults.NumResultMaterials() == 0)
            {
                Debug.LogError("Texture Bake Results has no result materials. Try baking materials. Can't combine meshes.");
                return false;
            }

            if (settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray)
            {
                if (uvs.Length != uvsSliceIdx.Length)
                {
                    Debug.LogError("uvs buffer and sliceIdx buffer are different sizes. Did you switch texture bake result from atlas to texture array result?");
                    return false;
                }
            }

            return true;
        }

        /*
        bool _validateMeshFlags()
        {
            if (mbDynamicObjectsInCombinedMesh.Count > 0)
            {
                if (settings.doNorm == false && doNorm == true ||
                    settings.doTan == false && doTan == true ||
                    settings.doCol == false && doCol == true ||
                    settings.doUV == false && doUV == true ||
                    settings.doUV3 == false && doUV3 == true ||
                    settings.doUV4 == false && doUV4 == true)
                {
                    Debug.LogError("The channels have changed. There are already objects in the combined mesh that were added with a different set of channels.");
                    return false;
                }
            }
            settings.doNorm = doNorm;
            settings.doTan = doTan;
            settings.doCol = doCol;
            settings.doUV = doUV;
            settings.doUV3 = doUV3;
            settings.doUV4 = doUV4;
            return true;
        }
        */

        bool _showHide(GameObject[] goToShow, GameObject[] goToHide)
        {
            if (goToShow == null) goToShow = empty;
            if (goToHide == null) goToHide = empty;
            //calculate amount to hide
            int numResultMats = _textureBakeResults.NumResultMaterials();
            if (!_Initialize(numResultMats))
            {
                return false;
            }

            for (int i = 0; i < goToHide.Length; i++)
            {
                if (!instance2Combined_MapContainsKey(goToHide[i]))
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Trying to hide an object " + goToHide[i] + " that is not in combined mesh. Did you initially bake with 'clear buffers after bake' enabled?");
                    return false;
                }
            }

            //now to show
            for (int i = 0; i < goToShow.Length; i++)
            {
                if (!instance2Combined_MapContainsKey(goToShow[i]))
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Trying to show an object " + goToShow[i] + " that is not in combined mesh. Did you initially bake with 'clear buffers after bake' enabled?");
                    return false;
                }
            }

            //set flags
            for (int i = 0; i < goToHide.Length; i++) _instance2combined_map[goToHide[i]].show = false;
            for (int i = 0; i < goToShow.Length; i++) _instance2combined_map[goToShow[i]].show = true;

            return true;
        }

        bool _addToCombined(GameObject[] goToAdd, int[] goToDelete, bool disableRendererInSource)
        {
            System.Diagnostics.Stopwatch sw = null;
            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                sw = new System.Diagnostics.Stopwatch();
                sw.Start();
            }
            GameObject[] _goToAdd;
            int[] _goToDelete;
            if (!_validateTextureBakeResults()) return false;
            if (!ValidateTargRendererAndMeshAndResultSceneObj()) return false;

            if (outputOption != MB2_OutputOptions.bakeMeshAssetsInPlace &&
                settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                if (_targetRenderer == null || !(_targetRenderer is SkinnedMeshRenderer))
                {
                    Debug.LogError("Target renderer must be set and must be a SkinnedMeshRenderer");
                    return false;
                }
            }
            if (settings.doBlendShapes && settings.renderType != MB_RenderType.skinnedMeshRenderer)
            {
                Debug.LogError("If doBlendShapes is set then RenderType must be skinnedMeshRenderer.");
                return false;
            }
            if (goToAdd == null) _goToAdd = empty;
            else _goToAdd = (GameObject[])goToAdd.Clone();
            if (goToDelete == null) _goToDelete = emptyIDs;
            else _goToDelete = (int[])goToDelete.Clone();
            if (_mesh == null) DestroyMesh(); //cleanup maps and arrays

            //MB2_TextureBakeResults.Material2AtlasRectangleMapper mat2rect_map = new MB2_TextureBakeResults.Material2AtlasRectangleMapper(textureBakeResults);
            UVAdjuster_Atlas uvAdjuster = new UVAdjuster_Atlas(textureBakeResults, LOG_LEVEL);

            int numResultMats = _textureBakeResults.NumResultMaterials();
            if (!_Initialize(numResultMats))
            {
                return false;
            }

            if (submeshTris.Length != numResultMats)
            {
                Debug.LogError("The number of submeshes " + submeshTris.Length + " in the combined mesh was not equal to the number of result materials " + numResultMats + " in the Texture Bake Result");
                return false;
            }

            if (_mesh.vertexCount > 0 && _instance2combined_map.Count == 0)
            {
                Debug.LogWarning("There were vertices in the combined mesh but nothing in the MeshBaker buffers. If you are trying to bake in the editor and modify at runtime, make sure 'Clear Buffers After Bake' is unchecked.");
            }

            if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("==== Calling _addToCombined objs adding:" + _goToAdd.Length + " objs deleting:" + _goToDelete.Length + " fixOutOfBounds:" + textureBakeResults.DoAnyResultMatsUseConsiderMeshUVs() + " doMultiMaterial:" + textureBakeResults.doMultiMaterial + " disableRenderersInSource:" + disableRendererInSource, LOG_LEVEL);

            //backward compatibility set up resultMaterials if it is blank
            if (_textureBakeResults.NumResultMaterials() == 0)
            {
                Debug.LogError("No resultMaterials in this TextureBakeResults. Try baking textures.");
                return false;
            }

            OrderedDictionary sourceMats2submeshIdx_map = BuildSourceMatsToSubmeshIdxMap(numResultMats);
            if (sourceMats2submeshIdx_map == null)
            {
                return false;
            }

            //STEP 1 update our internal description of objects being added and deleted keep track of changes to buffer sizes as we do.
            //calculate amount to delete
            int totalDeleteVerts = 0;
            int[] totalDeleteSubmeshTris = new int[numResultMats];
            int totalDeleteBlendShapes = 0;

            //in order to decide if a bone can be deleted need to know which dgos use it so build a map
            MB3_MeshCombinerSimpleBones boneProcessor = new MB3_MeshCombinerSimpleBones(this);
            boneProcessor.BuildBoneIdx2DGOMapIfNecessary(_goToDelete);
            for (int i = 0; i < _goToDelete.Length; i++)
            {
                MB_DynamicGameObject dgo = null;
                InstanceID2DGO(_goToDelete[i], out dgo);
                if (dgo != null)
                {
                    totalDeleteVerts += dgo.numVerts;
                    totalDeleteBlendShapes += dgo.numBlendShapes;
                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        boneProcessor.FindBonesToDelete(dgo);
                    }
                    for (int j = 0; j < dgo.submeshNumTris.Length; j++)
                    {
                        totalDeleteSubmeshTris[j] += dgo.submeshNumTris[j];
                    }
                }
                else {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Trying to delete an object that is not in combined mesh");
                }
            }

            //now add
            List<MB_DynamicGameObject> toAddDGOs = new List<MB_DynamicGameObject>();
            Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>(); //cache results

            //we are often adding the same sharedMesh many times. Only want to grab the results once and cache them
            MeshChannelsCache meshChannelCache = new MeshChannelsCache(LOG_LEVEL, settings.lightmapOption);

            int totalAddVerts = 0;
            int[] totalAddSubmeshTris = new int[numResultMats];
            int totalAddBlendShapes = 0;

            for (int i = 0; i < _goToAdd.Length; i++)
            {
                // if not already in mesh or we are deleting and re-adding in same operation
                if (!instance2Combined_MapContainsKey(_goToAdd[i]) || Array.FindIndex<int>(_goToDelete, o => o == _goToAdd[i].GetInstanceID()) != -1)
                {
                    MB_DynamicGameObject dgo = new MB_DynamicGameObject();

                    GameObject go = _goToAdd[i];
                    
                    Material[] sharedMaterials = MB_Utility.GetGOMaterials(go);
                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log(String.Format("Getting {0} shared materials for {1}",sharedMaterials.Length, go));
                    if (sharedMaterials == null)
                    {
                        Debug.LogError("Object " + go.name + " does not have a Renderer");
                        _goToAdd[i] = null;
                        return false;
                    }

                    Mesh m = MB_Utility.GetMesh(go);
                    if (sharedMaterials.Length > m.subMeshCount)
                    {
                        // The extra materials do nothing but could cause bugs.
                        Array.Resize(ref sharedMaterials, m.subMeshCount);
                    }

                    if (m == null)
                    {
                        Debug.LogError("Object " + go.name + " MeshFilter or SkinedMeshRenderer had no mesh");
                        _goToAdd[i] = null;
                        return false;
                    }
                    else if (MBVersion.IsRunningAndMeshNotReadWriteable(m))
                    {
                        Debug.LogError("Object " + go.name + " Mesh Importer has read/write flag set to 'false'. This needs to be set to 'true' in order to read data from this mesh.");
                        _goToAdd[i] = null;
                        return false;
                    }

                    if (!uvAdjuster.MapSharedMaterialsToAtlasRects(sharedMaterials, false, m, meshChannelCache, meshAnalysisResultsCache, sourceMats2submeshIdx_map, go, dgo))
                    {
                        _goToAdd[i] = null;
                        return false;
                    }

                    if (_goToAdd[i] != null)
                    {
                        toAddDGOs.Add(dgo);
                        dgo.name = String.Format("{0} {1}", _goToAdd[i].ToString(), _goToAdd[i].GetInstanceID());
                        dgo.instanceID = _goToAdd[i].GetInstanceID();
                        dgo.gameObject = _goToAdd[i];
                        dgo.numVerts = m.vertexCount;
                        
                        if (settings.doBlendShapes)
                        {
                            dgo.numBlendShapes = m.blendShapeCount;
                        }
                        Renderer r = MB_Utility.GetRenderer(go);
                        if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                        {
                            if (!boneProcessor.CollectBonesToAddForDGO(dgo, r, settings.smrNoExtraBonesWhenCombiningMeshRenderers, meshChannelCache))
                            {
                                Debug.LogError("Object " + go.name + " could not collect bones.");
                                _goToAdd[i] = null;
                                return false;
                            }
                        }
                        if (lightmapIndex == -1)
                        {
                            lightmapIndex = r.lightmapIndex; //initialize	
                        }
                        if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
                        {
                            if (lightmapIndex != r.lightmapIndex)
                            {
                                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + go.name + " has a different lightmap index. Lightmapping will not work.");
                            }
                            if (!MBVersion.GetActive(go))
                            {
                                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + go.name + " is inactive. Can only get lightmap index of active objects.");
                            }
                            if (r.lightmapIndex == -1)
                            {
                                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + go.name + " does not have an index to a lightmap.");
                            }
                        }
                        dgo.lightmapIndex = r.lightmapIndex;
                        dgo.lightmapTilingOffset = MBVersion.GetLightmapTilingOffset(r);
                        if (!_collectMaterialTriangles(m, dgo, sharedMaterials, sourceMats2submeshIdx_map))
                        {
                            return false;
                        }
                        dgo.meshSize = r.bounds.size;
                        dgo.submeshNumTris = new int[numResultMats];
                        dgo.submeshTriIdxs = new int[numResultMats];
                        dgo.sourceSharedMaterials = sharedMaterials;

                        bool doAnyResultsUseConsiderMeshUVs = textureBakeResults.DoAnyResultMatsUseConsiderMeshUVs();
                        if (doAnyResultsUseConsiderMeshUVs)
                        {
                            if (!_collectOutOfBoundsUVRects2(m, dgo, sharedMaterials, sourceMats2submeshIdx_map, meshAnalysisResultsCache, meshChannelCache))
                            {
                                return false;
                            }
                        }

                        totalAddVerts += dgo.numVerts;
                        totalAddBlendShapes += dgo.numBlendShapes;
                        for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++)
                        {
                            totalAddSubmeshTris[dgo.targetSubmeshIdxs[j]] += dgo._tmpSubmeshTris[j].data.Length;
                        }

                        dgo.invertTriangles = IsMirrored(go.transform.localToWorldMatrix);

                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.uvRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceSharedMaterials.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.encapsulatingRect.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceMaterialTiling.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        if (doAnyResultsUseConsiderMeshUVs) Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.obUVRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    }
                }
                else {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + _goToAdd[i].name + " has already been added");
                    _goToAdd[i] = null;
                }
            }

            for (int i = 0; i < _goToAdd.Length; i++)
            {
                if (_goToAdd[i] != null && disableRendererInSource)
                {
                    MB_Utility.DisableRendererInSource(_goToAdd[i]);
                    if (LOG_LEVEL == MB2_LogLevel.trace) Debug.Log("Disabling renderer on " + _goToAdd[i].name + " id=" + _goToAdd[i].GetInstanceID());
                }
            }

            // STEP 2 to allocate new buffers and copy everything over
            return MB3_MeshCombinerSimpleSubCombinerLegacy._addToCombined(this, totalAddVerts, totalDeleteVerts, boneProcessor, numResultMats, totalAddBlendShapes, totalDeleteBlendShapes, totalAddSubmeshTris, totalDeleteSubmeshTris,
                _goToDelete, toAddDGOs, _goToAdd, meshChannelCache, uvAdjuster, sw);
        }

        Transform[] _getBones(Renderer r, bool isSkinnedMeshWithBones)
        {
            return MBVersion.GetBones(r, isSkinnedMeshWithBones);
        }

        public override void Apply(GenerateUV2Delegate uv2GenerationMethod)
        {
            MB3_MeshCombinerSimpleSubCombinerLegacy.Apply(this, uv2GenerationMethod);
            return;
            /*
            bool doBones = false;
            if (settings.renderType == MB_RenderType.skinnedMeshRenderer) doBones = true;
            Apply(true, true, settings.doNorm, settings.doTan, 
                settings.doUV, doUV2(), settings.doUV3, settings.doUV4, settings.doUV5, settings.doUV6, settings.doUV7, settings.doUV8,
                settings.doCol, doBones, settings.doBlendShapes, uv2GenerationMethod);
            */
        }

        public virtual void ApplyShowHide()
        {
            if (_validationLevel >= MB2_ValidationLevel.quick && !ValidateTargRendererAndMeshAndResultSceneObj()) return;
            MB3_MeshCombinerSimpleSubCombinerLegacy.ApplyShowHide(this);
        }

        public override void Apply(bool triangles,
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

                MB3_MeshCombinerSimpleSubCombinerLegacy.Apply(this, triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, colors, bones, blendShapesFlag, uv2GenerationMethod);
                return;
            /*
            Apply(triangles, vertices, normals, tangents,
                uvs, uv2, uv3, uv4,
                false, false, false, false,
                colors, bones, blendShapesFlag, uv2GenerationMethod);
            */
        }

        public override void Apply(bool triangles,
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
            MB3_MeshCombinerSimpleSubCombinerLegacy.Apply(this, triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, uv5, uv6, uv7, uv8, colors, bones, blendShapesFlag, uv2GenerationMethod);
            return;
        }

        public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds,
                                        bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4,
                                        bool updateColors, bool updateSkinningInfo)
        {
            return _updateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4,
                                        false, false, false, false, updateColors, updateSkinningInfo);
        }

        public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds,
                                        bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4,
                                        bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                        bool updateColors, bool updateSkinningInfo)
        {
            return _updateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4,
                                        updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo);
        }

        bool _updateGameObjects(GameObject[] gos, bool recalcBounds,
                                        bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                        bool updateColors, bool updateSkinningInfo)
        {
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("UpdateGameObjects called on " + gos.Length + " objects.");
            int numResultMats = 1;
            if (textureBakeResults.doMultiMaterial) numResultMats = textureBakeResults.NumResultMaterials();
            
            if (!_Initialize(numResultMats))
            {
                return false;
            }
            
            if (_mesh.vertexCount > 0 && _instance2combined_map.Count == 0)
            {
                Debug.LogWarning("There were vertices in the combined mesh but nothing in the MeshBaker buffers. If you are trying to bake in the editor and modify at runtime, make sure 'Clear Buffers After Bake' is unchecked.");
            }
            bool success = true;
            MeshChannelsCache meshChannelCache = new MeshChannelsCache(LOG_LEVEL, settings.lightmapOption);
            UVAdjuster_Atlas uvAdjuster = null;
            OrderedDictionary sourceMats2submeshIdx_map = null;
            Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache = null;
            if (updateUV){
                sourceMats2submeshIdx_map = BuildSourceMatsToSubmeshIdxMap(numResultMats);
                if (sourceMats2submeshIdx_map == null)
                {
                    return false;
                }

                uvAdjuster = new UVAdjuster_Atlas(textureBakeResults, LOG_LEVEL);
                meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
            }

            for (int i = 0; i < gos.Length; i++)
            {
                success = success && _updateGameObject(gos[i], updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo, 
                    meshChannelCache, meshAnalysisResultsCache, sourceMats2submeshIdx_map, uvAdjuster);
            }
            if (recalcBounds)
                _mesh.RecalculateBounds();
            return success;
        }

        bool _updateGameObject(GameObject go, bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                        bool updateColors, bool updateSkinningInfo, 
                                        MeshChannelsCache meshChannelCache, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache,
                                        OrderedDictionary sourceMats2submeshIdx_map, UVAdjuster_Atlas uVAdjuster)
        {
            return MB3_MeshCombinerSimpleSubCombinerLegacy._updateGameObject(this, _instance2combined_map, go, 
                                        updateVertices, updateNormals, updateTangents,
                                        updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8,
                                        updateColors, updateSkinningInfo,
                                        meshChannelCache, meshAnalysisResultsCache,
                                        sourceMats2submeshIdx_map, uVAdjuster, LOG_LEVEL);
        }

        public bool ShowHideGameObjects(GameObject[] toShow, GameObject[] toHide)
        {
            if (textureBakeResults == null)
            {
                Debug.LogError("TextureBakeResults must be set.");
                return false;
            }
            return _showHide(toShow, toHide);
        }

        public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true)
        {
            int[] delInstanceIDs = null;
            if (deleteGOs != null)
            {
                delInstanceIDs = new int[deleteGOs.Length];
                for (int i = 0; i < deleteGOs.Length; i++)
                {
                    if (deleteGOs[i] == null)
                    {
                        Debug.LogError("The " + i + "th object on the list of objects to delete is 'Null'");
                    }
                    else {
                        delInstanceIDs[i] = deleteGOs[i].GetInstanceID();
                    }
                }
            }
            return AddDeleteGameObjectsByID(gos, delInstanceIDs, disableRendererInSource);
        }

        public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
        {
            //			Profile.StartProfile("AddDeleteGameObjectsByID");
            if (validationLevel > MB2_ValidationLevel.none)
            {
                //check for duplicates
                if (gos != null)
                {
                    for (int i = 0; i < gos.Length; i++)
                    {
                        if (gos[i] == null)
                        {
                            Debug.LogError("The " + i + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
                            return false;
                        }
                        if (validationLevel >= MB2_ValidationLevel.robust)
                        {
                            for (int j = i + 1; j < gos.Length; j++)
                            {
                                if (gos[i] == gos[j])
                                {
                                    Debug.LogError("GameObject " + gos[i] + " appears twice in list of game objects to add");
                                    return false;
                                }
                            }
                        }
                    }
                }
                if (deleteGOinstanceIDs != null && validationLevel >= MB2_ValidationLevel.robust)
                {
                    for (int i = 0; i < deleteGOinstanceIDs.Length; i++)
                    {
                        for (int j = i + 1; j < deleteGOinstanceIDs.Length; j++)
                        {
                            if (deleteGOinstanceIDs[i] == deleteGOinstanceIDs[j])
                            {
                                Debug.LogError("GameObject " + deleteGOinstanceIDs[i] + "appears twice in list of game objects to delete");
                                return false;
                            }
                        }
                    }
                }
            }

            if (_usingTemporaryTextureBakeResult && gos != null && gos.Length > 0)
            {
                MB_Utility.Destroy(_textureBakeResults);
                _textureBakeResults = null;
                _usingTemporaryTextureBakeResult = false;
            }

            //create a temporary _textureBakeResults if needed 
            if (_textureBakeResults == null && gos != null && gos.Length > 0 && gos[0] != null)
            {
                if (!_CreateTemporaryTextrueBakeResult(gos, GetMaterialsOnTargetRenderer()))
                {
                    return false;
                }
            }

            BuildSceneMeshObject(gos);


            if (!_addToCombined(gos, deleteGOinstanceIDs, disableRendererInSource))
            {
                Debug.LogError("Failed to add/delete objects to combined mesh");
                return false;
            }
            if (targetRenderer != null)
            {
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    SkinnedMeshRenderer smr = (SkinnedMeshRenderer)targetRenderer;
                    smr.sharedMesh = _mesh;
                    smr.bones = bones;
                    UpdateSkinnedMeshApproximateBoundsFromBounds();
                }
                _SetLightmapIndexIfPreserveLightmapping(targetRenderer);               
            }
            //			Profile.EndProfile("AddDeleteGameObjectsByID");
            //			Profile.PrintResults();
            return true;
        }

       

        public override bool CombinedMeshContains(GameObject go)
        {
            return objectsInCombinedMesh.Contains(go);
        }

        public override void ClearBuffers()
        {
            verts = new Vector3[0];
            normals = new Vector3[0];
            tangents = new Vector4[0];
            uvs = new Vector2[0];
            uvsSliceIdx = new float[0];
            uv2s = new Vector2[0];
            uv3s = new Vector2[0];
            uv4s = new Vector2[0];
            uv5s = new Vector2[0];
            uv6s = new Vector2[0];
            uv7s = new Vector2[0];
            uv8s = new Vector2[0];
            colors = new Color[0];
            bones = new Transform[0];
            bindPoses = new Matrix4x4[0];
            boneWeights = new BoneWeight[0];
            submeshTris = new SerializableIntArray[0];
            blendShapes = new MBBlendShape[0];
            blendShapesInCombined = new MBBlendShape[0];
            mbDynamicObjectsInCombinedMesh.Clear();
            objectsInCombinedMesh.Clear();
            instance2Combined_MapClear();
            if (_usingTemporaryTextureBakeResult)
            {
                MB_Utility.Destroy(_textureBakeResults);
                _textureBakeResults = null;
                _usingTemporaryTextureBakeResult = false;
            }
            if (LOG_LEVEL >= MB2_LogLevel.trace) MB2_Log.LogDebug("ClearBuffers called");
        }

        private Mesh NewMesh()
        {
            if (Application.isPlaying)
            {
                _meshBirth = MeshCreationConditions.CreatedAtRuntime;
            } else {
                _meshBirth = MeshCreationConditions.CreatedInEditor;
            }
            Mesh m = new Mesh();
            
            return m;
        }

        /*
		 * Empties all channels and clears the mesh
		 */
        public override void ClearMesh()
        {
            if (_mesh != null)
            {
                MBVersion.MeshClear(_mesh, false);
            }
            else {
                _mesh = NewMesh();
            }
            ClearBuffers();
        }

        public override void ClearMesh(MB2_EditorMethodsInterface editorMethods)
        {
            ClearMesh();
        }

        public override void DisposeRuntimeCreated()
        {
            if (Application.isPlaying)
            {
                if (_meshBirth == MeshCreationConditions.CreatedAtRuntime)
                {
                    GameObject.Destroy(_mesh);
                }
                else if (_meshBirth == MeshCreationConditions.AssignedByUser)
                {
                    _mesh = null;
                }

                ClearBuffers();
            }
        }

        /// <summary>
        /// Empties all channels, destroys the mesh and replaces it with a new mesh
        /// </summary>
        public override void DestroyMesh()
        {
            if (_mesh != null)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Destroying Mesh");
                MB_Utility.Destroy(_mesh);
                _meshBirth = MeshCreationConditions.NoMesh;
            }

            ClearBuffers();
        }

        public override void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods)
        {
            if (_mesh != null && editorMethods != null && !Application.isPlaying)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Destroying Mesh");
                editorMethods.Destroy(_mesh);
            }

            ClearBuffers();
        }

        public bool ValidateTargRendererAndMeshAndResultSceneObj()
        {
            if (_resultSceneObject == null)
            {
                if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Result Scene Object was not set.");
                return false;
            }
            else {
                if (_targetRenderer == null)
                {
                    if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target Renderer was not set.");
                    return false;
                }
                else {
                    if (_targetRenderer.transform.parent != _resultSceneObject.transform)
                    {
                        if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target Renderer game object is not a child of Result Scene Object was not set.");
                        return false;
                    }
                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        if (!(_targetRenderer is SkinnedMeshRenderer))
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Render Type is skinned mesh renderer but Target Renderer is not.");
                            return false;
                        }
                        /*
                        if (((SkinnedMeshRenderer)_targetRenderer).sharedMesh != _mesh)
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target renderer mesh is not equal to mesh.");
                            return false;
                        }
                        */
                    }
                    if (settings.renderType == MB_RenderType.meshRenderer)
                    {
                        if (!(_targetRenderer is MeshRenderer))
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Render Type is mesh renderer but Target Renderer is not.");
                            return false;
                        }
                        MeshFilter mf = _targetRenderer.GetComponent<MeshFilter>();
                        if (_mesh != mf.sharedMesh)
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target renderer mesh is not equal to mesh.");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        OrderedDictionary BuildSourceMatsToSubmeshIdxMap(int numResultMats)
        {
            OrderedDictionary sourceMats2submeshIdx_map = new OrderedDictionary();
            //build the sourceMats to submesh index map
            for (int resultMatIdx = 0; resultMatIdx < numResultMats; resultMatIdx++)
            {
                List<Material> sourceMats = _textureBakeResults.GetSourceMaterialsUsedByResultMaterial(resultMatIdx);
                for (int j = 0; j < sourceMats.Count; j++)
                {
                    if (sourceMats[j] == null)
                    {
                        Debug.LogError("Found null material in source materials for combined mesh materials " + resultMatIdx);
                        return null;
                    }

                    if (!sourceMats2submeshIdx_map.Contains(sourceMats[j]))
                    {
                        sourceMats2submeshIdx_map.Add(sourceMats[j], resultMatIdx);
                    }
                }
            }

            return sourceMats2submeshIdx_map;
        }

        internal Renderer BuildSceneHierarchPreBake(MB3_MeshCombinerSingle mom, GameObject root, Mesh m, bool createNewChild = false, GameObject[] objsToBeAdded = null)
        {
            if (mom._LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("Building Scene Hierarchy createNewChild=" + createNewChild);
            GameObject meshGO;
            MeshFilter mf = null;
            MeshRenderer mr = null;
            SkinnedMeshRenderer smr = null;
            Transform mt = null;
            if (root == null)
            {
                Debug.LogError("root was null.");
                return null;
            }
            if (mom.textureBakeResults == null)
            {
                Debug.LogError("textureBakeResults must be set.");
                return null;
            }
            if (root.GetComponent<Renderer>() != null)
            {
                Debug.LogError("root game object cannot have a renderer component");
                return null;
            }
            if (!createNewChild)
            {
                //try to find an existing child
                if (mom.targetRenderer != null && mom.targetRenderer.transform.parent == root.transform)
                {
                    mt = mom.targetRenderer.transform; //good setup
                }
                else
                {
                    Renderer[] rs = (Renderer[])root.GetComponentsInChildren<Renderer>(true);
                    if (rs.Length == 1)
                    {
                        if (rs[0].transform.parent != root.transform)
                        {
                            Debug.LogError("Target Renderer is not an immediate child of Result Scene Object. Try using a game object with no children as the Result Scene Object..");
                        }
                        mt = rs[0].transform;
                    }
                }
            }
            if (mt != null && mt.parent != root.transform)
            { //target renderer must be a child of root
                mt = null;
            }
            if (mt == null)
            {
                meshGO = new GameObject(mom.name + "-mesh");
                meshGO.transform.parent = root.transform;
                mt = meshGO.transform;
            }
            mt.parent = root.transform;
            meshGO = mt.gameObject;
            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                MeshRenderer r = meshGO.GetComponent<MeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                MeshFilter f = meshGO.GetComponent<MeshFilter>();
                if (f != null) MB_Utility.Destroy(f);
                smr = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (smr == null) smr = meshGO.AddComponent<SkinnedMeshRenderer>();
            }
            else
            {
                SkinnedMeshRenderer r = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                mf = meshGO.GetComponent<MeshFilter>();
                if (mf == null) mf = meshGO.AddComponent<MeshFilter>();
                mr = meshGO.GetComponent<MeshRenderer>();
                if (mr == null) mr = meshGO.AddComponent<MeshRenderer>();
            }
            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                smr.bones = mom.GetBones();
                bool origVal = smr.updateWhenOffscreen;
                smr.updateWhenOffscreen = true;
                smr.updateWhenOffscreen = origVal;
            }

            _ConfigureSceneHierarch(mom, root, mr, mf, smr, m, objsToBeAdded);

            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                return smr;
            }
            else
            {
                return mr;
            }
        }

        /*
         could be building for a multiMeshBaker or a singleMeshBaker, targetRenderer will be a scene object.
        */
        public static void BuildPrefabHierarchy(MB3_MeshCombinerSingle mom, GameObject instantiatedPrefabRoot, Mesh m, bool createNewChild = false, GameObject[] objsToBeAdded = null)
        {
            SkinnedMeshRenderer smr = null;
            MeshRenderer mr = null;
            MeshFilter mf = null;
            GameObject meshGO = new GameObject(mom.name + "-mesh");
            meshGO.transform.parent = instantiatedPrefabRoot.transform;
            Transform mt = meshGO.transform;
            
            mt.parent = instantiatedPrefabRoot.transform;
            meshGO = mt.gameObject;
            if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                MeshRenderer r = meshGO.GetComponent<MeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                MeshFilter f = meshGO.GetComponent<MeshFilter>();
                if (f != null) MB_Utility.Destroy(f);
                smr = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (smr == null) smr = meshGO.AddComponent<SkinnedMeshRenderer>();
            }
            else
            {
                SkinnedMeshRenderer r = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                mf = meshGO.GetComponent<MeshFilter>();
                if (mf == null) mf = meshGO.AddComponent<MeshFilter>();
                mr = meshGO.GetComponent<MeshRenderer>();
                if (mr == null) mr = meshGO.AddComponent<MeshRenderer>();
            }
            if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                smr.bones = mom.GetBones();
                bool origVal = smr.updateWhenOffscreen;
                smr.updateWhenOffscreen = true;
                smr.updateWhenOffscreen = origVal;
                smr.sharedMesh = m;

                MB_BlendShape2CombinedMap srcMap = mom._targetRenderer.GetComponent<MB_BlendShape2CombinedMap>();
                if (srcMap != null)
                {
                    MB_BlendShape2CombinedMap targMap = meshGO.GetComponent<MB_BlendShape2CombinedMap>();
                    if (targMap == null) targMap = meshGO.AddComponent<MB_BlendShape2CombinedMap>();
                    targMap.srcToCombinedMap = srcMap.srcToCombinedMap;
                    for (int i = 0; i < targMap.srcToCombinedMap.combinedMeshTargetGameObject.Length; i++)
                    {
                        targMap.srcToCombinedMap.combinedMeshTargetGameObject[i] = meshGO;
                    }
                }
                
            }

            _ConfigureSceneHierarch(mom, instantiatedPrefabRoot, mr, mf, smr, m, objsToBeAdded);
            
            //First try to get the materials from the target renderer. This is because the mesh may have fewer submeshes than number of result materials if some of the submeshes had zero length tris.
            //If we have just baked then materials on the target renderer will be correct wheras materials on the textureBakeResult may not be correct.
            if (mom.targetRenderer != null)
            {
                Material[] sharedMats = new Material[mom.targetRenderer.sharedMaterials.Length];
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    sharedMats[i] = mom.targetRenderer.sharedMaterials[i];
                }
                if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    smr.sharedMaterial = null;
                    smr.sharedMaterials = sharedMats;
                }
                else
                {
                    mr.sharedMaterial = null;
                    mr.sharedMaterials = sharedMats;
                }
            }
        }


        private static void _ConfigureSceneHierarch(MB3_MeshCombinerSingle mom, GameObject root, MeshRenderer mr, MeshFilter mf, SkinnedMeshRenderer smr, Mesh m, GameObject[] objsToBeAdded = null)
        {
            //assumes everything is set up correctly
            GameObject meshGO;
            if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                meshGO = smr.gameObject;
                //smr.sharedMesh = m; can't assign mesh for skinned mesh until it has skinning information
                smr.lightmapIndex = mom.GetLightmapIndex();
             
            }
            else {
                meshGO = mr.gameObject;
                mf.sharedMesh = m;
                mom._SetLightmapIndexIfPreserveLightmapping(mr); 
            }
            if (mom.settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping || mom.settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
            {
                meshGO.isStatic = true;
            }

            //set layer and tag of combined object if all source objs have same layer
            if (objsToBeAdded != null && objsToBeAdded.Length > 0 && objsToBeAdded[0] != null)
            {
                bool tagsAreSame = true;
                bool layersAreSame = true;
                string tag = objsToBeAdded[0].tag;
                int layer = objsToBeAdded[0].layer;
                for (int i = 0; i < objsToBeAdded.Length; i++)
                {
                    if (objsToBeAdded[i] != null)
                    {
                        if (!objsToBeAdded[i].tag.Equals(tag)) tagsAreSame = false;
                        if (objsToBeAdded[i].layer != layer) layersAreSame = false;
                    }
                }
                if (tagsAreSame)
                {
                    root.tag = tag;
                    meshGO.tag = tag;
                }
                if (layersAreSame)
                {
                    root.layer = layer;
                    meshGO.layer = layer;
                }
            }
        }
        /// <summary>
        /// If the option to preserve current lightmapping is enabled, we use
        /// this function to add the MB_PreserveLightMapData script to the gameobject 
        /// to ensure that the lightmap index and lightmap scale offset are set to prevent the lightmap
        /// data from being lost. 
        /// </summary>
        /// <param name="tr"></param>
        private void _SetLightmapIndexIfPreserveLightmapping(Renderer tr)
        {
            tr.lightmapIndex = GetLightmapIndex();
            tr.lightmapScaleOffset = new Vector4(1, 1, 0, 0);
            if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
            {
                MB_PreserveLightmapData script = tr.gameObject.GetComponent<MB_PreserveLightmapData>();
                if (script == null)
                {
                    script = tr.gameObject.AddComponent<MB_PreserveLightmapData>();
                }
                script.lightmapIndex = GetLightmapIndex();
                script.lightmapScaleOffset = new Vector4(1, 1, 0, 0);
            }
        }

        public void BuildSceneMeshObject(GameObject[] gos = null, bool createNewChild = false)
        {
            if (_resultSceneObject == null)
            {
                _resultSceneObject = new GameObject("CombinedMesh-" + name);
            }

            _targetRenderer = BuildSceneHierarchPreBake(this, _resultSceneObject, GetMesh(), createNewChild, gos);

        }

        //tests if a matrix has been mirrored
        bool IsMirrored(Matrix4x4 tm)
        {
            Vector3 x = tm.GetRow(0);
            Vector3 y = tm.GetRow(1);
            Vector3 z = tm.GetRow(2);
            x.Normalize(); y.Normalize(); z.Normalize();
            float an = Vector3.Dot(Vector3.Cross(x, y), z);
            return an >= 0 ? false : true;
        }

        public override void CheckIntegrity()
        {
            if (!MB_Utility.DO_INTEGRITY_CHECKS) return;
            //check bones.
            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {

                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    HashSet<int> usedBonesWeights = new HashSet<int>();
                    HashSet<int> usedBonesIndexes = new HashSet<int>();
                    for (int j = dgo.vertIdx; j < dgo.vertIdx + dgo.numVerts; j++)
                    {
                        usedBonesWeights.Add(boneWeights[j].boneIndex0);
                        usedBonesWeights.Add(boneWeights[j].boneIndex1);
                        usedBonesWeights.Add(boneWeights[j].boneIndex2);
                        usedBonesWeights.Add(boneWeights[j].boneIndex3);
                    }
                    for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++)
                    {
                        usedBonesIndexes.Add(dgo.indexesOfBonesUsed[j]);
                    }

                    usedBonesIndexes.ExceptWith(usedBonesWeights);
                    if (usedBonesIndexes.Count > 0)
                    {
                        Debug.LogError("The bone indexes were not the same. " + usedBonesWeights.Count + " " + usedBonesIndexes.Count);
                    }
                    for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++)
                    {
                        if (j < 0 || j > bones.Length)
                            Debug.LogError("Bone index was out of bounds.");
                    }
                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer && dgo.indexesOfBonesUsed.Length < 1)
                        Debug.Log("DGO had no bones");

                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.uvRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceSharedMaterials.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.encapsulatingRect.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceMaterialTiling.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.obUVRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                }

            }

            //check blend shapes
            if (settings.doBlendShapes)
            {
                if (settings.renderType != MB_RenderType.skinnedMeshRenderer)
                {
                    Debug.LogError("Blend shapes can only be used with skinned meshes.");
                }
            }
        }

        public override List<Material> GetMaterialsOnTargetRenderer()
        {
            List<Material> outMats = new List<Material>();
            if (_targetRenderer != null)
            {
                outMats.AddRange(_targetRenderer.sharedMaterials);
            }
            return outMats;
        }
    }
}