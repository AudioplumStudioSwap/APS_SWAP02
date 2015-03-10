using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class ScriptingTriggerInspector
	//!
	//! @brief Custom Inspector for class ScriptingCollisionTrigger
	[CustomEditor(typeof(CollisionTrigger))]
	public class CollisionTriggerInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			// tags
			m_restrictionTagFoldout = EditorGUILayout.Foldout(m_restrictionTagFoldout, new GUIContent("Tag Restrictions"));
			if(m_restrictionTagFoldout)
			{
				int tagIndex = 0;
				while(tagIndex < m_restrictionTagArrayProperty.arraySize)
				{
					EditorGUILayout.BeginHorizontal();
					{
						SerializedProperty tagProperty = m_restrictionTagArrayProperty.GetArrayElementAtIndex(tagIndex);
						tagProperty.stringValue = EditorGUILayout.TagField(tagProperty.stringValue);
						if(GUILayout.Button("-", GUILayout.Width(20)))
						{
							m_restrictionTagArrayProperty.DeleteArrayElementAtIndex(tagIndex);
						}
						else
						{
							++tagIndex;
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.BeginHorizontal();
				{
					string newTag = EditorGUILayout.TagField("");
					if(string.IsNullOrEmpty(newTag) == false)
					{
						++m_restrictionTagArrayProperty.arraySize;
						SerializedProperty newTagProperty = m_restrictionTagArrayProperty.GetArrayElementAtIndex(m_restrictionTagArrayProperty.arraySize - 1);
						newTagProperty.stringValue = newTag;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			// events
			EditorCollection.Show(m_onEnterEventsProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);
			EditorCollection.Show(m_onExitEventsProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);
			EditorCollection.Show(m_onStayEventsProperty, EditorCollection.Option.Alternative, null, OnEventAdded, null);

			serializedObject.ApplyModifiedProperties();
		}

#region Private
	#region Methods
		void OnEnable()
		{
			m_restrictionTagArrayProperty = serializedObject.FindProperty("m_restrictionTags");

			m_onEnterEventsProperty = serializedObject.FindProperty("m_onEnterEvents");
			m_onExitEventsProperty = serializedObject.FindProperty("m_onExitEvents");
			m_onStayEventsProperty = serializedObject.FindProperty("m_onStayEvents");

			m_restrictionTagFoldout = false;
		}

		void OnEventAdded(int a_index, SerializedProperty property)
		{
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
			SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
			SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
			objectProperty.objectReferenceValue = (target as CollisionTrigger).gameObject;
			componentProperty.objectReferenceValue = null;
			methodNameProperty.stringValue = "";
		}
	#endregion

	#region Attributes
		private SerializedProperty m_restrictionTagArrayProperty = null;

		private SerializedProperty m_onEnterEventsProperty = null;
		private SerializedProperty m_onExitEventsProperty = null;
		private SerializedProperty m_onStayEventsProperty = null;

		private bool m_restrictionTagFoldout;
	#endregion
#endregion
	}
}