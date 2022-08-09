using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DigitalOpus.MB.Core {
    public class MB3_CopyBoneWeights {
        public static void CopyBoneWeightsFromSeamMeshToOtherMeshes(float radius, Mesh seamMesh, Mesh[] targetMeshes, Transform[][] newBonesForSMRs, Transform[] seamMeshBones, Transform[][] targMeshBones) {
            //Todo make sure meshes are assets
            Debug.Assert(targetMeshes.Length == newBonesForSMRs.Length);
            List<int> seamVerts = new List<int> ();
            if (seamMesh == null) {
                Debug.LogError(string.Format("The SeamMesh cannot be null"));
                return;
            }
            if (seamMesh.vertexCount == 0){
                Debug.LogError("The seam mesh has no vertices. Check that the Asset Importer for the seam mesh does not have 'Optimize Mesh' checked.");
                return;
            }
            Vector3[] srcVerts = seamMesh.vertices;
            BoneWeight[] srcBW = seamMesh.boneWeights;
            Vector3[] srcNormal = seamMesh.normals;
            Vector4[] srcTangent = seamMesh.tangents;
            Vector2[] srcUVs = seamMesh.uv;

            if (srcUVs.Length != srcVerts.Length) {
                Debug.LogError("The seam mesh needs uvs to identify which vertices are part of the seam. Vertices with UV > .5 are part of the seam. Vertices with UV < .5 are not part of the seam.");
                return;
            }

            for (int i = 0; i < srcUVs.Length; i++) {
                if (srcUVs[i].x > .5f && srcUVs[i].y > .5f){
                    seamVerts.Add (i);
                }
            }
            Debug.Log (string.Format("The seam mesh has {0} vertices of which {1} are seam vertices.", seamMesh.vertices.Length, seamVerts.Count));
            if (seamVerts.Count == 0) {
                Debug.LogError("None of the vertices in the Seam Mesh were marked as seam vertices. To mark a vertex as a seam vertex the UV" +
                               " must be greater than (.5,.5). Vertices with UV less than (.5,.5) are excluded.");
                return;
            }

            //validate
            bool failed = false;
            if (radius <= 0f)
            {
                Debug.LogError("radius must be greater than zero.");
            }

            for (int meshIdx = 0; meshIdx < targetMeshes.Length; meshIdx++) {
                if (targetMeshes[meshIdx] == null) {
                        Debug.LogError(string.Format("Mesh {0} was null", meshIdx));
                    failed = true;
                }
            }
            if (failed) {
                return;
            }

            // For re-mapping bone weight indexes.
            Dictionary<Transform, int> seamMesh_Transform2boneIdx_map = new Dictionary<Transform, int>();
            int[] map_seamMeshIdx2targMeshIdx = new int[seamMeshBones.Length];
            {
                // Build maps so we can adjust bone weight indexes.
                for (int i = 0; i < seamMeshBones.Length; i++)
                {
                    seamMesh_Transform2boneIdx_map.Add(seamMeshBones[i], i);
                }
            }

            Dictionary<int, int> newBonesToAdd = new Dictionary<int, int>();
            for (int meshIdx = 0; meshIdx < targetMeshes.Length; meshIdx++) {
                Mesh tm = targetMeshes[meshIdx];
                if (tm == seamMesh) continue;

                Vector3[] otherVerts = tm.vertices;
                BoneWeight[] otherBWs = tm.boneWeights;
                Vector3[] otherNormals = tm.normals;
                Vector4[] otherTangents = tm.tangents;
                newBonesToAdd.Clear();
                {
                    for (int i = 0; i < map_seamMeshIdx2targMeshIdx.Length; i++) map_seamMeshIdx2targMeshIdx[i] = -1; // for integrity checking.
                    Transform[] bones = targMeshBones[meshIdx];
                    for (int targMeshBnIdx = 0; targMeshBnIdx < bones.Length; targMeshBnIdx++)
                    {
                        Transform t = bones[targMeshBnIdx];
                        if (seamMesh_Transform2boneIdx_map.ContainsKey(t))
                        {
                            int seamMeshBnIdx = seamMesh_Transform2boneIdx_map[t];
                            map_seamMeshIdx2targMeshIdx[seamMeshBnIdx] = targMeshBnIdx;
                        }
                    }
                }

                int numMatches = 0;
                for (int i = 0; i < otherVerts.Length; i++)
                {
                    for (int sIdx = 0; sIdx < seamVerts.Count; sIdx++)
                    {
                        int j = seamVerts[sIdx];
                        if (Vector3.Distance(otherVerts[i], srcVerts[j]) <= radius)
                        {
                            if (seamMesh == targetMeshes[meshIdx])
                            {
                                if (i != j) Debug.LogError("Same mesh but different verts overlapped. radius too big " + i + "  " + j);
                            }
                            numMatches++;
                            BoneWeight bw = srcBW[j];
                            RemapBoneWeightIndexes(targetMeshes[meshIdx].name, ref bw, map_seamMeshIdx2targMeshIdx, targMeshBones[meshIdx], newBonesToAdd, seamMeshBones);

                            otherBWs[i] = bw;
                            otherVerts[i] = srcVerts[j];
                            if (otherNormals.Length == otherVerts.Length && srcNormal.Length == srcNormal.Length)
                            {
                                otherNormals[i] = srcNormal[j];
                            }
                            if (otherTangents.Length == otherVerts.Length && srcTangent.Length == srcVerts.Length)
                            {
                                otherTangents[i] = srcTangent[j];
                            }
                        }
                    }
                }

                if (newBonesToAdd.Count > 0)
                {
                    // The seam mesh verts used bones that the target mesh verts did not.
                    // Add these bones to the target mesh SMRs.                    
                    int numNewAllBones = targMeshBones[meshIdx].Length + newBonesToAdd.Count;
                    Transform[] newAllBonessList = new Transform[numNewAllBones];
                    Matrix4x4[] newBindPoses = new Matrix4x4[numNewAllBones];
                    Matrix4x4[] oldBindPoses = targetMeshes[meshIdx].bindposes;
                    for (int i = 0; i < targMeshBones[meshIdx].Length; i++) // add orginal bones
                    {
                        newBindPoses[i] = oldBindPoses[i];
                        newAllBonessList[i] = targMeshBones[meshIdx][i];
                    }

                    Matrix4x4[] seamMeshBindPoses = seamMesh.bindposes;
                    foreach(var k in newBonesToAdd) // add new bones.
                    {
                        newAllBonessList[k.Value] = seamMeshBones[k.Key];
                        newBindPoses[k.Value] = seamMeshBindPoses[k.Key];
                    }

                    // integrity check
                    for (int i = 0; i < newBonesToAdd.Count; i++)
                    {
                        if (newAllBonessList[i] == null) Debug.LogError("Should never happend. Not all target indexes were covered.");
                    }

                    newBonesForSMRs[meshIdx] = newAllBonessList;
                    targetMeshes[meshIdx].bindposes = newBindPoses;
                }

                if (numMatches > 0) {
                    targetMeshes[meshIdx].vertices = otherVerts;
                    targetMeshes[meshIdx].boneWeights = otherBWs;
                    targetMeshes[meshIdx].normals = otherNormals;
                    targetMeshes[meshIdx].tangents = otherTangents;
                }
                Debug.Log(string.Format("Copied boneweights for {1} vertices in mesh {0} that matched positions in the seam mesh.", targetMeshes[meshIdx].name, numMatches));
            }
        }

        static void RemapBoneWeightIndexes(string nm, ref BoneWeight seamMeshBw, int[] map_seamMeshIdx2targMeshIdx, Transform[] targBones, Dictionary<int,int> extraBones, Transform[] seamBones)
        {
            int targMeshBoneIndex0 = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex0];
            int targMeshBoneIndex1 = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex1];
            int targMeshBoneIndex2 = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex2];
            int targMeshBoneIndex3 = map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex3];

            // The seam mesh might use bones that are not in the list of bones used by the targetMesh. We need to add these bones
            // to the target mesh.
            if (targMeshBoneIndex0 == -1)
            {
                Debug.Assert(!extraBones.ContainsKey(seamMeshBw.boneIndex0));
                targMeshBoneIndex0 = targBones.Length + extraBones.Count;
                extraBones.Add(seamMeshBw.boneIndex0, targMeshBoneIndex0);
                map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex0] = targMeshBoneIndex0;
                //Debug.LogError("Error copying bone weight from seam mesh to target mesh " + nm + ". The seam mesh vertex used a bone " + seamBones[seamMeshBw.boneIndex0] + " that was not used by the bone weight on the target mesh vertes.");
            }
            if (targMeshBoneIndex1 == -1)
            {
                Debug.Assert(!extraBones.ContainsKey(seamMeshBw.boneIndex1));
                targMeshBoneIndex1 = targBones.Length + extraBones.Count;
                extraBones.Add(seamMeshBw.boneIndex1, targMeshBoneIndex1);
                map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex1] = targMeshBoneIndex1;
                //Debug.LogError("Error copying bone weight from seam mesh to target mesh " + nm + ". The seam mesh vertex used a bone " + seamBones[seamMeshBw.boneIndex1] + " that was not used by the bone weight on the target mesh vertes.");
            }
            if (targMeshBoneIndex2 == -1)
            {
                Debug.Assert(!extraBones.ContainsKey(seamMeshBw.boneIndex2));
                targMeshBoneIndex2 = targBones.Length + extraBones.Count;
                extraBones.Add(seamMeshBw.boneIndex2, targMeshBoneIndex2);
                map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex2] = targMeshBoneIndex2;
                //Debug.LogError("Error copying bone weight from seam mesh to target mesh " + nm + ". The seam mesh vertex used a bone " + seamBones[seamMeshBw.boneIndex2] + " that was not used by the bone weight on the target mesh vertes.");
            }
            if (targMeshBoneIndex3 == -1)
            {
                Debug.Assert(!extraBones.ContainsKey(seamMeshBw.boneIndex3));
                targMeshBoneIndex3 = targBones.Length + extraBones.Count;
                extraBones.Add(seamMeshBw.boneIndex3, targMeshBoneIndex3);
                map_seamMeshIdx2targMeshIdx[seamMeshBw.boneIndex3] = targMeshBoneIndex3;
                //Debug.LogError("Error copying bone weight from seam mesh to target mesh " + nm + ". The seam mesh vertex used a bone " + seamBones[seamMeshBw.boneIndex3] + " that was not used by the bone weight on the target mesh vertes.");
            }
            
            seamMeshBw.boneIndex0 = targMeshBoneIndex0;
            seamMeshBw.boneIndex1 = targMeshBoneIndex1;
            seamMeshBw.boneIndex2 = targMeshBoneIndex2;
            seamMeshBw.boneIndex3 = targMeshBoneIndex3;
        }
    }
}
