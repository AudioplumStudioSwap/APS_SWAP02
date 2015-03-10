using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class ScriptingTriggerInspector
	//!
	//! @brief Custom Inspector for class ScriptingCollisionTrigger
	[CustomEditor(typeof(BehaviourTrigger))]
	public class BehaviourTriggerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorCollection.Show(m_onStartEventsProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);
			EditorCollection.Show(m_onEnableEventsProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);
			EditorCollection.Show(m_onDisableEventsProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);
			
			serializedObject.ApplyModifiedProperties();
		}
		
#region Private
	#region Methods
		void OnEnable()
		{
			m_onStartEventsProperty = serializedObject.FindProperty("m_onStartEvents");
			m_onEnableEventsProperty = serializedObject.FindProperty("m_onEnableEvents");
			m_onDisableEventsProperty = serializedObject.FindProperty("m_onDisableEvents");
		}
		
		void OnEventAdded(int a_index, SerializedProperty property)
		{
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
			SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
			SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
			objectProperty.objectReferenceValue = (target as BehaviourTrigger).gameObject;
			componentProperty.objectReferenceValue = null;
			methodNameProperty.stringValue = "";
		}
	#endregion
		
	#region Attributes
		private SerializedProperty m_onStartEventsProperty = null;
		private SerializedProperty m_onEnableEventsProperty = null;
		private SerializedProperty m_onDisableEventsProperty = null;
	#endregion
#endregion
	}
}