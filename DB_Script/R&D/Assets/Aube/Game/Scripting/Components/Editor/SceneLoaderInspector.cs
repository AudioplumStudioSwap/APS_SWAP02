using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class SceneLoaderInspector
	//!
	//! @brief Inspector of class SceneLoader
	[CustomEditor(typeof(SceneLoader))]
	public class SceneLoaderInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty sceneNameProperty = serializedObject.FindProperty("m_sceneName");

			GUIContent[] sceneNames = new GUIContent[EditorBuildSettings.scenes.Length];
			int selectedIndex = -1;
			for(int sceneIndex = 0; sceneIndex < EditorBuildSettings.scenes.Length; ++sceneIndex)
			{
				string sceneName = System.IO.Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[sceneIndex].path);
				sceneNames[sceneIndex] = new GUIContent(sceneName);

				if(sceneName == sceneNameProperty.stringValue)
				{
					selectedIndex = sceneIndex;
				}
			}

			int newSelectedIndex = EditorGUILayout.Popup(new GUIContent("Scene"), selectedIndex, sceneNames);
			if(newSelectedIndex != selectedIndex)
			{
				sceneNameProperty.stringValue = System.IO.Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[newSelectedIndex].path);
			}

			SerializedProperty load_property = serializedObject.FindProperty("m_loadingScreen");
			EditorGUILayout.PropertyField(load_property);

			serializedObject.ApplyModifiedProperties();
		}
	}
}