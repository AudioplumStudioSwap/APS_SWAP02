using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System;

//////////////////////////////////////////////////////////////////////////
// Copyright © 2010-2014 Artefacts Studio, all rights reserved.
//////////////////////////////////////////////////////////////////////////

namespace Aube
{
    [ExecuteInEditMode()]
    public class HdSdManagerEditor : EditorWindow
    {

        private GUILayoutOption m_smallUI;
        private GUILayoutOption m_midUI;

        private LightMapsParameters m_lightMapsParameters;

        private bool m_isCurrentVersionHD = false;
        private bool m_isBaking = false;
        private bool m_sdBakeIsWaiting = false;
        private bool m_isHdBaking = true;

        private string m_lightMapsPath = HdSdManagerEngine.ms_lightMapsPath;

        private DateTime m_timer = DateTime.Now;

        private string m_currentScene = "";

        //Game default Parameters
        private LightMapsParameters m_lightmapsDefaultResolution;

        private HdSdManagerEditor()
        {
            m_lightmapsDefaultResolution = new LightMapsParameters();
            m_lightMapsParameters = new LightMapsParameters();
            
        }

        [MenuItem("Aube/HD and SD Manager")]
        private static void Init()
        {
            HdSdManagerEditor lightMapMgr = EditorWindow.GetWindow<HdSdManagerEditor>();
            lightMapMgr.title = "HD & SD Manager";
            lightMapMgr.Show();
        }

        void OnEnable()
        {

            m_isBaking = false;
            m_sdBakeIsWaiting = false;
            m_isHdBaking = true;
            m_isCurrentVersionHD = false;
            LoadGameParameters();
            LoadLightsParameters();
            FindCurrentVersionInScene();
        }

        void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                if (m_currentScene != HdSdUtils.GetNameOfTheCurrentScene())
                {
                    LoadLightsParameters();
                    m_currentScene = HdSdUtils.GetNameOfTheCurrentScene();
                    FindCurrentVersionInScene();
                }

                if (m_isBaking && !Lightmapping.isRunning)
                {
                    m_isBaking = false;
                    SaveCurrentLightmapping(m_isHdBaking);
                    //Change dynamic object with ms_nameGameObjectDynamicToBake to dynamic after the bake
                    GameObjectStateUtils.DynamicToStatic(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer, HdSdManagerEngine.ms_nameGameObjectDynamicToBake, false);
                    Debug.Log("Bake time = " + (DateTime.Now - m_timer).TotalSeconds + " seconds");
                    //launch SD bake if we ask to generate both lightmaps
                    if (m_sdBakeIsWaiting)
                    {
                        //Change dynamic object with ms_nameGameObjectDynamicToBake to dynamic after the bake
                        GameObjectStateUtils.DynamicToStatic(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer, HdSdManagerEngine.ms_nameGameObjectDynamicToBake, false);
                        m_sdBakeIsWaiting = false;
                        GenerateLightMaps(false, true);
                    }
                }
            }
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("DEFAULT PARAMETERS", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Default LightMaps Resolution Parameters", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            m_lightmapsDefaultResolution.m_hdResolution = EditorGUILayout.IntField("Default HD resolution", m_lightmapsDefaultResolution.m_hdResolution);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_lightmapsDefaultResolution.m_sdResolution = EditorGUILayout.IntField("Default SD resolution", m_lightmapsDefaultResolution.m_sdResolution);

            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Default Parameters"))
                SaveDefaultResolutionParameters();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Shader Changer", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load HD Shader"))
                ShaderHdSdManager.SwitchShaderToHd(HdSdManagerEngine.ms_shaderName);

            if (GUILayout.Button("Load SD Shader"))
                ShaderHdSdManager.SwitchShaderToSd(HdSdManagerEngine.ms_shaderName);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("CURRENT SCENE PARAMETERS---------------------------------", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("LightMaps Resolution Parameters", EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            m_lightMapsParameters.m_hdResolution = EditorGUILayout.IntField("HD resolution", m_lightMapsParameters.m_hdResolution);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_lightMapsParameters.m_sdResolution = EditorGUILayout.IntField("SD resolution", m_lightMapsParameters.m_sdResolution);

            EditorGUILayout.EndHorizontal();

            // UI Buttons

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Parameters"))
                SaveLightMapsParameters();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("LightMaps Baking", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Bake HD and SD LigtMaps"))
                GenerateLightMaps(true, true);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Bake HD LigtMaps"))
                GenerateLightMaps(true, false);

            if (GUILayout.Button("Bake SD LigtMaps"))
                GenerateLightMaps(false, true);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Bake HD and SD Selected"))
                GenerateSelectedLightmaps(true, true);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Bake HD Selected"))
                GenerateSelectedLightmaps(true, false);

            if (GUILayout.Button("Bake SD Selected"))
                GenerateSelectedLightmaps(false, true);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Cancel Bake"))
                Lightmapping.Cancel();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("LightMaps Loader", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load HD lightmaps"))
                LoadLightMaps(true);

            if (GUILayout.Button("Load SD lightmaps"))
                LoadLightMaps(false);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.LabelField("LightMaps Switcher", EditorStyles.boldLabel);
            if (m_isCurrentVersionHD)
                EditorGUILayout.LabelField("Current version is in HD", EditorStyles.largeLabel);
            else
                EditorGUILayout.LabelField("Current version is in SD", EditorStyles.largeLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Switch to HD version"))
                SwitchVersion(true);

            if (GUILayout.Button("Switch to SD version"))
                SwitchVersion(false);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
        }


        private void LoadLightsParameters()
        {
            string path = Application.dataPath + "/" + m_lightMapsPath + HdSdUtils.GetNameOfTheCurrentScene() + "/";
            if (File.Exists(path + "LightMapsParameters.xml"))
            {
                object obj = null;
                Stream ms = File.Open(path + "LightMapsParameters.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(LightMapsParameters));
                obj = x.Deserialize(ms);
                ms.Close();

                if (obj != null)
                    m_lightMapsParameters = (LightMapsParameters)obj;
            }
            else
            {
                m_lightMapsParameters.m_hdResolution = m_lightmapsDefaultResolution.m_hdResolution;
                m_lightMapsParameters.m_sdResolution = m_lightmapsDefaultResolution.m_sdResolution;
            }
        }

        private void SaveLightMapsParameters()
        {
            string path = Application.dataPath + "/" + m_lightMapsPath;
            HdSdUtils.ExistOrCreateDirectory(path);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += HdSdUtils.GetNameOfTheCurrentScene() + "/";

            if (File.Exists(path + "LightMapsParameters.xml"))
                File.Delete(path + "LightMapsParameters.xml");

            if (!File.Exists(path + "LightMapsParameters.xml") && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            Stream ms = File.OpenWrite(path + "LightMapsParameters.xml");
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(m_lightMapsParameters.GetType());
            x.Serialize(ms, m_lightMapsParameters);
            ms.Close();
        }

        private void LoadGameParameters()
        {
            string path = Application.dataPath + "/" + m_lightMapsPath;
            if (File.Exists(path + "LightMapsDefaultParameters.xml"))
            {
                object obj = null;
                Stream ms = File.Open(path + "LightMapsDefaultParameters.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(LightMapsParameters));
                obj = x.Deserialize(ms);
                ms.Close();

                if (obj != null)
                    m_lightmapsDefaultResolution = (LightMapsParameters)obj;
            }
            else
                m_lightmapsDefaultResolution = new LightMapsParameters(HdSdManagerEngine.ms_lightmapHdResolutionDefault, HdSdManagerEngine.ms_lightmapSdResolutionDefault);
        }

        private void SaveDefaultResolutionParameters()
        {
            string path = Application.dataPath + "/" + m_lightMapsPath;
            HdSdUtils.ExistOrCreateDirectory(path);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(path + "LightMapsDefaultParameters.xml"))
                File.Delete(path + "LightMapsDefaultParameters.xml");

            if (!File.Exists(path + "LightMapsDefaultParameters.xml") && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            Stream ms = File.OpenWrite(path + "LightMapsDefaultParameters.xml");
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(m_lightmapsDefaultResolution.GetType());
            x.Serialize(ms, m_lightmapsDefaultResolution);
            ms.Close();
        }


        //********************************************************************************************
        // LightMaps manager
        //********************************************************************************************

        private void LoadLightMaps(bool loadHD)
        {
            string fullscenepath = m_lightMapsPath + HdSdUtils.GetNameOfTheCurrentScene();

            if (loadHD)
                fullscenepath += "/HD";
            else
                fullscenepath += "/SD";

            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + fullscenepath);

            AssetDatabase.Refresh();
            LightmapSettings.lightmaps = new LightmapData[0];
            object obj = null;

            if (dir.Exists)
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    if (file.Name == "Lightmap.xml")
                    {
                        Stream ms = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(LightmappedScene));
                        obj = x.Deserialize(ms);
                        ms.Close();
                    }

                    if (file.Name.EndsWith(".exr") && !file.Name.EndsWith(".meta"))
                    {
                        AssetDatabase.Refresh();
                        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath("Assets" + "/" + fullscenepath + "/" + file.Name);
                        importer.textureType = TextureImporterType.Lightmap;
                        importer.isReadable = true;
                        importer.lightmap = true;
                        importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                        AssetDatabase.ImportAsset("Assets" + "/" + fullscenepath + "/" + file.Name, ImportAssetOptions.ForceSynchronousImport);
                    }
                    else if (file.Name == "LightProbes.asset")
                    {
                        AssetDatabase.ImportAsset("Assets/" + fullscenepath + "/" + file.Name, ImportAssetOptions.ForceSynchronousImport);
                        LightmapSettings.lightProbes = (LightProbes)AssetDatabase.LoadAssetAtPath("Assets" + "/" + fullscenepath + "/" + file.Name, typeof(LightProbes));
                    }
                }


                if (obj != null)
                {
                    LightmappedScene sceneobj = (LightmappedScene)obj;
                    if (sceneobj != null && sceneobj.Lightmaps != null)
                    {

                        foreach (var lobj in sceneobj.Lightmaps)
                        {

                            LightmapData d = new LightmapData();
                            if (LightmapSettings.lightmapsMode == LightmapsMode.Dual)
                            {
                                d.lightmapFar = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets\\" + fullscenepath + "\\" + lobj.Value.far, typeof(Texture2D));
                                d.lightmapNear = (Texture2D)Resources.LoadAssetAtPath("Assets\\" + fullscenepath + "\\" + lobj.Value.near, typeof(Texture2D));
                            }
                            else if (LightmapSettings.lightmapsMode == LightmapsMode.Single)
                            {
                                d.lightmapFar = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets\\" + fullscenepath + "\\" + lobj.Value.far, typeof(Texture2D));

                            }
                            else if (LightmapSettings.lightmapsMode == LightmapsMode.Directional)
                            {
                                d.lightmapFar = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets\\" + fullscenepath + "\\" + lobj.Value.far, typeof(Texture2D));
                                d.lightmapNear = (Texture2D)Resources.LoadAssetAtPath("Assets\\" + fullscenepath + "\\" + lobj.Value.near, typeof(Texture2D));
                            }

                            if (lobj.Key < LightmapSettings.lightmaps.Length)
                            {
                                LightmapSettings.lightmaps[lobj.Key].lightmapFar = d.lightmapFar;
                                LightmapSettings.lightmaps[lobj.Key].lightmapNear = d.lightmapNear;
                            }
                            else
                            {

                                ResizeArray.Add(lobj.Key, d);
                            }
                        }
                    }

                    IndexManager manager;
                    GameObject indexerGO = GameObject.Find("SceneIndexer");
                    if (indexerGO == null)
                    {
                        indexerGO = new GameObject("SceneIndexer");
                        indexerGO.hideFlags = HideFlags.HideInHierarchy;
                        manager = (IndexManager)indexerGO.AddComponent(typeof(IndexManager));
                    }
                    else
                    {
                        manager = (IndexManager)indexerGO.GetComponent(typeof(IndexManager));
                    }


                    if (manager != null)
                    {
                        foreach (var key in sceneobj.LightmappedObjects.Keys)
                        {
                            ObjectIndexer indexer = manager.FindByID(int.Parse(key.ToString()));
                            if (indexer != null && indexer.m_obj != null)
                            {
                                GameObject LMobj = (GameObject)EditorUtility.InstanceIDToObject(indexer.m_obj.GetInstanceID());
                                if (LMobj != null && LMobj.renderer != null && LMobj.isStatic)
                                {
                                    LMobj.renderer.lightmapIndex = sceneobj.LightmappedObjects[key].mapindex;
                                    LMobj.renderer.lightmapTilingOffset = new Vector4(sceneobj.LightmappedObjects[key].tilex, sceneobj.LightmappedObjects[key].tiley, sceneobj.LightmappedObjects[key].offsetx, sceneobj.LightmappedObjects[key].offsety);
                                }
                            }
                        }
                    }
                }
                AssetDatabase.Refresh();
            }
        }

        private void GenerateLightMaps(bool generateHD, bool generateSD)
        {
            if (Lightmapping.isRunning)
                Debug.LogWarning("bake is already running");
            else if (generateHD && !generateSD)
            {
                if (!m_isCurrentVersionHD)
                    SwitchVersion(true);

                LightmapEditorSettings.resolution = m_lightMapsParameters.m_hdResolution;
                Debug.Log("bake HD with a " + LightmapEditorSettings.resolution + " resolution");
                m_isHdBaking = true;
                m_timer = DateTime.Now;
                //Change dynamic object with ms_nameGameObjectDynamicToBake to static for the bake
                GameObjectStateUtils.DynamicToStatic(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer, HdSdManagerEngine.ms_nameGameObjectDynamicToBake, true);
                Lightmapping.BakeAsync();
                m_isBaking = true;
            }
            else if (generateSD && !generateHD)
            {
                if (m_isCurrentVersionHD)
                    SwitchVersion(false);

                LightmapEditorSettings.resolution = m_lightMapsParameters.m_sdResolution;
                Debug.Log("bake SD with a " + LightmapEditorSettings.resolution + " resolution");
                m_isHdBaking = false;
                m_timer = DateTime.Now;
                //Change dynamic object with ms_nameGameObjectDynamicToBake to static for the bake
                GameObjectStateUtils.DynamicToStatic(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer, HdSdManagerEngine.ms_nameGameObjectDynamicToBake, true);
                Lightmapping.BakeAsync();
                m_isBaking = true;
            }
            else
            {
                //generate HD and SD
                m_sdBakeIsWaiting = true;
                GenerateLightMaps(true, false);
            }
        }


        private void GenerateSelectedLightmaps(bool generateHD, bool generateSD)
        {
            if (Lightmapping.isRunning)
                Debug.LogWarning("bake is already running");
            else if (generateHD && !generateSD)
            {
                if (!m_isCurrentVersionHD)
                    SwitchVersion(true);

                LightmapEditorSettings.resolution = m_lightMapsParameters.m_hdResolution;
                Debug.Log("bake selected HD with a " + LightmapEditorSettings.resolution + " resolution");
                m_isHdBaking = true;
                m_timer = DateTime.Now;
                //Change dynamic object with ms_nameGameObjectDynamicToBake to static for the bake
                GameObjectStateUtils.DynamicToStatic(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer, HdSdManagerEngine.ms_nameGameObjectDynamicToBake, true);
                Lightmapping.BakeSelectedAsync();
                m_isBaking = true;
            }
            else if (generateSD && !generateHD)
            {
                if (m_isCurrentVersionHD)
                    SwitchVersion(false);

                LightmapEditorSettings.resolution = m_lightMapsParameters.m_sdResolution;
                Debug.Log("bake selected SD with a " + LightmapEditorSettings.resolution + " resolution");
                m_isHdBaking = false;
                m_timer = DateTime.Now;
                //Change dynamic object with ms_nameGameObjectDynamicToBake to static for the bake
                GameObjectStateUtils.DynamicToStatic(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer, HdSdManagerEngine.ms_nameGameObjectDynamicToBake, true);
                Lightmapping.BakeSelectedAsync();
                m_isBaking = true;
            }
            else
            {
                //generate HD and SD
                m_sdBakeIsWaiting = true;
                GenerateSelectedLightmaps(true, false);
            }

        }

        private void SaveCurrentLightmapping(bool saveHd)
        {
            IndexManager manager;
            string path = Application.dataPath + "/" + m_lightMapsPath;
            string label;
            HdSdUtils.ExistOrCreateDirectory(path);

            GameObject indexerGO = GameObject.Find("SceneIndexer");
            if (indexerGO == null)
            {
                indexerGO = new GameObject("SceneIndexer");
                indexerGO.hideFlags = HideFlags.HideInHierarchy;
                manager = (IndexManager)indexerGO.AddComponent(typeof(IndexManager));
            }
            else
            {
                manager = (IndexManager)indexerGO.GetComponent(typeof(IndexManager));
            }

            var scenepath = EditorApplication.currentScene;
            string[] pathelements = scenepath.Split(new char[] { '.' });
            string fullscenepath = pathelements[0].Replace("Assets/", string.Empty);
            pathelements = pathelements[0].Split(new char[] { '/' });


            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += HdSdUtils.GetNameOfTheCurrentScene() + "/";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (saveHd)
            {
                Debug.Log("save HD lightmaps in " + path + "HD/");
                label = "HD";
            }
            else
            {
                Debug.Log("save SD lightmaps in " + path + "SD/");
                label = "SD";
            }

            path += label;
            if (Directory.Exists(path))
            {
                string[] listFileToDelete = Directory.GetFiles(path);
                foreach (string pathFile in listFileToDelete)
                {
                    if (!pathFile.Contains("LightMapsParameters"))
                    {
                        File.Delete(pathFile);
                    }
                }
            }
            Directory.CreateDirectory(path);
            DirectoryInfo dirinfo = new DirectoryInfo(Application.dataPath + "/" + fullscenepath);

            LightmappedScene lscene = new LightmappedScene();

            int i = 0;

            for (i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                if (LightmapSettings.lightmapsMode == LightmapsMode.Dual)
                {
                    var lm = new Lightmap("LightmapFar-" + i.ToString() + ".exr", "LightmapNear-" + i.ToString() + ".exr");
                    lscene.Lightmaps.Add(i, lm);
                }
                else if (LightmapSettings.lightmapsMode == LightmapsMode.Single)
                {
                    var lm = new Lightmap("LightmapFar-" + i.ToString() + ".exr", "");
                    lscene.Lightmaps.Add(i, lm);
                }
                else if (LightmapSettings.lightmapsMode == LightmapsMode.Directional)
                {
                    var lm = new Lightmap("LightmapColor-" + i.ToString() + ".exr", "LightmapScale-" + i.ToString() + ".exr");
                    lscene.Lightmaps.Add(i, lm);
                }
            }

            string copyFilePath = "Assets/" + fullscenepath + "/";

            foreach (FileInfo file in dirinfo.GetFiles())
            {
                if (file.Name != "LightProbes.asset" && !file.Name.EndsWith(".meta"))
                {
                    file.CopyTo(path + "/" + file.Name);
                    if (file.Name.EndsWith(".exr"))
                    {
                        AssetDatabase.Refresh();
                        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(copyFilePath + file.Name);
                        importer.textureType = TextureImporterType.Lightmap;
                        importer.isReadable = true;
                        importer.lightmap = true;
                        importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                    }

                }
                else
                {
                    AssetDatabase.CopyAsset(scenepath.Replace(".unity", "") + "/LightProbes.asset", path + "/LightProbes.asset");
                }
            }
            AssetDatabase.Refresh();

            UnityEngine.Object[] activeGOs;

            activeGOs = GameObject.FindObjectsOfType(typeof(MeshRenderer));

            foreach (MeshRenderer activeGO in activeGOs)
            {
                if (activeGO.gameObject.isStatic)
                {
                    ObjectIndexer indexer;
                    Vector4 vec = activeGO.lightmapTilingOffset;

                    indexer = manager.FindByID(activeGO.gameObject.GetInstanceID());

                    if (indexer == null)
                    {
                        indexer = new ObjectIndexer(activeGO.gameObject.GetInstanceID(), activeGO.gameObject);
                        manager.m_sceneObjects.Add(indexer);
                    }

                    if (lscene != null && lscene.LightmappedObjects != null && !lscene.LightmappedObjects.ContainsKey(indexer.m_id))
                        lscene.LightmappedObjects.Add(indexer.m_id, new LightmappedObject(vec.x, vec.y, vec.z, vec.w, activeGO.lightmapIndex));

                }
            }
            Stream ms = File.OpenWrite(path + "/Lightmap.xml");

            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(lscene.GetType());
            x.Serialize(ms, lscene);
            ms.Close();

            Directory.Delete(Application.dataPath + "/" + fullscenepath, true);

            AssetDatabase.Refresh();

            if (!m_sdBakeIsWaiting && !Lightmapping.isRunning)
                LoadLightMaps(m_isCurrentVersionHD);
        }

        //********************************************************************************************
        // Version manager
        //********************************************************************************************

        private void SwitchVersion(bool switchToHD)
        {
            GameObject _SetGeometry = GameObject.Find(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer);
            GameObject Props_HD = null;
            GameObject Props_SD = null;

            if (_SetGeometry != null)
            {
                //use a custom find function to find inactive gameObject
                Props_HD = HdSdUtils.Find(_SetGeometry, HdSdManagerEngine.ms_nameGameObjectPropsHd);
                Props_SD = HdSdUtils.Find(_SetGeometry, HdSdManagerEngine.ms_nameGameObjectPropsSd);
            }

            if (switchToHD)
                LightmapEditorSettings.resolution = m_lightMapsParameters.m_hdResolution;
            else
                LightmapEditorSettings.resolution = m_lightMapsParameters.m_sdResolution;

            if (Props_HD != null)
                Props_HD.SetActive(switchToHD);

            if (Props_SD != null)
                Props_SD.SetActive(!switchToHD);

            m_isCurrentVersionHD = switchToHD;
            LoadLightMaps(switchToHD);
        }
        private void FindCurrentVersionInScene()
        {
            if (m_lightMapsParameters.m_hdResolution != 0 && m_lightMapsParameters.m_sdResolution != 0 && m_lightMapsParameters.m_hdResolution != m_lightMapsParameters.m_sdResolution)
            {
                if (m_lightMapsParameters.m_sdResolution == LightmapEditorSettings.resolution)
                    m_isCurrentVersionHD = false;
                else if (m_lightMapsParameters.m_hdResolution == LightmapEditorSettings.resolution)
                    m_isCurrentVersionHD = true;
            }
            else
            {
                GameObject _SetGeometry = GameObject.Find(HdSdManagerEngine.ms_nameGameObjectPropsHdSdContainer);
                GameObject Props_HD = null;
                GameObject Props_SD = null;

                if (_SetGeometry != null)
                {
                    //use a custom find function to find inactive gameObject
                    Props_HD = HdSdUtils.Find(_SetGeometry, HdSdManagerEngine.ms_nameGameObjectPropsHd);
                    Props_SD = HdSdUtils.Find(_SetGeometry, HdSdManagerEngine.ms_nameGameObjectPropsSd);

                    if (Props_SD != null)
                    {
                        if (Props_SD.activeInHierarchy)
                            m_isCurrentVersionHD = false;
                        else
                            m_isCurrentVersionHD = true;
                    }
                    else if (Props_HD != null)
                    {
                        if (Props_HD.activeInHierarchy)
                            m_isCurrentVersionHD = true;
                        else
                            m_isCurrentVersionHD = false;
                    }
                }
            }
        }

    }
}
