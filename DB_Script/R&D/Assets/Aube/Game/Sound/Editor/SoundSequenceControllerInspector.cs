using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class SoundSequenceControllerInspector
	//!
	//! @brief Custom Inspector of a Sound Sequence Controller
	[CustomEditor(typeof(SoundSequenceController))]
	public class SoundSequenceControllerInspector : Editor
	{
		public override void OnInspectorGUI()
		{	
			EditorGUILayout.PropertyField(m_playOnStartProperty);
			EditorGUILayout.PropertyField(m_lastSequencePolicyProperty);
			EditorGUILayout.PropertyField(m_loopPolicyProperty);

			Aube.EditorCollection.Show(m_sequenceArrayProperty, Aube.EditorCollection.Option.Alternative | Aube.EditorCollection.Option.BoxElement, null, OnRandomSoundSequenceAdded, OnRandomSoundSequenceRemoved, OnRandomSoundSequenceGUI);

			EditorCollection.Show(m_onSequenceBeginEventArrayProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);
			EditorCollection.Show(m_onSequenceEndEventArrayProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);

			serializedObject.ApplyModifiedProperties();
		}
		
#region Private
		protected void OnEnable()
		{
			m_playOnStartProperty = serializedObject.FindProperty("m_playOnStart");
			m_lastSequencePolicyProperty = serializedObject.FindProperty("m_lastSequencePolicy");
			m_loopPolicyProperty = serializedObject.FindProperty("m_loopPolicy");

			m_sequenceArrayProperty = serializedObject.FindProperty("m_sequences");
			m_weightArrayProperty = serializedObject.FindProperty("m_weigths");

			m_onSequenceBeginEventArrayProperty = serializedObject.FindProperty("m_onSequenceBeginEvents");
			m_onSequenceEndEventArrayProperty = serializedObject.FindProperty("m_onSequenceEndEvents");
		}

		void OnRandomSoundSequenceAdded(int a_index, SerializedProperty a_property)
		{
			SerializedProperty elementArrayProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_nextElements");
			SerializedProperty offsetArrayProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_offsets");

			elementArrayProperty.arraySize = 0;
			offsetArrayProperty.arraySize = 0;
			
			++m_weightArrayProperty.arraySize;
			SerializedProperty weightProperty = m_weightArrayProperty.GetArrayElementAtIndex(m_weightArrayProperty.arraySize - 1);
			weightProperty.floatValue = 1.0f;
		}
		
		void OnRandomSoundSequenceRemoved(int a_index, SerializedProperty a_property)
		{
			m_weightArrayProperty.DeleteArrayElementAtIndex(a_index);
		}
		
		void OnRandomSoundSequenceGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			SerializedProperty weightProperty = m_weightArrayProperty.GetArrayElementAtIndex(a_index);
			EditorGUILayout.PropertyField(weightProperty, new GUIContent("Weight"));
			
			EditorGUILayout.PropertyField(a_property, new GUIContent(a_label));
		}

		void OnEventAdded(int a_index, SerializedProperty property)
		{
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
			SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
			SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
			objectProperty.objectReferenceValue = (target as SoundSequenceController).gameObject;
			componentProperty.objectReferenceValue = null;
			methodNameProperty.stringValue = "";
		}

		SerializedProperty m_playOnStartProperty;
		SerializedProperty m_lastSequencePolicyProperty;
		SerializedProperty m_loopPolicyProperty;

		SerializedProperty m_sequenceArrayProperty;
		SerializedProperty m_weightArrayProperty;

		SerializedProperty m_onSequenceBeginEventArrayProperty;
		SerializedProperty m_onSequenceEndEventArrayProperty;
#endregion
	}
}