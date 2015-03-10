using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Aube
{
	[CustomEditor(typeof(StateMachine))]

	//!	@class	StateMachineInspector
	//!
	//!	@brief	Custom inspector for class StateMachine
	public class StateMachineInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorCollection.Option options = EditorCollection.Option.ElementAdd | EditorCollection.Option.ElementRemove | EditorCollection.Option.CollectionLabel;
			EditorCollection.Show(m_booleanNameArrayProperty, options, null, OnBooleanAdded, OnBooleanRemoved, OnBooleanGUI);
			EditorCollection.Show(m_integerNameArrayProperty, options, null, OnIntegerAdded, OnIntegerRemoved, OnIntegerGUI);
			EditorCollection.Show(m_floatNameArrayProperty, options, null, OnFloatAdded, OnFloatRemoved, OnFloatGUI);
			EditorCollection.Show(m_triggerNameArrayProperty, options, null, OnTriggerAdded, OnTriggerRemoved, OnTriggerGUI);

			serializedObject.ApplyModifiedProperties();
		}
		
#region Unity Callbacks
		private void OnEnable()
		{
			m_booleanNameArrayProperty = serializedObject.FindProperty("m_booleanNames");
			m_booleanHashedNameArrayProperty = serializedObject.FindProperty("m_hashedBooleanNames");
			m_booleanValueArrayProperty = serializedObject.FindProperty("m_booleanValues");

			m_integerNameArrayProperty = serializedObject.FindProperty("m_integerNames");
			m_integerHashedNameArrayProperty = serializedObject.FindProperty("m_hashedIntegerNames");
			m_integerValueArrayProperty = serializedObject.FindProperty("m_integerValues");

			m_floatNameArrayProperty = serializedObject.FindProperty("m_floatNames");
			m_floatHashedNameArrayProperty = serializedObject.FindProperty("m_hashedFloatNames");
			m_floatValueArrayProperty = serializedObject.FindProperty("m_floatValues");

			m_triggerNameArrayProperty = serializedObject.FindProperty("m_triggerNames");
			m_triggerHashedNameArrayProperty = serializedObject.FindProperty("m_hashedTriggerNames");
		}
#endregion
		
#region Private
	#region Methods
		private void OnBooleanAdded(int a_index, SerializedProperty a_property)
		{
			m_booleanHashedNameArrayProperty.InsertArrayElementAtIndex(a_index);

			m_booleanValueArrayProperty.InsertArrayElementAtIndex(a_index);
			SerializedProperty newBooleanProperty = m_booleanValueArrayProperty.GetArrayElementAtIndex(a_index);
			newBooleanProperty.boolValue = false;
		}

		private void OnBooleanRemoved(int a_index, SerializedProperty a_property)
		{
			m_booleanHashedNameArrayProperty.DeleteArrayElementAtIndex(a_index);
			m_booleanValueArrayProperty.DeleteArrayElementAtIndex(a_index);
		}

		private void OnBooleanGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PropertyField(a_property, GUIContent.none);
				SerializedProperty booleanProperty = m_booleanValueArrayProperty.GetArrayElementAtIndex(a_index);
				EditorGUILayout.PropertyField(booleanProperty, GUIContent.none, GUILayout.Width(50.0f));

				SerializedProperty booleanHashProperty = m_booleanHashedNameArrayProperty.GetArrayElementAtIndex(a_index);
				booleanHashProperty.intValue = StateMachine.StringToHash(a_property.stringValue);
			}
			EditorGUILayout.EndHorizontal();

			if(a_property.stringValue == string.Empty)
			{
				EditorGUILayout.HelpBox("The name of the parameter above is empty.", MessageType.Warning);
			}
		}

		private void OnIntegerAdded(int a_index, SerializedProperty a_property)
		{
			m_integerHashedNameArrayProperty.InsertArrayElementAtIndex(a_index);

			m_integerValueArrayProperty.InsertArrayElementAtIndex(a_index);
			SerializedProperty newIntegerProperty = m_integerValueArrayProperty.GetArrayElementAtIndex(a_index);
			newIntegerProperty.intValue = 0;
		}
		
		private void OnIntegerRemoved(int a_index, SerializedProperty a_property)
		{
			m_integerHashedNameArrayProperty.DeleteArrayElementAtIndex(a_index);
			m_integerValueArrayProperty.DeleteArrayElementAtIndex(a_index);
		}
		
		private void OnIntegerGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PropertyField(a_property, GUIContent.none);
				SerializedProperty integerProperty = m_integerValueArrayProperty.GetArrayElementAtIndex(a_index);
				EditorGUILayout.PropertyField(integerProperty, GUIContent.none, GUILayout.Width(50.0f));

				SerializedProperty integerHashProperty = m_integerHashedNameArrayProperty.GetArrayElementAtIndex(a_index);
				integerHashProperty.intValue = StateMachine.StringToHash(a_property.stringValue);
			}
			EditorGUILayout.EndHorizontal();

			if(a_property.stringValue == string.Empty)
			{
				EditorGUILayout.HelpBox("The name of the parameter above is empty.", MessageType.Warning);
			}
		}

		private void OnFloatAdded(int a_index, SerializedProperty a_property)
		{
			m_floatHashedNameArrayProperty.InsertArrayElementAtIndex(a_index);

			m_floatValueArrayProperty.InsertArrayElementAtIndex(a_index);
			SerializedProperty newFloatProperty = m_floatValueArrayProperty.GetArrayElementAtIndex(a_index);
			newFloatProperty.floatValue = 0.0f;
		}
		
		private void OnFloatRemoved(int a_index, SerializedProperty a_property)
		{
			m_floatHashedNameArrayProperty.DeleteArrayElementAtIndex(a_index);
			m_floatValueArrayProperty.DeleteArrayElementAtIndex(a_index);
		}
		
		private void OnFloatGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PropertyField(a_property, GUIContent.none);
				SerializedProperty floatProperty = m_floatValueArrayProperty.GetArrayElementAtIndex(a_index);
				EditorGUILayout.PropertyField(floatProperty, GUIContent.none, GUILayout.Width(50.0f));

				SerializedProperty floatHashProperty = m_floatHashedNameArrayProperty.GetArrayElementAtIndex(a_index);
				floatHashProperty.intValue = StateMachine.StringToHash(a_property.stringValue);
			}
			EditorGUILayout.EndHorizontal();

			if(a_property.stringValue == string.Empty)
			{
				EditorGUILayout.HelpBox("The name of the parameter above is empty.", MessageType.Warning);
			}
		}

		private void OnTriggerAdded(int a_index, SerializedProperty a_property)
		{
			m_triggerHashedNameArrayProperty.InsertArrayElementAtIndex(a_index);
		}
		
		private void OnTriggerRemoved(int a_index, SerializedProperty a_property)
		{
			m_triggerHashedNameArrayProperty.DeleteArrayElementAtIndex(a_index);
		}
		
		private void OnTriggerGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			EditorGUILayout.BeginHorizontal();
			{
                EditorGUILayout.PropertyField(a_property, GUIContent.none);

                SerializedProperty triggerHashProperty = m_triggerHashedNameArrayProperty.GetArrayElementAtIndex(a_index);
				triggerHashProperty.intValue = StateMachine.StringToHash(a_property.stringValue);
			}
			EditorGUILayout.EndHorizontal();
			
			if(a_property.stringValue == string.Empty)
			{
				EditorGUILayout.HelpBox("The name of the parameter above is empty.", MessageType.Warning);
			}
		}
	#endregion

	#region Attributes
		private SerializedProperty m_booleanNameArrayProperty;
		private SerializedProperty m_booleanHashedNameArrayProperty;
		private SerializedProperty m_booleanValueArrayProperty;

		private SerializedProperty m_integerNameArrayProperty;
		private SerializedProperty m_integerHashedNameArrayProperty;
		private SerializedProperty m_integerValueArrayProperty;

		private SerializedProperty m_floatNameArrayProperty;
		private SerializedProperty m_floatHashedNameArrayProperty;
		private SerializedProperty m_floatValueArrayProperty;

		private SerializedProperty m_triggerNameArrayProperty;
		private SerializedProperty m_triggerHashedNameArrayProperty;
	#endregion
#endregion
	}
}
