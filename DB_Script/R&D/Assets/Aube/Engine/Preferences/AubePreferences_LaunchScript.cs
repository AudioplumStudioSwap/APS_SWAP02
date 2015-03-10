using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Aube
{
	public static partial class AubePreferences
	{
#if UNITY_EDITOR
		public static bool alwaysSaveCurrentScene
		{
			get{ return EditorPrefs.GetBool(ms_keyAlwaysSaveCurrentScene, false); }
			set{ EditorPrefs.SetBool(ms_keyAlwaysSaveCurrentScene, value); }
		}

		public static string currentScene
		{
			get{ return EditorPrefs.GetString(ms_keyCurrentScene, null); }
			set{ EditorPrefs.SetString(ms_keyCurrentScene, value); }
		}

		public static string sceneToLaunch
		{
			get{ return EditorPrefs.GetString(ms_keySceneToLaunch, null); }
			set{ EditorPrefs.SetString(ms_keySceneToLaunch, value); }
		}
#endif // UNITY_EDITOR
		
#region Private		
	#region Preferences Keys
#if UNITY_EDITOR
		private static string ms_keyAlwaysSaveCurrentScene = "AUBE/Play/alwaysSaveCurrentScene";
		private static string ms_keyCurrentScene = "AUBE/Play/currentScene";
		private static string ms_keySceneToLaunch = "AUBE/Play/sceneToLaunch";
#endif // UNITY_EDITOR
	#endregion
#endregion
	}
}