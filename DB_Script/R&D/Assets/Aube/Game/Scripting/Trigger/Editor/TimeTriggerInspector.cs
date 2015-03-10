using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class TimeTriggerInspector
	//!
	//! @brief Custom Inspector for a Time Trigger
	[CustomEditor(typeof(TimeTrigger))]
	public class TimeTriggerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(m_periodProperty);
			EditorGUILayout.PropertyField(m_periodOffsetProperty);
			EditorCollection.Show(m_onPeriodEventArrayProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);
			
			serializedObject.ApplyModifiedProperties();
		}
		
#region Private
	#region Methods
		void OnEnable()
		{
			m_periodProperty = serializedObject.FindProperty("m_period");
			m_periodOffsetProperty = serializedObject.FindProperty("m_periodOffset");
			m_onPeriodEventArrayProperty = serializedObject.FindProperty("m_onPeriodEvents");
		}
		
		void OnEventAdded(int a_index, SerializedProperty property)
		{
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
			SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
			SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
			objectProperty.objectReferenceValue = (target as TimeTrigger).gameObject;
			componentProperty.objectReferenceValue = null;
			methodNameProperty.stringValue = "";
		}
	#endregion
		
	#region Attributes		
		private SerializedProperty m_periodProperty;
		private SerializedProperty m_periodOffsetProperty;
		private SerializedProperty m_onPeriodEventArrayProperty;
	#endregion
#endregion
	}
}