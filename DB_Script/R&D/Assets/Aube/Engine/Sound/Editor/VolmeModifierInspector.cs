using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Aube
{
	//! @class VolmeModifierInspector
	//!
	//! @brief Custom inspector for class VolumeModifier
	[CustomEditor(typeof(VolumeModifier))]
	public class VolmeModifierInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(m_categoryProperty);

			string foldoutName = ((m_requestFoldout)? "\u25BC " : "\u25BA ") + "<b><size=11>Init Requests</size></b>";
			if(GUILayout.Toggle(true, foldoutName, "dragtab", GUILayout.MinWidth(20f)) == false)
			{
				m_requestFoldout = !m_requestFoldout;
			}

			if(m_requestFoldout)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.BeginVertical("AS TextArea");

				int requestIndex = 0;
				while(requestIndex < m_initRequestArrayProperty.arraySize)
				{
					SerializedProperty requestProperty = m_initRequestArrayProperty.GetArrayElementAtIndex(requestIndex);

					SerializedProperty targetProperty = requestProperty.serializedObject.FindProperty(requestProperty.propertyPath + ".target");
					SerializedProperty propertyNameProperty = requestProperty.serializedObject.FindProperty(requestProperty.propertyPath + ".propertyName");

					Object target = targetProperty.objectReferenceValue;
					string propertyName = propertyNameProperty.stringValue;
					OnRequestGUI(ref target, ref propertyName);
					targetProperty.objectReferenceValue = target;
					propertyNameProperty.stringValue = propertyName;

					if(targetProperty.objectReferenceValue == null)
					{
						m_initRequestArrayProperty.DeleteArrayElementAtIndex(requestIndex);
					}
					else
					{
						++requestIndex;
					}

					EditorGUILayout.Space();
				}

				Object newTarget = null;
				string newPropertyName = string.Empty;
				OnRequestGUI(ref newTarget, ref newPropertyName);

				if(newTarget != null)
				{
					++m_initRequestArrayProperty.arraySize;
					SerializedProperty newRequestProperty = m_initRequestArrayProperty.GetArrayElementAtIndex(m_initRequestArrayProperty.arraySize - 1);

					SerializedProperty targetProperty = newRequestProperty.serializedObject.FindProperty(newRequestProperty.propertyPath + ".target");
					SerializedProperty propertyNameProperty = newRequestProperty.serializedObject.FindProperty(newRequestProperty.propertyPath + ".propertyName");
					targetProperty.objectReferenceValue = newTarget;
					propertyNameProperty.stringValue = newPropertyName;
				}

				EditorGUILayout.EndHorizontal();
				--EditorGUI.indentLevel;
			}
			
			serializedObject.ApplyModifiedProperties();
		}
		
#region Private
		private void OnEnable()
		{
			m_categoryProperty = serializedObject.FindProperty("m_category");
			m_initRequestArrayProperty = serializedObject.FindProperty("m_initRequests");

			m_requestFoldout = true;
		}

		private void OnRequestGUI(ref Object a_target, ref string a_propertyName)
		{
			a_target = EditorGUILayout.ObjectField("Target", a_target, typeof(Object), true);

			if(a_target == null)
			{
				a_propertyName = string.Empty;
			}
			else
			{
				GameObject gameObject = (a_target is Component)? (a_target as Component).gameObject : a_target as GameObject;
				if(gameObject == null)
				{
					a_target = null;
					a_propertyName = string.Empty;
				}
				else
				{
					System.Reflection.PropertyInfo[] properties;
					GUIContent[] propertyNames;
					UpdateCache(gameObject, out properties, out propertyNames);
					
					System.Reflection.PropertyInfo propertySelected = GetPropertyInfo(ref a_target, ref a_propertyName);
					int selectionIndex = (properties == null)? -1 : System.Array.IndexOf(properties, propertySelected);
					int selectionNewIndex = EditorGUILayout.Popup(new GUIContent("Property"), selectionIndex, propertyNames);
					
					if(selectionIndex != selectionNewIndex)
					{
						SetPropertyInfo(properties[selectionNewIndex], ref a_target, ref a_propertyName);
					}
				}
			}
		}

		private void UpdateCache(GameObject go, out System.Reflection.PropertyInfo[] a_properties, out GUIContent[] a_propertyNames)
		{
			if(go == null)
			{
				a_properties = null;
				a_propertyNames = null;
			}
			else
			{
				// get all properties
				System.Type goType = typeof(GameObject);
				List<System.Reflection.PropertyInfo> properties = new List<System.Reflection.PropertyInfo>(goType.GetProperties(VolumeModifier.bindingFlags));
				Component[] allComponents = go.GetComponents<Component>();
				foreach(Component component in allComponents)
				{
					if(component != null)
					{
						System.Type componentType = component.GetType();
						properties.AddRange(componentType.GetProperties(VolumeModifier.bindingFlags));
					}
				}
				
				int index = 0;
				while(index < properties.Count)
				{
					if(IsValidProperty(properties[index]))
					{
						++index;
					}
					else
					{
						properties.RemoveAt(index);
					}
				}
				
				a_properties = properties.ToArray();				
				a_propertyNames = new GUIContent[a_properties.Length];
				for(int methodIndex = 0; methodIndex < a_properties.Length; ++methodIndex)
				{
					string propertyFullName = a_properties[methodIndex].DeclaringType.Name + "." + a_properties[methodIndex].Name;
					a_propertyNames[methodIndex] = new GUIContent(propertyFullName);
				}
			}
		}

		private bool IsValidProperty(System.Reflection.PropertyInfo a_propertyInfo)
		{
			return a_propertyInfo.PropertyType == typeof(float);
		}

		void SetPropertyInfo(System.Reflection.PropertyInfo a_propertyInfo, ref Object a_target, ref string a_propertyName)
		{			
			if(a_propertyInfo == null)
			{
				a_propertyName = string.Empty;
			}
			else
			{
				if(typeof(GameObject).IsAssignableFrom(a_propertyInfo.DeclaringType)  &&  a_target is Component)
				{
					a_target = (a_target as Component).gameObject;
				}
				else if(typeof(Component).IsAssignableFrom(a_propertyInfo.DeclaringType)  &&  a_target.GetType().IsAssignableFrom(a_propertyInfo.DeclaringType) == false)
				{
					GameObject gameObject = (a_target is GameObject)? a_target as GameObject : (a_target as Component).gameObject;
					a_target = gameObject.GetComponent(a_propertyInfo.DeclaringType);
				}
				
				a_propertyName = a_propertyInfo.Name;
			}
		}
		
		System.Reflection.PropertyInfo GetPropertyInfo(ref Object a_target, ref string a_propertyName)
		{			
			if(string.IsNullOrEmpty(a_propertyName)  ||  a_target == null)
			{
				return null;
			}
			
			System.Type declaringLeafType = a_target.GetType();
			
			System.Reflection.PropertyInfo[] potentialProperties = declaringLeafType.GetProperties(VolumeModifier.bindingFlags);			
			
			int potentialPropertyIndex = 0;
			while(potentialPropertyIndex < potentialProperties.Length
			      &&  (potentialProperties[potentialPropertyIndex].Name != a_propertyName  ||  IsValidProperty(potentialProperties[potentialPropertyIndex]) == false))
			{
				++potentialPropertyIndex;
			}
			
			return (potentialPropertyIndex < potentialProperties.Length)? potentialProperties[potentialPropertyIndex] : null;
		}

	#region Attributes
		private SerializedProperty m_categoryProperty;
		private SerializedProperty m_initRequestArrayProperty;

		private bool m_requestFoldout;
	#endregion
#endregion
	}
}
