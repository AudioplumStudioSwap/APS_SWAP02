using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Aube
{
    //! @class AndroidSDKFolder
    //!
    //! @brief Unity Fix - Function used by Unity to find Android-sdk path when building with Jenkins. 
    public static class AndroidSDKFolder
    {
        public static string Path
        {
            get { return EditorPrefs.GetString("AndroidSdkRoot"); }
            set { EditorPrefs.SetString("AndroidSdkRoot", value); }
        }
    }

    //! @class GenericBuildMenu
    //!
    //! @brief Building menu on platforms: Android, iPhone, Linux, Mac OS X, Win32, Win64 and Windows Phone 8
    //! Used by Jenkins for automatic builds.
    class GenericBuildMenu {

        static string[] m_allScenes         = FindEnabledEditorScenes();
        static string   m_applicationName   = PlayerSettings.productName;
        static string   m_buidTargetPath    = "D:/Jenkins/building_in_progress/" + m_applicationName;

        [MenuItem("Aube/Build/Android")]
        static void PerformAndroidBuild()
        {
            string executable = m_applicationName + ".apk";
			AndroidSDKFolder.Path = "C:/Android/adt-bundle-windows-x86-20140702/sdk";
            GenericBuild("/android/" + executable, BuildTarget.Android, BuildOptions.None);
        }

        [MenuItem("Aube/Build/iPhone")]
        static void PerformiPhoneBuild()
        {
            string executable = m_applicationName + ".ipa";
            GenericBuild("/ios/" + executable, BuildTarget.iPhone, BuildOptions.None);
        }

        [MenuItem("Aube/Build/Linux")]
        static void PerformLinuxBuild()
        {
            string executable = m_applicationName + ".app";
            GenericBuild("/linux/" + executable, BuildTarget.StandaloneLinux, BuildOptions.None);
        }

        [MenuItem("Aube/Build/Mac OS X")]
        static void PerformMacOSXBuild()
        {
            string executable = m_applicationName + ".app";
            GenericBuild("/osx/" + executable, BuildTarget.StandaloneOSXIntel, BuildOptions.None);
        }

        [MenuItem("Aube/Build/Windows 32 bits")]
        static void PerformWin32Build()
        {
            string executable = m_applicationName + ".exe";
            GenericBuild("/win32/" + executable, BuildTarget.StandaloneWindows, BuildOptions.None);
        }

        [MenuItem("Aube/Build/Windows 32 bits - development")]
        static void PerformWin32DevelopmentBuild()
        {
            string executable = m_applicationName + ".exe";
            GenericBuild("/win32-dev/" + executable, BuildTarget.StandaloneWindows, BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        [MenuItem("Aube/Build/Windows 64 bits")]
        static void PerformWin64Build()
        {
            string executable = m_applicationName + ".exe";
            GenericBuild("/win64/" + executable, BuildTarget.StandaloneWindows64, BuildOptions.None);
        }

        [MenuItem("Aube/Build/Windows Phone 8")]
        static void PerformWP8Build()
        {
            string executable = m_applicationName + ".exe";
            GenericBuild("/wp8/" + executable, BuildTarget.WP8Player, BuildOptions.None);
        }

        private static string[] FindEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                EditorScenes.Add(scene.path);
            }
            return EditorScenes.ToArray();
        }

        static void GenericBuild(string target_dir, BuildTarget build_target, BuildOptions build_options)
        {
            UnityEngine.Debug.Log("GenericBuild - Building project " + m_applicationName + " to " + m_buidTargetPath + target_dir);
            EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
			string outputPath = m_buidTargetPath + target_dir;
			if(!Directory.Exists(outputPath))
			{    
				Directory.CreateDirectory(outputPath);
			}
			string res = BuildPipeline.BuildPlayer(m_allScenes, outputPath, build_target, build_options);
            if (res.Length > 0)
            {
                throw new Exception("BuildPlayer failure: " + res);
            }
        }
    }
} // namespace Aube