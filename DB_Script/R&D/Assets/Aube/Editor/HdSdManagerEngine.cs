using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System;

//////////////////////////////////////////////////////////////////////////
// Copyright © 2010-2014 Artefacts Studio, all rights reserved.
//////////////////////////////////////////////////////////////////////////

namespace Aube
{
    [ExecuteInEditMode]  
    public static class HdSdManagerEngine
    {
        //Edit this to change the name of the lightmaps folder
        public static string ms_lightMapsPath = "3DAssets/LightMaps/";
        //Edit this to change the name of the gameObject for SD&HD version in all scene
        public static string ms_nameGameObjectPropsHdSdContainer = "_SetGeometry";
        public static string ms_nameGameObjectPropsHd = "Props_HD";
        public static string ms_nameGameObjectPropsSd = "Props_SD";
        //Edit this to change the default resolution for the lightmaps (load if no resolution are saved for the scene) 
        public static int ms_lightmapHdResolutionDefault = 50;
        public static int ms_lightmapSdResolutionDefault = 30;
        //Name contains in the gameobject, we need to switch to static for the baking 
        public static string ms_nameGameObjectDynamicToBake = "Bake_";

        //Edit this to change the name of the shader to load in HD/SD ("" or null if we don't need to load a shader)
        public static string ms_shaderName = "ATF_Outline_Rim_Lightmap";
        public static string ms_shaderName2 = "ATF_Reflect-Diffuse-Vert";

        //********************************************************************************************
        // Version Manager
        //********************************************************************************************
        //call this to bake all scene in the game (lightmaps parameters must have been set previously and save for each scene)
        public static void BakeAllScene()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            Debug.Log("Start pass for lightmaps baking from the scene index " + arguments[arguments.Length - 3] + ", bake HD = "+ arguments[arguments.Length - 2] +", bake SD = "+ arguments[arguments.Length - 1] + " at " + System.DateTime.Now);
            BakeScene(int.Parse(arguments[arguments.Length - 3]), Convert.ToBoolean(arguments[arguments.Length - 2]), Convert.ToBoolean(arguments[arguments.Length - 1]));
        }

        //Call this to switch all scenes to HD/SD
        public static void SwitchAllScenesToHDCommand()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            SwitchAllScenesToHD(Convert.ToBoolean(arguments[arguments.Length - 1]));
        }

        //Open all the scene for the build, load lightmaps and delete other version gameObject
        public static void SwitchAllScenesToHD(bool isHDBuild)
        {
            if (isHDBuild)
            {
                Debug.Log("Start clean version for build HD at " + System.DateTime.Now);
                ShaderHdSdManager.SwitchShaderToHd(ms_shaderName);
                ShaderHdSdManager.SwitchShaderToHd(ms_shaderName2);
            }
            else
            {
                Debug.Log("Start clean version for build SD at " + System.DateTime.Now);
                ShaderHdSdManager.SwitchShaderToSd(ms_shaderName);
                ShaderHdSdManager.SwitchShaderToSd(ms_shaderName2);
            }
            List<string> BuildScenes = HdSdUtils.FindEnabledEditorScenes();
            for (int i = 0; i < BuildScenes.Count; i++)
            {
                EditorApplication.OpenScene(BuildScenes[i]);
                PrepareSceneForBuild(isHDBuild);
                EditorApplication.SaveScene();
            }
            Debug.Log("End clean version for build at " + System.DateTime.Now);

        }

        //des/activate props and load lightmaps parameters before bake
        static void SwitchVersion(bool switchToHD)
        {
            GameObject _SetGeometry = GameObject.Find(ms_nameGameObjectPropsHdSdContainer);
            GameObject Props_HD = null;
            GameObject Props_SD = null;

            if (_SetGeometry != null)
            {
                //use a custom find function to find inactive gameObject
                Props_HD = HdSdUtils.Find(_SetGeometry, ms_nameGameObjectPropsHd);
                Props_SD = HdSdUtils.Find(_SetGeometry, ms_nameGameObjectPropsSd);
            }

            if (Props_HD != null)
                Props_HD.SetActive(switchToHD);

            if (Props_SD != null)
                Props_SD.SetActive(!switchToHD);

            UpdateLightmapsParameters(switchToHD);
        }

        //delete gameobject for other version and load the lightmap for the current version
        static void PrepareSceneForBuild(bool BuildHD)
        {
            GameObject _SetGeometry = GameObject.Find(ms_nameGameObjectPropsHdSdContainer);
            GameObject Props_HD = null;
            GameObject Props_SD = null;

            if (_SetGeometry != null)
            {
                //use a custom find function to find inactive gameObject
                Props_HD = HdSdUtils.Find(_SetGeometry, ms_nameGameObjectPropsHd);
                Props_SD = HdSdUtils.Find(_SetGeometry, ms_nameGameObjectPropsSd);
            }

            if (Props_HD != null)
                Props_HD.SetActive(BuildHD);
            if (Props_SD != null)
                Props_SD.SetActive(!BuildHD);

            if (BuildHD && Props_SD != null)
                GameObject.DestroyImmediate(Props_SD);

            if (!BuildHD && Props_HD != null)
                GameObject.DestroyImmediate(Props_HD);

            LoadLightMaps(BuildHD);
        }

        static void BakeScene(int sceneIndex, bool bakeHdVersion, bool bakeSdVersion)
        {
            int[] resolutionParameters = new int[]{};
            List<string> BuildScenes = HdSdUtils.FindEnabledEditorScenes();
            if (sceneIndex < BuildScenes.Count)
            {
                EditorApplication.OpenScene(BuildScenes[sceneIndex]);
                SwitchVersion(bakeHdVersion);
                //Change dynamic object with ms_nameGameObjectDynamicToBake to static for the bake
                GameObjectStateUtils.DynamicToStatic(ms_nameGameObjectPropsHdSdContainer, ms_nameGameObjectDynamicToBake, true);
                resolutionParameters = GetLightmapsParametersToDisplay();
                if (resolutionParameters[0] == 0 && resolutionParameters[1] == 0)
                    Debug.LogWarning("Start bake for the scene " + HdSdUtils.GetNameOfTheCurrentScene() + " WITH A NULL HD RESOLUTION AND A NULL SD RESOLUTION, scene number  " + (sceneIndex + 1) + "/" + BuildScenes.Count + "  at " + System.DateTime.Now);
                else
                    Debug.Log("Start bake for the scene " + HdSdUtils.GetNameOfTheCurrentScene() + " with a HD res of " + resolutionParameters[0] + " and a SD res of " + resolutionParameters[1] + ", scene number  " + (sceneIndex + 1) + "/" + BuildScenes.Count + "  at " + System.DateTime.Now);
                if (!Lightmapping.isRunning)
                    Lightmapping.BakeAsync();
                else
                    Debug.LogError("Error Lightmap is already baking");

                StaticEditorModeCoroutine.StartCoroutine(WaitBakeEnd(sceneIndex, bakeHdVersion, bakeSdVersion, bakeHdVersion));
            }
            else
            {
                Debug.Log("End of baking at " + System.DateTime.Now);
                //Exit manually Unity
                EditorApplication.Exit(0);
            }
        }

        static IEnumerator WaitBakeEnd(int nextSceneIndex, bool bakeHdVersion, bool bakeSdVersion, bool currentVersionIsHD)
        {
            while (Lightmapping.isRunning)
            {
                yield return null;
            }
            if ((bakeSdVersion && !currentVersionIsHD) || !bakeSdVersion)
            {
                SaveCurrentLightmapping(currentVersionIsHD);
                LoadLightMaps(currentVersionIsHD);
                //Change dynamic object with ms_nameGameObjectDynamicToBake to dynamic after the bake
                GameObjectStateUtils.DynamicToStatic(ms_nameGameObjectPropsHdSdContainer, ms_nameGameObjectDynamicToBake, false);
                EditorApplication.SaveScene();
                BakeScene(nextSceneIndex + 1, bakeHdVersion, bakeSdVersion);
            }
            else if (bakeHdVersion && bakeSdVersion && currentVersionIsHD)
            {
                //Change dynamic object with ms_nameGameObjectDynamicToBake to dynamic after the bake
                GameObjectStateUtils.DynamicToStatic(ms_nameGameObjectPropsHdSdContainer, ms_nameGameObjectDynamicToBake, false);
                SaveCurrentLightmapping(currentVersionIsHD);
                SwitchVersion(!currentVersionIsHD);
                //Change dynamic object with ms_nameGameObjectDynamicToBake to static for the bake
                GameObjectStateUtils.DynamicToStatic(ms_nameGameObjectPropsHdSdContainer, ms_nameGameObjectDynamicToBake, true);
                if (!Lightmapping.isRunning)
                    Lightmapping.BakeAsync();
                else
                    Debug.LogError("Error Lightmap is already baking");
                StaticEditorModeCoroutine.StartCoroutine(WaitBakeEnd(nextSceneIndex, bakeHdVersion, bakeSdVersion, false));
            }
            yield break;
        }

        //********************************************************************************************
        // LightMaps Parameters
        //********************************************************************************************
        private static LightMapsParameters LoadLightsParameters()
        {
            LightMapsParameters sceneLightMapsParameters = new LightMapsParameters();
            string path = Application.dataPath + "/" + ms_lightMapsPath + HdSdUtils.GetNameOfTheCurrentScene() + "/";
            HdSdUtils.ExistOrCreateDirectory(path);
            if (File.Exists(path + "LightMapsParameters.xml"))
            {
                object obj = null;
                Stream ms = File.Open(path + "LightMapsParameters.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(LightMapsParameters));
                obj = x.Deserialize(ms);
                ms.Close();

                if (obj != null)
                    sceneLightMapsParameters = (LightMapsParameters)obj;
            }
            else
            {
                LightMapsParameters myGameParameters = LoadGameParameters();
                sceneLightMapsParameters.m_hdResolution = myGameParameters.m_hdResolution;
                sceneLightMapsParameters.m_sdResolution = myGameParameters.m_sdResolution;
            }
            return sceneLightMapsParameters;
        }

        private static void UpdateLightmapsParameters(bool HDRes)
        {
            LightMapsParameters sceneLightMapsParameters = LoadLightsParameters();
            if (HDRes)
                LightmapEditorSettings.resolution = sceneLightMapsParameters.m_hdResolution;
            else
               LightmapEditorSettings.resolution = sceneLightMapsParameters.m_sdResolution;
        }

        private static int[] GetLightmapsParametersToDisplay()
        {
            LightMapsParameters sceneLightMapsParameters = LoadLightsParameters();
            return new int[] {sceneLightMapsParameters.m_hdResolution, sceneLightMapsParameters.m_sdResolution};
        }

        private static LightMapsParameters LoadGameParameters()
        {
            string path = Application.dataPath + "/" + ms_lightMapsPath;
            LightMapsParameters gameParameters = new LightMapsParameters();
            if (File.Exists(path + "LightMapsDefaultParameters.xml"))
            {
                object obj = null;
                Stream ms = File.Open(path + "LightMapsDefaultParameters.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(typeof(LightMapsParameters));
                obj = x.Deserialize(ms);
                ms.Close();

                if (obj != null)
                    gameParameters = (LightMapsParameters)obj;
            }
            else
            {
                gameParameters.m_hdResolution = ms_lightmapHdResolutionDefault;
                gameParameters.m_sdResolution = ms_lightmapSdResolutionDefault;
            }
            return gameParameters;
        }

        //********************************************************************************************
        // LightMaps Loader
        //********************************************************************************************
        static void LoadLightMaps(bool loadHD)
        {
            string fullscenepath = ms_lightMapsPath + HdSdUtils.GetNameOfTheCurrentScene();

            if (loadHD)
                fullscenepath += "/HD";
            else
                fullscenepath += "/SD";

            DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/" + fullscenepath);

            if (dir.Exists)
            {
                AssetDatabase.Refresh();
                LightmapSettings.lightmaps = new LightmapData[0];
                object obj = null;

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


        //********************************************************************************************
        // LightMaps generate
        //********************************************************************************************
        static void GenerateLightMaps(bool generateHD, bool generateSD, int HdRes, int SdRes)
        {
            if (Lightmapping.isRunning)
                Debug.LogWarning("bake is already running");
            else if (generateHD && !generateSD)
            {
                SwitchVersion(true);

                LightmapEditorSettings.resolution = HdRes;
                Debug.Log("bake HD with a " + LightmapEditorSettings.resolution + " resolution");
                Lightmapping.BakeAsync();
            }
            else if (generateSD && !generateHD)
            {
                SwitchVersion(false);

                LightmapEditorSettings.resolution = SdRes;
                Debug.Log("bake SD with a " + LightmapEditorSettings.resolution + " resolution");
                Lightmapping.BakeAsync();
            }
            else
            {
                //generate HD and SD
                GenerateLightMaps(true, false, HdRes, SdRes);
                GenerateLightMaps(false, true, HdRes, SdRes);
            }
        }

        static void SaveCurrentLightmapping(bool saveHd)
        {
            IndexManager manager;
            string path = Application.dataPath + "/" + ms_lightMapsPath;
            HdSdUtils.ExistOrCreateDirectory(path);
            string label;

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
                label = "HD";

            else
                label = "SD";

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
        }
    }
}
