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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Data.SQLite;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class FlexActor : MonoBehaviour
    {
        #region Properties

        public FlexContainer container
        {
            get { return m_container; }
            set { m_container = value; }
        }

        public FlexAsset asset
        {
            get { return m_currentAsset; }
        }

        public int particleGroup
        {
            get { return m_particleGroup; }
            set { m_particleGroup = value; }
        }

        public bool selfCollide
        {
            get { return m_selfCollide; }
            set { m_selfCollide = value; }
        }

        public bool fluid
        {
            get { return m_fluid; }
            set { m_fluid = value; }
        }

        public float massScale
        {
            get { return m_massScale; }
            set { m_massScale = value; }
        }

        public bool drawParticles
        {
            get { return m_drawParticles; }
            set { m_drawParticles = value; }
        }

        public FlexExt.Instance.Handle handle
        {
            get { return m_instanceHandle; }
        }

        public int[] indices
        {
            get { return m_indices; }
        }

        public int indexCount
        {
            get { return m_indexCount; }
        }

        public Bounds bounds { get { return m_bounds; } }

        #endregion

        #region Events

        public delegate void OnBeforeRecreateFn();
        public event OnBeforeRecreateFn onBeforeRecreate;

        public delegate void OnAfterRecreateFn();
        public event OnAfterRecreateFn onAfterRecreate;

        public delegate void OnFlexUpdateFn(FlexContainer.ParticleData _particleData);
        public event OnFlexUpdateFn onFlexUpdate;

        #endregion

        #region Methods

        public void Recreate()
        {
            if (onBeforeRecreate != null) onBeforeRecreate();

            if (m_recreateActor == 2)
            {
                DestroyActor();
                CreateActor();
            }
            else if (m_recreateActor == 1)
            {
                DestroyInstance();
                CreateInstance();
            }

            m_recreateActor = 0;

            if (onAfterRecreate != null) onAfterRecreate();
        }

        public void Teleport(Vector3 _targetPosition, Quaternion _targetRotation)
        {
            m_teleportPosition = _targetPosition;
            m_teleportRotation = _targetRotation;
            m_teleport = 5;
        }

        public void ApplyImpulse(Vector3 _impulse, int _particle = -1)
        {
            ImpulseInfo info;
            info.impulse = _impulse;
            info.particle = _particle;
            m_impulses.Add(info);
        }


        //Wrapper of the PresetVelocities function
        public bool PresetVelocity(FlexContainer.ParticleData _particleData)
        {
            return PresetVelocities(_particleData);
        }

        //Wrapper of the CoutAccelerations function
        public bool CoutAcceleration(FlexContainer.ParticleData _particleData)
        {
            return CoutAccelerations(_particleData);
        }

        #endregion

        #region Messages

        void OnEnable()
        {
            CreateActor();
        }

        void OnDisable()
        {
            DestroyActor();
        }

        void OnValidate()
        {
            ValidateFields();
            if (m_recreateActor > 0) Recreate();
        }

        void Update()
        {
            if (transform.hasChanged && !Application.isPlaying)
            {
                m_recreateActor = 1;
                Recreate();
                transform.hasChanged = false;
            }
        }
        #endregion

        #region Protected

        protected static Color GIZMO_COLOR = new Color(1.0f, 0.5f, 0.0f);

        protected FlexContainer m_currentContainer;
        protected FlexAsset m_currentAsset;

        protected virtual FlexAsset subclassAsset { get { return null; } }

        protected int m_phase = 0;

        void AddToContainer()
        {
            if (m_container)
            {
                m_currentContainer = m_container;
                m_currentContainer.onFlexUpdate += OnFlexUpdate;
                m_currentContainer.onBeforeRecreate += OnBeforeRecreate;
                m_currentContainer.onAfterRecreate += OnAfterRecreate;
                m_currentContainer.onBeforeDestroy += OnBeforeDestroy;
                m_currentContainer.AddActor(this);
            }
        }

        void RemoveFromContainer()
        {
            if (m_currentContainer)
            {
                m_currentContainer.onFlexUpdate -= OnFlexUpdate;
                m_currentContainer.onBeforeRecreate -= OnBeforeRecreate;
                m_currentContainer.onAfterRecreate -= OnAfterRecreate;
                m_currentContainer.onBeforeDestroy -= OnBeforeDestroy;
                m_currentContainer.RemoveActor(this);
                m_currentContainer = null;
            }
        }

        void AcquireAsset()
        {
            if (subclassAsset)
            {
                m_currentAsset = subclassAsset;
                m_currentAsset.onBeforeRebuild += OnBeforeRecreate;
                m_currentAsset.onAfterRebuild += OnAfterRecreate;
            }
        }

        void ReleaseAsset()
        {
            if (m_currentAsset)
            {
                m_currentAsset.onBeforeRebuild -= OnBeforeRecreate;
                m_currentAsset.onAfterRebuild -= OnAfterRecreate;
                m_currentAsset = null;
            }
        }

        protected virtual void CreateInstance()
        {
            if (m_currentContainer && m_currentContainer.handle && m_currentAsset && m_currentAsset.handle)
            {
                int group = m_particleGroup < 0 ? ++sm_nextGroup : m_particleGroup;
                Flex.Phase flags = Flex.Phase.Default;
                if (m_selfCollide) flags |= Flex.Phase.SelfCollide;
                if (m_selfCollideFilter) flags |= Flex.Phase.SelfCollideFilter;
                if (m_fluid) flags |= Flex.Phase.Fluid;
                m_phase = Flex.MakePhase(group, flags);
                m_instanceHandle = m_currentContainer.CreateInstance(m_currentAsset.handle, transform.localToWorldMatrix, Vector3.zero, m_phase, m_massScale);
                if (m_instanceHandle)
                {
                    FlexExt.Instance instance = m_instanceHandle.instance;
                    m_indices = new int[m_currentAsset.maxParticles];
                    m_indexCount = instance.numParticles;
                    if (m_indexCount > 0) FlexUtils.FastCopy(instance.particleIndices, 0, ref m_indices[0], 0, sizeof(int) * m_indexCount);
                }
            }
        }

        protected virtual void DestroyInstance()
        {
            if (m_instanceHandle)
            {
                m_currentContainer.DestroyInstance(m_instanceHandle);
                m_instanceHandle.Clear();
                m_indices = new int[0];
            }

            if (m_drawParticlesHelper)
            {
                if (gameObject.activeInHierarchy) StartCoroutine("DestroyDelayed", m_drawParticlesHelper.gameObject);
                else DestroyImmediate(m_drawParticlesHelper.gameObject);
                m_drawParticlesHelper = null;
            }
        }

        IEnumerator DestroyDelayed(GameObject go)
        {
            yield return new WaitForEndOfFrame();
            DestroyImmediate(go);
        }

        protected virtual void ValidateFields()
        {
            m_massScale = Mathf.Max(m_massScale, 0.01f);
        }

        protected virtual void OnFlexUpdate(FlexContainer.ParticleData _particleData)
        {
            UpdateDrawParticles();

            if (transform.hasChanged && Application.isPlaying)
            {
                MoveFixedParticles(_particleData);
                //if (Application.isPlaying) MoveFixedParticles(_particleData);
                //else { m_recreateActor = 1; Recreate(); }
                transform.hasChanged = false;
            }

            if (m_teleport > 0)
            {
                TeleportParticles(_particleData);
                --m_teleport;
            }

            if (m_impulses.Count > 0)
            {
                ApplyImpulses(_particleData);
                m_impulses.Clear();
            }

            UpdateBounds(_particleData);

            if (onFlexUpdate != null) onFlexUpdate(_particleData);
        }

        protected void SetIndices(int[] _indices, int _count, FlexContainer.ParticleData _particleData)
        {
            m_indexCount = Mathf.Min(_count, m_currentAsset.maxParticles);
            Array.Copy(_indices, m_indices, m_indexCount);
            FlexExt.Instance instance = m_instanceHandle.instance;
            instance.numParticles = m_indexCount;
            if (m_indexCount > 0) FlexUtils.FastCopy(ref m_indices[0], 0, instance.particleIndices, 0, sizeof(int) * m_indexCount);
            m_instanceHandle.instance = instance;
        }

        #endregion

        #region Private

        void CreateActor()
        {
            AddToContainer();
            AcquireAsset();
            CreateInstance();
        }

        void DestroyActor()
        {
            DestroyInstance();
            ReleaseAsset();
            RemoveFromContainer();
        }

        void OnBeforeRecreate()
        {
            if (onBeforeRecreate != null) onBeforeRecreate();
            DestroyInstance();
        }

        void OnAfterRecreate()
        {
            CreateInstance();
            if (onAfterRecreate != null) onAfterRecreate();
        }

        void OnBeforeDestroy()
        {
            DestroyActor();
        }

        void MoveFixedParticles(FlexContainer.ParticleData _particleData)
        {
            if (Application.isPlaying)
            {
                if (m_currentAsset && m_currentAsset.fixedParticles.Length > 0 && m_instanceHandle)
                {
                    FlexExt.Instance instance = m_instanceHandle.instance;
                    Vector4[] particles = m_currentAsset.particles;
                    foreach (var index in m_currentAsset.fixedParticles)
                    {
                        if (index < particles.Length)
                        {
                            Vector4 particle = transform.TransformPoint(particles[index]); particle.w = 0.0f;
                            _particleData.SetParticle(indices[index], particle);
                        }
                    }
                }
            }
            else
            {
                if (m_currentAsset && m_instanceHandle)
                {
                    FlexExt.Instance instance = m_instanceHandle.instance;
                    Vector4[] particles = m_currentAsset.particles;
                    for (int i = 0; i < particles.Length; ++i)
                    {
                        Vector4 particle = transform.TransformPoint(particles[i]);
                        particle.w = _particleData.GetParticle(indices[i]).w;
                        _particleData.SetParticle(indices[i], particle);
                    }
                }
            }
        }

        void UpdateDrawParticles()
        {
            if (!m_drawParticles && m_drawParticlesHelper)
            {
                DestroyImmediate(m_drawParticlesHelper.gameObject);
                m_drawParticlesHelper = null;
            }
            if (m_drawParticles && !m_drawParticlesHelper)
            {
                GameObject drawParticlesObject = new GameObject("FlexDrawParticles");
                drawParticlesObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                drawParticlesObject.transform.parent = transform;
                drawParticlesObject.transform.localPosition = Vector3.zero;
                drawParticlesObject.transform.rotation = Quaternion.identity;
                m_drawParticlesHelper = drawParticlesObject.AddComponent<_auxFlexDrawParticles>();
            }
            if (m_drawParticlesHelper)
                m_drawParticlesHelper.UpdateMesh();
        }

        void UpdateBounds(FlexContainer.ParticleData _particleData)
        {
            Vector3 boundsMin = Vector3.one * float.MaxValue, boundsMax = Vector3.one * float.MaxValue;
            if (m_container != null && m_indices != null && m_indices.Length > 0)
                FlexUtils.ComputeBounds(_particleData.particleData.particles, ref m_indices[0], m_indices.Length, ref boundsMin, ref boundsMax);
            m_bounds.SetMinMax(boundsMin, boundsMax);
        }

        void TeleportParticles(FlexContainer.ParticleData _particleData)
        {
            Matrix4x4 offset = Matrix4x4.TRS(m_teleportPosition, Quaternion.identity, Vector3.one)
                                * Matrix4x4.TRS(Vector3.zero, m_teleportRotation, Vector3.one)
                                * Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(transform.rotation), Vector3.one)
                                * Matrix4x4.TRS(-transform.position, Quaternion.identity, Vector3.one);
            if (m_currentAsset && m_instanceHandle)
            {
                FlexExt.Instance instance = m_instanceHandle.instance;
                int[] indices = new int[instance.numParticles];
                FlexUtils.FastCopy(instance.particleIndices, indices);
                foreach (var index in indices)
                {
                    Vector4 particle = _particleData.GetParticle(index);
                    float storeW = particle.w;
                    particle = offset.MultiplyPoint3x4(particle);
                    particle.w = storeW;
                    _particleData.SetParticle(index, particle);
                    _particleData.SetVelocity(index, Vector3.zero);
                }
            }
            transform.position = m_teleportPosition;
            transform.rotation = m_teleportRotation;
            transform.hasChanged = false;
        }

        void ApplyImpulses(FlexContainer.ParticleData _particleData)
        {
            if (m_currentAsset && m_instanceHandle)
            {
                FlexExt.Instance instance = m_instanceHandle.instance;
                int[] indices = new int[instance.numParticles];
                FlexUtils.FastCopy(instance.particleIndices, indices);
                foreach (var info in m_impulses)
                {
                    if (info.impulse.sqrMagnitude < float.Epsilon) continue;
                    float mass = 0;
                    foreach (var index in indices)
                    {
                        if (info.particle == -1 || info.particle == index)
                        {
                            Vector4 particle = _particleData.GetParticle(index);
                            mass += 1.0f / particle.w;
                        }
                    }
                    if (mass < float.Epsilon) continue;
                    Vector3 velocityChange = info.impulse / mass;
                    foreach (var index in indices)
                    {
                        _particleData.SetVelocity(index, _particleData.GetVelocity(index) + velocityChange);
                    }
                }
            }
        }

        //This function is to set the initial velocity of the fluid
        bool PresetVelocities(FlexContainer.ParticleData _particleData)
        {
            Vector3 initVelocity = new Vector3(0,0,-2.0f);              //Change the Vector to adjust the intial velocity
            FlexExt.Instance instance = m_instanceHandle.instance;
            int[] indices = new int[instance.numParticles];
            FlexUtils.FastCopy(instance.particleIndices, indices);

            foreach (var index in indices)
            {
                _particleData.SetVelocity(index, initVelocity);
            }
            return true;
        }

        //This function is to get the velocity of 40 frames
        //And then call the CalcAcceleration function to calculate the accelerations
        bool CoutAccelerations(FlexContainer.ParticleData _particleData)
        {
            FlexExt.Instance instance = m_instanceHandle.instance;
            int[] indices = new int[instance.numParticles];
            FlexUtils.FastCopy(instance.particleIndices, indices);
            float totalzVelocity = 0;

            foreach (var index in indices)
            {
                totalzVelocity += _particleData.GetVelocity(index).z;
            }

            frameVelocity.Add(totalzVelocity/indices.Length);

            if (frameVelocity.Count >= 40)                              //Change the 40 to other numbers to adjust the time of data collections
            {
                return CalcAcceleration();
            }
            else
            {
                return false;
            }
        }

        //This function is to calculate the accelerations
        bool CalcAcceleration()
        {
            List<float> frameAcceleration = new List<float>();
            float acceleration = 0.0f;

            for (int i = 0; i <frameVelocity.Count - 1; i++)
            {
                frameAcceleration.Add(Mathf.Abs(frameVelocity[i] - frameVelocity[i + 1]));      //Calcuate the accelerations between each frames
            }

            for (int i = 0; i < frameAcceleration.Count; i++)
            {
                acceleration += frameAcceleration[i];
            } 

            GameObject obj = GameObject.FindGameObjectWithTag("Player");                        //Get the gameobject to get the rotation angles

            if (!AccelerationWritten)
            {
                AccelerationWritten = WriteAcceleration(obj.transform.rotation.x.ToString(), 
                    obj.transform.rotation.y.ToString(),
                    (acceleration / frameAcceleration.Count).ToString());                       //Write the data to the file
            }

            return true;
        }

        //This function is to write the data to the files for further usage
        //The data form is:
        //Rotation angle of x axis
        //Rotation angle of y axis
        //The average acceleration in 40 frames(or other time gaps)
        bool WriteAcceleration(string xRotation, string yRotation, string acceleration)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            string objName = obj.GetComponent<ObjImporter>().getObjName();
            string fileName = Application.dataPath + "/" + Path.GetFileName(Environment.GetCommandLineArgs()[1]) + ".txt";
            string textToAdd = xRotation + "\n" + yRotation + "\n" + acceleration + "\n";

            FileStream fs = null;

            try
            {

                if (!File.Exists(fileName))
                {
                    File.Create(fileName);
                }
                fs = new FileStream(fileName, FileMode.Append);
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(textToAdd);
                }
            }
            finally
            {
                 
                if (fs != null)
                    fs.Dispose();
            }


            //When all angles were tested(25 angles) for an object,
            //the program will regather all the data from the output file
            //and form them into a list of Vector3 for database reading and analyzing
            if (SceneManager.GetActiveScene().buildIndex == 24)
            {
                GenerateList(fileName);                         //Call the function to generate a list of Vector3.

/*                 for (int i = 0; i < OutputList.Count; i++)
                {
                    Debug.Log(OutputList[i]);  
                } */
            }

             return true;
        }

        //This function is to generate a list of Vector3 for further usage.
        //As the rotation angles and accelerations are relatively small numbers,
        //while the Vector3 contains 3 float numbers with 1 degree accuracy,
        //The numbers are multiplied by 1000000000. This is also our standard of mass of objects.
        //Hence the final output of accelerations are numerically the same as drag forces.
        private void GenerateList(string fileName)
        {
            StreamReader stream = File.OpenText(fileName);
            string entireText = stream.ReadToEnd();
            stream.Close();
            using (StringReader reader = new StringReader(entireText))
            {
                string currentText = reader.ReadLine();
                float xRotation;
                float yRotation;
                float acceleration;
                do
                {
                    xRotation = float.Parse(currentText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat) * 1000000000;


                    currentText = reader.ReadLine();
                    yRotation = float.Parse(currentText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat) * 1000000000;

                    currentText = reader.ReadLine();
                    acceleration = float.Parse(currentText, System.Globalization.CultureInfo.InvariantCulture.NumberFormat) * 1000000000;

                    Vector3 temp = new Vector3(xRotation, yRotation, acceleration);

                    OutputList.Add(temp);

                    currentText = reader.ReadLine();

                }while(currentText != null);
            }

            //Write object results to database
            //writeObjectRecord();

            Application.Quit();
        }

        void writeObjectRecord()
        {
            string localDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DARPA_RUNS");
            string dbPath = Path.Combine(localDataDirectory, "run-data.db");
            string connStr = @"URI=file:" + dbPath;
            SQLiteConnection dbConn = new SQLiteConnection(connStr);
            dbConn.Open();
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            string objName = Environment.GetCommandLineArgs()[1];
            string objResultsInsertSql = "";
            foreach(Vector3 angle in OutputList)
            {
                SQLiteCommand insertThisAngleSql = dbConn.CreateCommand();
                insertThisAngleSql.CommandText = "INSERT INTO results (OBJ_NAME, ROT_X, ROT_Y, ACCEL) VALUES ('" + objName + "', " + angle.x + ", " + angle.y + ", " + angle.z + ");";
                insertThisAngleSql.ExecuteNonQuery();
                Debug.Log(objResultsInsertSql);
            }
            dbConn.Close();
        }

        static int sm_nextGroup = 0;


        Vector3 m_teleportPosition = Vector3.zero;
        Quaternion m_teleportRotation = Quaternion.identity;
        int m_teleport = 0;
        struct ImpulseInfo { public Vector3 impulse; public int particle; }
        List<ImpulseInfo> m_impulses = new List<ImpulseInfo>();

        List<Vector3> OutputList = new List<Vector3>();

        bool AccelerationWritten = false;

        List<float> frameVelocity = new List<float>();

        [NonSerialized]
        int[] m_indices = new int[0];
        [NonSerialized]
        int m_indexCount = 0;
        [NonSerialized]
        _auxFlexDrawParticles m_drawParticlesHelper;
        [NonSerialized]
        Bounds m_bounds = new Bounds();

        FlexExt.Instance.Handle m_instanceHandle;

        [SerializeField]
        FlexContainer m_container;
        [SerializeField]
        int m_particleGroup = -1;
        [SerializeField]
        bool m_selfCollide = true;
        [SerializeField]
        bool m_selfCollideFilter = false;
        [SerializeField]
        bool m_fluid = false;
        [SerializeField]
        float m_massScale = 1.0f;
        [SerializeField]
        bool m_drawParticles = false;

        [SerializeField]
        int m_recreateActor = 0; // 0 - nothing, 1 - re-create instance, 2 - re-add to container

        #endregion
    }
}
