using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Aube
{
	//! @class SwitcherInspector
	//!
	//! @brief Custom Inspector for class Switcher
	[CustomEditor(typeof(Switcher))]
	public class SwitcherInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			if(EditorApplication.isPlayingOrWillChangePlaymode)
			{
				GUI.enabled = false;
			}

			EditorCollection.Show(m_domainArrayProperty, EditorCollection.Option.Alternative | EditorCollection.Option.BoxElement, null, null, null, OnDomainGUI);

			System.Reflection.PropertyInfo[] properties;
			GUIContent[] propertyNames;
			UpdateCache((target as Switcher).gameObject, out properties, out propertyNames);

			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Property");
				System.Reflection.PropertyInfo propertySelected = GetPropertyInfo();
				int selectionIndex = (properties == null)? -1 : System.Array.IndexOf(properties, propertySelected);
				int selectionNewIndex = EditorGUILayout.Popup(selectionIndex, propertyNames);
				
				if(selectionIndex != selectionNewIndex)
				{
					SetPropertyInfo(properties[selectionNewIndex]);
				}
				
				propertySelected = GetPropertyInfo();
			}
			EditorGUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();

			if(EditorApplication.isPlayingOrWillChangePlaymode)
			{
				GUI.enabled = true;
			}
		}

#region Private
	#region Methods
		private void OnEnable()
		{
			m_domainArrayProperty = serializedObject.FindProperty("m_domains");
			m_targetProperty = serializedObject.FindProperty("m_target");
			m_propertyNameProperty = serializedObject.FindProperty("m_propertyName");
			m_propertyTypeProperty = serializedObject.FindProperty("m_propertyType");
		}

		private void OnDomainGUI(int a_elementIndex, string a_label, SerializedProperty a_property)
		{
			SerializedProperty targetProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".target");
			SerializedProperty activeInsideProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".activeInside");
			SerializedProperty minValueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".minValue");
			SerializedProperty maxValueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".maxValue");

			EditorGUILayout.PropertyField(targetProperty);
			EditorGUILayout.PropertyField(activeInsideProperty);

			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("domain");
				EditorGUILayout.PropertyField(minValueProperty, GUIContent.none);
				EditorGUILayout.PropertyField(maxValueProperty, GUIContent.none);
			}
			EditorGUILayout.EndHorizontal();
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
				List<System.Reflection.PropertyInfo> properties = new List<System.Reflection.PropertyInfo>(goType.GetProperties(Switcher.bindingFlags));
				Component[] allComponents = go.GetComponents<Component>();
				foreach(Component component in allComponents)
				{
					if(component != null)
					{
						System.Type componentType = component.GetType();
						properties.AddRange(componentType.GetProperties(Switcher.bindingFlags));
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

		void SetPropertyInfo(System.Reflection.PropertyInfo a_propertyInfo)
		{			
			if(a_propertyInfo == null)
			{
				m_targetProperty.objectReferenceValue = null;
				m_propertyNameProperty.stringValue = "";
				m_propertyTypeProperty.enumValueIndex = (int)Switcher.PropertyType.Integer;
			}
			else
			{
				if(a_propertyInfo.DeclaringType.IsAssignableFrom(typeof(GameObject)))
				{
					m_targetProperty.objectReferenceValue = (target as Switcher).gameObject;
				}
				else
				{
					m_targetProperty.objectReferenceValue = (target as Switcher).GetComponent(a_propertyInfo.DeclaringType);
				}
				
				m_propertyNameProperty.stringValue = a_propertyInfo.Name;

				if(a_propertyInfo.PropertyType == typeof(int))
				{
					m_propertyTypeProperty.enumValueIndex = (int)Switcher.PropertyType.Integer;
				}
				else if(a_propertyInfo.PropertyType == typeof(float))
				{
					m_propertyTypeProperty.enumValueIndex = (int)Switcher.PropertyType.Float;
				}
				else
				{
					Debug.LogError("Invalid property type : " + a_propertyInfo.PropertyType.Name);
					m_targetProperty.objectReferenceValue = null;
					m_propertyNameProperty.stringValue = "";
					m_propertyTypeProperty.enumValueIndex = (int)Switcher.PropertyType.Integer;
				}
			}
		}
		
		System.Reflection.PropertyInfo GetPropertyInfo()
		{			
			if(string.IsNullOrEmpty(m_propertyNameProperty.stringValue)  ||  m_targetProperty.objectReferenceValue == null)
			{
				return null;
			}

			System.Type declaringLeafType = m_targetProperty.objectReferenceValue.GetType();
			
			System.Reflection.PropertyInfo[] potentialProperties = declaringLeafType.GetProperties(Switcher.bindingFlags);			

			int potentialPropertyIndex = 0;
			while(potentialPropertyIndex < potentialProperties.Length
			      &&  (potentialProperties[potentialPropertyIndex].Name != m_propertyNameProperty.stringValue  ||  IsValidProperty(potentialProperties[potentialPropertyIndex]) == false))
			{
				++potentialPropertyIndex;
			}

			return (potentialPropertyIndex < potentialProperties.Length)? potentialProperties[potentialPropertyIndex] : null;
		}

		private bool IsValidProperty(System.Reflection.PropertyInfo a_propertyInfo)
		{
			return a_propertyInfo.PropertyType == typeof(int)
				||  a_propertyInfo.PropertyType == typeof(float);
		}
	#endregion

	#region Attributes
		private SerializedProperty m_domainArrayProperty;
		private SerializedProperty m_targetProperty;
		private SerializedProperty m_propertyNameProperty;
		private SerializedProperty m_propertyTypeProperty;
	#endregion
#endregion
	}
}