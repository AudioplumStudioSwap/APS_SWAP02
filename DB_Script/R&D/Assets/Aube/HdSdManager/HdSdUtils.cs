using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Xml.Serialization;
using System.IO;

//////////////////////////////////////////////////////////////////////////
// Copyright © 2010-2014 Artefacts Studio, all rights reserved.
//////////////////////////////////////////////////////////////////////////

namespace Aube
{

    [System.Serializable]
    [XmlRoot("LightMapsParameters")]
    public class LightMapsParameters
    {
        public int m_hdResolution;
        public int m_sdResolution;

        public LightMapsParameters() { m_hdResolution = 0; m_sdResolution = 0; }
        public LightMapsParameters(int a, int b) { m_hdResolution = a; m_sdResolution = b; }
    }


    [Serializable]
    public class ObjectIndexer
    {

        public int m_id;
        public GameObject m_obj;

        public ObjectIndexer()
        {
        }
        public ObjectIndexer(int id, GameObject o)
        {
            m_id = id;
            m_obj = o;
        }
    }

    [Serializable]
    public static class ResizeArray
    {
        public static void Add(int key, object obj)
        {
            LightmapData[] LMArray = LightmapSettings.lightmaps;
            List<object> newarr = new List<object>();
            for(int i =0; i< LMArray.Length; i++)
            {
                newarr.Add(LMArray[i]);
            }

            if(key >= newarr.Count)
                newarr.Add(obj);
            else
	            newarr[key] = obj;

	         LightmapData[] builtinArray = new LightmapData[newarr.Count];
		         for(int i =0 ; i<newarr.Count ; i++)
		         {
		            LightmapData item = newarr[i] as LightmapData;
		            builtinArray[i] = item;
		        }
	        LightmapSettings.lightmaps = builtinArray;
	    }
    }

    public static class HdSdUtils
    {
#if UNITY_EDITOR
        public static string GetNameOfTheCurrentScene()
        {
            string scene = EditorApplication.currentScene;
            int startLetter, endletter;

            if (scene != "")
            {

                for (endletter = scene.Length - 1; endletter >= 0; --endletter)
                {
                    if (scene[endletter] == '.')
                    {
                        break;
                    }
                }
                for (startLetter = endletter; startLetter >= 0; --startLetter)
                {
                    if (scene[startLetter] == '/')
                    {
                        ++startLetter;
                        break;
                    }
                }
                scene = scene.Remove(endletter);
                scene = scene.Remove(0, startLetter);

                return scene;
            }
            else
                return "";
        }
#endif

        public static GameObject Find(GameObject parent, string nameToFind)
        {
            GameObject result = null;
            result = GameObject.Find(nameToFind);

            if (result == null)
            {
                int childCount = parent.transform.childCount;
                for (int i = 0; i < childCount; ++i)
                {
                    Transform child = parent.transform.GetChild(i);
                    if (child.gameObject.name == nameToFind)
                        return child.gameObject;
                }
                return null;
            }
            else
            {
                return result;
            }
        }
#if UNITY_EDITOR
        public static List<string> FindEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                EditorScenes.Add(scene.path);
            }
            return EditorScenes;
        }
#endif

        public static void ExistOrCreateDirectory(string path)
        {
            if(!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}