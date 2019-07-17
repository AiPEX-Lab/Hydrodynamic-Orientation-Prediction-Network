// This code contains NVIDIA Confidential Information and is disclosed to you
// under a form of NVIDIA software license agreement provided separately to you.
//
// Notice
// NVIDIA Corporation and its licensors retain all intellectual property and
// proprietary rights in and to this software and related documentation and
// any modifications thereto. Any use, reproduction, disclosure, or
// distribution of this software and related documentation without an express
// license agreement from NVIDIA Corporation is strictly prohibited.
//
// ALL NVIDIA DESIGN SPECIFICATIONS, CODE ARE PROVIDED "AS IS.". NVIDIA MAKES
// NO WARRANTIES, EXPRESSED, IMPLIED, STATUTORY, OR OTHERWISE WITH RESPECT TO
// THE MATERIALS, AND EXPRESSLY DISCLAIMS ALL IMPLIED WARRANTIES OF NONINFRINGEMENT,
// MERCHANTABILITY, AND FITNESS FOR A PARTICULAR PURPOSE.
//
// Information and code furnished is believed to be accurate and reliable.
// However, NVIDIA Corporation assumes no responsibility for the consequences of use of such
// information or for any infringement of patents or other rights of third parties that may
// result from its use. No license is granted by implication or otherwise under any patent
// or patent rights of NVIDIA Corporation. Details are subject to change without notice.
// This code supersedes and replaces all information previously supplied.
// NVIDIA Corporation products are not authorized for use as critical
// components in life support devices or systems without express written approval of
// NVIDIA Corporation.
//
// Copyright (c) 2018 NVIDIA Corporation. All rights reserved.

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    public class FlexSolidAsset : FlexAsset
    {
        #region Properties

        public Mesh boundaryMesh
        {
            get { return m_boundaryMesh; }
            set { m_boundaryMesh = value; }
        }

        public Vector3 meshLocalScale
        {
            get { return m_meshLocalScale; }
            set { m_meshLocalScale = value; }
        }

        public float meshExpansion
        {
            get { return m_meshExpansion; }
            set { m_meshExpansion = value; }
        }

        public float particleSpacing
        {
            get { return m_particleSpacing; }
            set { m_particleSpacing = Mathf.Max(value, 0.01f); }
        }

        #endregion

        #region Methods

        public Mesh ObjImporter()
        {
            string objPath = "Assets/src/Flat Plate DQ v1.obj";

            Mesh nm = Importer.ImportFile(objPath);

            return nm;
        } 

        public class Importer
        {
        private struct meshStruct
        {
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector2[] uv;
            public Vector2[] uv1;
            public Vector2[] uv2;
            public int[] triangles;
            public int[] faceVerts;
            public int[] faceUVs;
            public Vector3[] faceData;
            public string name;
            public string fileName;
        }

        public static Mesh ImportFile(string filePath)
        {
            meshStruct newMesh = createMeshStruct(filePath);
            populateMeshStruct(ref newMesh);

            Vector3[] newVerts = new Vector3[newMesh.faceData.Length];
            Vector2[] newUVs = new Vector2[newMesh.faceData.Length];
            Vector3[] newNormals = new Vector3[newMesh.faceData.Length];
            int i = 0;
            /* The following foreach loops through the facedata and assigns the appropriate vertex, uv, or normal
             * for the appropriate Unity mesh array.
             */
            foreach (Vector3 v in newMesh.faceData)
            {
                newVerts[i] = newMesh.vertices[(int)v.x - 1];
                if (v.y >= 1)
                    newUVs[i] = newMesh.uv[(int)v.y - 1];

                if (v.z >= 1)
                    newNormals[i] = newMesh.normals[(int)v.z - 1];
                i++;
            }

            Mesh mesh = new Mesh();

            mesh.vertices = newVerts;
            mesh.uv = newUVs;
            //mesh.normals = newNormals;
            mesh.triangles = newMesh.triangles;

            mesh.RecalculateBounds();    
            mesh.RecalculateNormals();

            return mesh;
        }

        private static meshStruct createMeshStruct(string filename)
        {
            int triangles = 0;
            int vertices = 0;
            int vt = 0;
            int vn = 0;
            int face = 0;
            meshStruct mesh = new meshStruct();
            mesh.fileName = filename;
            StreamReader stream = File.OpenText(filename);
            string entireText = stream.ReadToEnd();
            stream.Close();
            using (StringReader reader = new StringReader(entireText))
            {
                string currentText = reader.ReadLine();
                char[] splitIdentifier = { ' ' };
                string[] brokenString;
                while (currentText != null)
                {
                    if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ")
                        && !currentText.StartsWith("vn "))
                    {
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                    else
                    {
                        currentText = currentText.Trim();                           //Trim the current line
                        brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
                        switch (brokenString[0])
                        {
                            case "v":
                                vertices++;
                                break;

                            case "vt":
                                vt++;
                                break;

                            case "vn":
                                vn++;
                                break;

                            case "f":
                                face = face + brokenString.Length - 1;
                                triangles = triangles + 3 * (brokenString.Length - 2); /*brokenString.Length is 3 or greater since a face must have at least
                                                                                     3 vertices.  For each additional vertice, there is an additional
                                                                                     triangle in the mesh (hence this formula).*/
                                break;
                        }
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                }
            }
            mesh.triangles = new int[triangles];
            mesh.vertices = new Vector3[vertices];
            mesh.uv = new Vector2[vt];
            mesh.normals = new Vector3[vn];
            mesh.faceData = new Vector3[face];
            return mesh;
        }

        private static void populateMeshStruct(ref meshStruct mesh)
        {
            StreamReader stream = File.OpenText(mesh.fileName);

            string entireText = stream.ReadToEnd();
            stream.Close();
            using (StringReader reader = new StringReader(entireText))
            {
                string currentText = reader.ReadLine();

                char[] splitIdentifier = { ' ' };
                char[] splitIdentifier2 = { '/' };
                string[] brokenString;
                string[] brokenBrokenString;
                int f = 0;
                int f2 = 0;
                int v = 0;
                int vn = 0;
                int vt = 0;
                int vt1 = 0;
                int vt2 = 0;
                while (currentText != null)
                {
                    if (!currentText.StartsWith("f ") && !currentText.StartsWith("v ") && !currentText.StartsWith("vt ") &&
                        !currentText.StartsWith("vn ") && !currentText.StartsWith("g ") && !currentText.StartsWith("usemtl ") &&
                        !currentText.StartsWith("mtllib ") && !currentText.StartsWith("vt1 ") && !currentText.StartsWith("vt2 ") &&
                        !currentText.StartsWith("vc ") && !currentText.StartsWith("usemap "))
                    {
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");
                        }
                    }
                    else
                    {
                        currentText = currentText.Trim();
                        brokenString = currentText.Split(splitIdentifier, 50);
                        switch (brokenString[0])
                        {
                            case "g":
                                break;

                            case "usemtl":
                                break;

                            case "usemap":
                                break;

                            case "mtllib":
                                break;

                            case "v":
                                mesh.vertices[v] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                         System.Convert.ToSingle(brokenString[3]));
                                v++;
                                break;

                            case "vt":
                                mesh.uv[vt] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                                vt++;
                                break;

                            case "vt1":
                                mesh.uv[vt1] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                                vt1++;
                                break;

                            case "vt2":
                                mesh.uv[vt2] = new Vector2(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]));
                                vt2++;
                                break;

                            case "vn":
                                mesh.normals[vn] = new Vector3(System.Convert.ToSingle(brokenString[1]), System.Convert.ToSingle(brokenString[2]),
                                                        System.Convert.ToSingle(brokenString[3]));
                                vn++;
                                break;

                            case "vc":
                                break;

                            case "f":

                                int j = 1;
                                List<int> intArray = new List<int>();
                                while (j < brokenString.Length && ("" + brokenString[j]).Length > 0)
                                {
                                    Vector3 temp = new Vector3();
                                    brokenBrokenString = brokenString[j].Split(splitIdentifier2, 3);    //Separate the face into individual components (vert, uv, normal)
                                    temp.x = System.Convert.ToInt32(brokenBrokenString[0]);
                                    if (brokenBrokenString.Length > 1)                                  //Some .obj files skip UV and normal
                                    {
                                        if (brokenBrokenString[1] != "")                                    //Some .obj files skip the uv and not the normal
                                        {
                                            temp.y = System.Convert.ToInt32(brokenBrokenString[1]);
                                        }
                                        temp.z = System.Convert.ToInt32(brokenBrokenString[2]);
                                    }
                                    j++;

                                    mesh.faceData[f2] = temp;
                                    intArray.Add(f2);
                                    f2++;
                                }
                                j = 1;
                                while (j + 2 < brokenString.Length)     //Create triangles out of the face data.  There will generally be more than 1 triangle per face.
                                {
                                    mesh.triangles[f] = intArray[0];
                                    f++;
                                    mesh.triangles[f] = intArray[j];
                                    f++;
                                    mesh.triangles[f] = intArray[j + 1];
                                    f++;

                                    j++;
                                }
                                break;
                        }
                        currentText = reader.ReadLine();
                        if (currentText != null)
                        {
                            currentText = currentText.Replace("  ", " ");       //Some .obj files insert double spaces, this removes them.
                        }
                    }
                }
            }
        }
    }

        #endregion

        #region Messages


        #endregion

        #region Protected

        protected override void ValidateFields()
        {
            base.ValidateFields();
            m_particleSpacing = Mathf.Max(m_particleSpacing, 0.01f);
        }

        protected override void RebuildAsset()
        {
            BuildFromMesh();
            base.RebuildAsset();
        }

        #endregion

        #region Private

        void BuildFromMesh()
        {
            if (m_boundaryMesh)
            {
                Vector3[] vertices = m_boundaryMesh.vertices;
                if (vertices != null && vertices.Length > 0)
                {
                    for (int i = 0; i < vertices.Length; ++i)
                    {
                        Vector3 v = vertices[i];
                        vertices[i] = new Vector3(v.x * m_meshLocalScale.x, v.y * m_meshLocalScale.y, v.z * m_meshLocalScale.z);
                    }
                    int[] indices = m_boundaryMesh.triangles;
                    if (indices != null && indices.Length > 0)
                    {
                        FlexExt.Asset.Handle assetHandle = FlexExt.CreateRigidFromMesh(ref vertices[0], vertices.Length, ref indices[0], indices.Length, m_particleSpacing, m_meshExpansion);
                        if (assetHandle)
                        {
                            StoreAsset(assetHandle.asset);

                            FlexExt.DestroyAsset(assetHandle);
                        }
                    }
                }
            }
        }

        [SerializeField]
        Mesh m_boundaryMesh = null;
        [SerializeField]
        Vector3 m_meshLocalScale = Vector3.one;
        [SerializeField, Tooltip("Particles will be moved inwards (if negative) or outwards (if positive) from the surface of the mesh according to this factor")]
        float m_meshExpansion = 0.0f;
        [SerializeField, Tooltip("The spacing used for voxelization, note that the number of voxels grows proportional to the inverse cube of radius, currently this method limits construction to resolutions < 64^3")]
        float m_particleSpacing = 0.1f;

        #endregion
    }
}
