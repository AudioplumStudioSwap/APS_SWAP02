using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class ShakeComponentInspector
	//!
	//! @brief Custom Inspector of class ShakeComponent
	[CustomEditor(typeof(ShakeComponentBase), true)]
	public class ShakeComponentInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(m_playOnStartProperty);

			EditorGUILayout.PropertyField(m_translationAxisProperty);
			EditorGUILayout.PropertyField(m_rotationAxisProperty);

			for(int translationDegreeOfFreedom = 0; translationDegreeOfFreedom < 3; ++translationDegreeOfFreedom)
			{
				bool display = (m_translationAxisProperty.intValue & (1 << translationDegreeOfFreedom)) != 0;

				if(display)
				{
					int realIndex = translationDegreeOfFreedom;
					GUIContent propertyName = new GUIContent(m_translationAxisProperty.enumNames[translationDegreeOfFreedom]);

					SerializedProperty parameterProperty = m_parameterArrayProperty.GetArrayElementAtIndex(realIndex);
					EditorGUILayout.PropertyField(parameterProperty, propertyName, true);
				}
			}

			for(int rotationDegreeOfFreedom = 0; rotationDegreeOfFreedom < 3; ++rotationDegreeOfFreedom)
			{
				bool display = (m_rotationAxisProperty.intValue & (1 << rotationDegreeOfFreedom)) != 0;
				
				if(display)
				{
					int realIndex = rotationDegreeOfFreedom + 3;
					GUIContent propertyName = new GUIContent(m_rotationAxisProperty.enumNames[rotationDegreeOfFreedom]);
					
					SerializedProperty parameterProperty = m_parameterArrayProperty.GetArrayElementAtIndex(realIndex);
					EditorGUILayout.PropertyField(parameterProperty, propertyName, true);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

#region Private
	#region Methods
		void OnEnable()
		{
			m_playOnStartProperty = serializedObject.FindProperty("m_playOnStart");

			m_translationAxisProperty = serializedObject.FindProperty("m_translationAxis");
			m_rotationAxisProperty = serializedObject.FindProperty("m_rotationAxis");

			m_parameterArrayProperty = serializedObject.FindProperty("m_parameters");
		}
	#endregion

	#region Attributes
		SerializedProperty m_playOnStartProperty;

		SerializedProperty m_translationAxisProperty;
		SerializedProperty m_rotationAxisProperty;

		SerializedProperty m_parameterArrayProperty;
	#endregion
#endregion
	}
}