using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	[CustomEditor(typeof(Pool))]

	//!	@class	PoolInspector
	//!
	//!	@brief	Custom Inspector for class Pool
	public class PoolInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			// force recreate instances
			bool forceSync = GUILayout.Button("Force Sync");

			// source
			Object currentSource = m_sourceProperty.objectReferenceValue;
			EditorGUILayout.PropertyField(m_sourceProperty);
			if(forceSync  ||  currentSource != m_sourceProperty.objectReferenceValue)
			{
				for(int copyIndex = 0; copyIndex < m_copyArrayProperty.arraySize; ++copyIndex)
				{
					SerializedProperty copyProperty = m_copyArrayProperty.GetArrayElementAtIndex(copyIndex);
					if(copyProperty.objectReferenceValue != null)
					{
						GameObject.DestroyImmediate(copyProperty.objectReferenceValue as GameObject);
					}

					copyProperty.objectReferenceValue = CreatePooledObject();
				}
			}

			// size
			int newSize = Mathf.Max(EditorGUILayout.IntField("Size", m_copyArrayProperty.arraySize), 0);
			if(newSize != m_copyArrayProperty.arraySize)
			{
				while(m_copyArrayProperty.arraySize > newSize)
				{
					SerializedProperty copyProperty = m_copyArrayProperty.GetArrayElementAtIndex(m_copyArrayProperty.arraySize - 1);
					if(copyProperty.objectReferenceValue != null)
					{
						GameObject.DestroyImmediate(copyProperty.objectReferenceValue as GameObject);
						copyProperty.objectReferenceValue = null;
					}
					m_copyArrayProperty.DeleteArrayElementAtIndex(m_copyArrayProperty.arraySize - 1);
				}

				while(m_copyArrayProperty.arraySize < newSize)
				{
					++m_copyArrayProperty.arraySize;
					SerializedProperty copyProperty = m_copyArrayProperty.GetArrayElementAtIndex(m_copyArrayProperty.arraySize - 1);
					copyProperty.objectReferenceValue = CreatePooledObject();
				}
			}

			// options
			EditorGUILayout.PropertyField(m_optionProperty);

			serializedObject.ApplyModifiedProperties();
		}

#region Unity Callbacks
		private void OnEnable()
		{
			m_sourceProperty = serializedObject.FindProperty("m_source");
			m_copyArrayProperty = serializedObject.FindProperty("m_copies");
			m_optionProperty = serializedObject.FindProperty("m_options");
		}
#endregion

#region Private
	#region Methods
		private GameObject CreatePooledObject()
		{
			GameObject sourceObject = m_sourceProperty.objectReferenceValue as GameObject;

			if(sourceObject == null)
			{
				return null;
			}
			else
			{
				GameObject copyObject = null;
				if(PrefabUtility.GetPrefabType(sourceObject) == PrefabType.Prefab)
				{
					copyObject = PrefabUtility.InstantiatePrefab(sourceObject) as GameObject;
				}
				else
				{
					copyObject = GameObject.Instantiate(sourceObject) as GameObject;
				}
				copyObject.hideFlags = HideFlags.NotEditable;
				copyObject.SetActive(false);
				copyObject.transform.parent = (serializedObject.targetObject as Pool).transform;
				EditorUtility.SetDirty(copyObject);

				return copyObject;
			}
		}
	#endregion

	#region Attributes
		private SerializedProperty m_sourceProperty;
		private SerializedProperty m_copyArrayProperty;
		private SerializedProperty m_optionProperty;
	#endregion
#endregion
	}
}