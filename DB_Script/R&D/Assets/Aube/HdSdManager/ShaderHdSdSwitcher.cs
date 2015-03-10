#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

//////////////////////////////////////////////////////////////////////////
// Copyright © 2010-2014 Artefacts Studio, all rights reserved.
//////////////////////////////////////////////////////////////////////////

namespace Aube
{

    [ExecuteInEditMode]  
    public static class ShaderHdSdManager 
    {      
        public static void SwitchShaderToHd(string shaderName)
        {
            if(shaderName != "" && shaderName != null)
                SwitchShader(shaderName, true);
        }

        public static void SwitchShaderToSd(string shaderName)
        {
            if (shaderName != "" && shaderName != null)
                SwitchShader(shaderName, false);
        }

        static void SwitchShader(string shaderName, bool switchToHd)
        {
            string path = Application.dataPath +"/Shaders/";
            if (File.Exists(path + shaderName + ".shader"))
            {
                if (!switchToHd && File.Exists(path + shaderName + "_Sd.shader"))
                {
                    Debug.Log("Change shader " + shaderName + " to SD version!");
                    File.Delete(path + shaderName + ".shader");
                    File.Copy(path + shaderName + "_Sd.shader", path + shaderName + ".shader");
                    AssetDatabase.Refresh();
                }
                else if (switchToHd && File.Exists(path + shaderName + "_Hd.shader"))
                {
                    Debug.Log("Change shader " + shaderName + " to HD version!");
                    File.Delete(path + shaderName + ".shader");
                    File.Copy(path + shaderName + "_Hd.shader", path + shaderName + ".shader");
                    AssetDatabase.Refresh();
                }
                else
                    Debug.LogWarning("Shader " + shaderName + "_Hd.shader ou " + shaderName + "_Sd.shader not found in the Shader Folder!");
            }
            else
                Debug.LogWarning("Shader " + shaderName + ".shader not found in the Shader Folder!");

        }

        
    }
}
#endif
