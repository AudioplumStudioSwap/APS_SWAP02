using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class EntryPointInspector
	//!
	//! @brief Inspector of class EntryPoint
	[CustomEditor(typeof(EntryPoint))]
	public class EntryPointInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			SerializedProperty sceneNameProperty = serializedObject.FindProperty("m_startScene");
			
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
			
			serializedObject.ApplyModifiedProperties();
		}
	}
}