#if !AUBE_NO_UI

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	[CustomEditor(typeof(UICredits))]
	public class UICreditsInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(m_fileProperty);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(m_beginPlaceHolderProperty);
			EditorGUILayout.PropertyField(m_endPlaceHolderProperty);

			m_speedFoldout = EditorGUILayout.Foldout(m_speedFoldout, "Speed Configuration");
			if(m_speedFoldout)
			{
				EditorGUILayout.PropertyField(m_enableSpeedChangeProperty);
				if(m_enableSpeedChangeProperty.boolValue)
				{
					EditorGUILayout.PropertyField(m_speedProperty, new GUIContent("default speed"));
					EditorGUILayout.PropertyField(m_speedBoundsProperty);

					if(m_speedBoundsProperty.vector2Value.x > m_speedBoundsProperty.vector2Value.y)
					{
						EditorGUILayout.HelpBox("the minimal speed bound must be lower than the maximal speed bound.", MessageType.Warning);
					}
					else if(m_speedProperty.floatValue < m_speedBoundsProperty.vector2Value.x
						||  m_speedProperty.floatValue > m_speedBoundsProperty.vector2Value.y)
					{
						EditorGUILayout.HelpBox("the default speed value must be inside speed bounds.", MessageType.Warning);
					}

					EditorGUILayout.PropertyField(m_increaseSpeedKeyProperty);
					EditorGUILayout.PropertyField(m_decreaseSpeedKeyProperty);
					EditorGUILayout.PropertyField(m_accelerationProperty);
				}
				else
				{
					EditorGUILayout.PropertyField(m_speedProperty);
				}
			}

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(m_defaultParametersProperty, new GUIContent("Default Parameters"), true);

			EditorCollection.Show(m_identifiersArrayProperty, EditorCollection.Option.Alternative, null, OnIdentifierAdded, OnIdentifierRemoved, OnIdentifierGUI);

			EditorGUILayout.PropertyField(m_endCreditsKeyProperty);
			EditorCollection.Show(m_onEndCreditsEventsProperty, EditorCollection.Option.Alternative, null, OnEndCreditsEventAdded, null);

			serializedObject.ApplyModifiedProperties();
		}

#region Private
	#region Methods
		void OnEnable()
		{
			m_fileProperty = serializedObject.FindProperty("m_creditsFile");

			m_beginPlaceHolderProperty = serializedObject.FindProperty("m_beginPlaceholder");
			m_endPlaceHolderProperty = serializedObject.FindProperty("m_endPlaceHolder");

			m_speedFoldout = false;
			m_enableSpeedChangeProperty = serializedObject.FindProperty("m_enableSpeedChange");
			m_speedProperty = serializedObject.FindProperty("m_speed");
			m_speedBoundsProperty = serializedObject.FindProperty("m_speedBounds");
			m_increaseSpeedKeyProperty = serializedObject.FindProperty("m_increaseSpeedKey");
			m_decreaseSpeedKeyProperty = serializedObject.FindProperty("m_decreaseSpeedKey");
			m_accelerationProperty = serializedObject.FindProperty("m_acceleration");

			m_defaultParametersProperty = serializedObject.FindProperty("m_default");
			m_identifiersArrayProperty = serializedObject.FindProperty("m_identifiers");
			m_parametersArrayProperty = serializedObject.FindProperty("m_idParameters");

			m_endCreditsKeyProperty = serializedObject.FindProperty("m_endCreditsKey");
			m_onEndCreditsEventsProperty = serializedObject.FindProperty("m_onEndCreditsEvents");
		}

		void OnIdentifierAdded(int a_index, SerializedProperty a_property)
		{
			++m_parametersArrayProperty.arraySize;

			SerializedProperty parametersProperty = m_parametersArrayProperty.GetArrayElementAtIndex(a_index);
			SerializedProperty prefabProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_prefab");
			prefabProperty.objectReferenceValue = null;
			SerializedProperty spaceBeforeProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_spaceBefore");
			spaceBeforeProperty.floatValue = 0.0f;
			SerializedProperty spaceAfterProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_spaceAfter");
			spaceAfterProperty.floatValue = 0.0f;

			SerializedProperty poolLengthProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_poolLength");
			poolLengthProperty.intValue = 0;
		}

		void OnIdentifierRemoved(int a_index, SerializedProperty a_property)
		{
			m_parametersArrayProperty.DeleteArrayElementAtIndex(a_index);
		}

		void OnIdentifierGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			EditorGUILayout.PropertyField(a_property, new GUIContent("identifier"));

			SerializedProperty parametersProperty = m_parametersArrayProperty.GetArrayElementAtIndex(a_index);
			SerializedProperty prefabProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_prefab");
			SerializedProperty spaceBeforeProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_spaceBefore");
			SerializedProperty spaceAfterProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_spaceAfter");
			SerializedProperty poolLengthProperty = parametersProperty.serializedObject.FindProperty(parametersProperty.propertyPath + ".m_poolLength");

			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				EditorGUILayout.PropertyField(prefabProperty);
				EditorGUILayout.PropertyField(spaceBeforeProperty);
				EditorGUILayout.PropertyField(spaceAfterProperty);
				EditorGUILayout.PropertyField(poolLengthProperty);
			}
			EditorGUILayout.EndVertical();
		}

		void OnEndCreditsEventAdded(int a_index, SerializedProperty property)
		{
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
			SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
			SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
			objectProperty.objectReferenceValue = (target as UICredits).gameObject;
			componentProperty.objectReferenceValue = null;
			methodNameProperty.stringValue = "";
		}
	#endregion

	#region Attributes
		SerializedProperty m_fileProperty;

		SerializedProperty m_beginPlaceHolderProperty;
		SerializedProperty m_endPlaceHolderProperty;

		SerializedProperty m_enableSpeedChangeProperty;
		SerializedProperty m_speedProperty;
		SerializedProperty m_speedBoundsProperty;
		SerializedProperty m_increaseSpeedKeyProperty;
		SerializedProperty m_decreaseSpeedKeyProperty;
		SerializedProperty m_accelerationProperty;

		SerializedProperty m_defaultParametersProperty;
		SerializedProperty m_identifiersArrayProperty;
		SerializedProperty m_parametersArrayProperty;

		SerializedProperty m_endCreditsKeyProperty;
		SerializedProperty m_onEndCreditsEventsProperty;

		bool m_speedFoldout;
	#endregion
#endregion
	}
}

#endif // !AUBE_NO_UI