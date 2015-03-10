using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Aube
{
	//! @class AubePreferences
	//!
	//! @brief Editor Preferences for Aube
	public static partial class AubePreferences
	{
#region Private
#if UNITY_EDITOR
		[UnityEditor.PreferenceItem("Aube")]
		static void PreferencesGUI()
		{
			ms_launchScriptFoldout ^= GUILayout.Button("Launch Script", EditorStyles.toolbarDropDown);
			if(ms_launchScriptFoldout)
			{
				alwaysSaveCurrentScene = EditorGUILayout.Toggle("Always save current scene", alwaysSaveCurrentScene);
			}
		}
#endif // UNITY_EDITOR

	#region GUI Foldout variables
		private static bool ms_launchScriptFoldout = true;
	#endregion
#endregion
	}
}