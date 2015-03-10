using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Aube
{
	//! @class ScriptingEventInspector
	//!
	//! @brief Custom inspector for class ScriptingEvent
	[CustomPropertyDrawer(typeof(ScriptingEvent))]
	public class ScriptingEventInspector : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
			SerializedProperty parametersProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_parameters");

			float parameterHeight = parametersProperty.arraySize * GetParameterHeight();

			return base.GetPropertyHeight(objectProperty, label) + parameterHeight;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
		
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");

			// draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			
			// calculate rects
			float objectHeight = base.GetPropertyHeight(objectProperty, label);
			Rect objectRect = new Rect(position.x, position.y, position.width * 0.5f, objectHeight);
			Rect popupRect = new Rect(objectRect.x + objectRect.width, position.y, position.width * 0.5f, objectHeight);
			
			// draw fields
			Object oldObject = objectProperty.objectReferenceValue;
			EditorGUI.PropertyField(objectRect, objectProperty, GUIContent.none);
			if(oldObject != objectProperty.objectReferenceValue)
			{
				SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
				SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
				SerializedProperty parametersProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_parameters");

				componentProperty.objectReferenceValue = null;
				methodNameProperty.stringValue = "";
				parametersProperty.arraySize = 0;
			}

			EditorGUI.BeginDisabledGroup(objectProperty.objectReferenceValue == null);
			{
				System.Reflection.MethodInfo[] methods;
				GUIContent[] methodNames;
				UpdateCache(objectProperty.objectReferenceValue as GameObject, out methods, out methodNames);

				System.Reflection.MethodInfo methodSelected = GetMethodInfo(property);
				int selectionIndex = (methods == null)? -1 : System.Array.IndexOf(methods, methodSelected);
				int selectionNewIndex = EditorGUI.Popup(popupRect, selectionIndex, methodNames);

				if(selectionIndex != selectionNewIndex)
				{
					SetMethodInfo(property, methods[selectionNewIndex]);
				}

				methodSelected = GetMethodInfo(property);
				EditorGUI.BeginDisabledGroup(methodSelected == null);
				if(methodSelected != null)
				{
					++EditorGUI.indentLevel;

					Rect paramRect = new Rect(position.x, objectRect.y + objectRect.height, position.width, GetParameterHeight());

					SerializedProperty parametersProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_parameters");
					System.Reflection.ParameterInfo[] parameters = methodSelected.GetParameters();
					Aube.Assertion.Check(parametersProperty.arraySize == parameters.Length, "Error synchronizing scripting event.");

					for(int paramIndex = 0; paramIndex < parameters.Length; ++paramIndex)
					{
						System.Reflection.ParameterInfo parameter = parameters[paramIndex];
						System.Type expectedType = parameter.ParameterType;

						Rect labelRect = new Rect(paramRect.x, paramRect.y, 150, paramRect.height);
						Rect argRect = new Rect(labelRect.x + labelRect.width, paramRect.y, paramRect.width - labelRect.width, paramRect.height);

						EditorGUI.LabelField(labelRect, "Arg " + (paramIndex + 1) + " <" + expectedType.Name + ">", "");
						DisplayParameter(expectedType, argRect, parametersProperty.GetArrayElementAtIndex(paramIndex));

						paramRect.y += paramRect.height;
					}

					--EditorGUI.indentLevel;
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUI.EndDisabledGroup();
			
			EditorGUI.EndProperty();
		}

#region Private
		void UpdateCache(GameObject go, out System.Reflection.MethodInfo[] a_methods, out GUIContent[] a_methodNames)
		{
			if(go == null)
			{
				a_methods = null;
				a_methodNames = null;
			}
			else
			{
				System.Type goType = typeof(GameObject);
				List<System.Reflection.MethodInfo> methods = new List<System.Reflection.MethodInfo>(goType.GetMethods(ScriptingEvent.BindingFlags));
				Component[] allComponents = go.GetComponents<Component>();
				foreach(Component component in allComponents)
				{
					if(component != null)
					{
						System.Type componentType = component.GetType();
						methods.AddRange(componentType.GetMethods(ScriptingEvent.BindingFlags));
					}
				}

				int index = 0;
				while(index < methods.Count)
				{
					if(IsValidMethod(methods[index]))
					{
						++index;
					}
					else
					{
						methods.RemoveAt(index);
					}
				}

				a_methods = methods.ToArray();

				a_methodNames = new GUIContent[a_methods.Length];
				for(int methodIndex = 0; methodIndex < a_methods.Length; ++methodIndex)
				{
					string methodFullName = a_methods[methodIndex].DeclaringType.Name + "." + a_methods[methodIndex].Name;
					a_methodNames[methodIndex] = new GUIContent(methodFullName);
				}
			}
		}

		void SetMethodInfo(SerializedProperty property, System.Reflection.MethodInfo a_methodInfo)
		{
			SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
			SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
			SerializedProperty parametersProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_parameters");

			if(a_methodInfo == null)
			{
				componentProperty.objectReferenceValue = null;
				methodNameProperty.stringValue = "";
				parametersProperty.arraySize = 0;
			}
			else
			{
				if(a_methodInfo.DeclaringType.IsAssignableFrom(typeof(GameObject)))
				{
					componentProperty.objectReferenceValue = null;
				}
				else
				{
					SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
					Component component = (objectProperty.objectReferenceValue as GameObject).GetComponent(a_methodInfo.DeclaringType);
					Assertion.Check(component != null, "Invalid method info given : there is no component of type '" + a_methodInfo.DeclaringType.Name + "' in object '" + objectProperty.objectReferenceValue.name + "'.");
					
					componentProperty.objectReferenceValue = component;
				}
				
				methodNameProperty.stringValue = a_methodInfo.Name;

				System.Reflection.ParameterInfo[] parameters = a_methodInfo.GetParameters();
				parametersProperty.arraySize = 0;

				for(int paramIndex = 0; paramIndex < parameters.Length; ++paramIndex)
				{
					parametersProperty.InsertArrayElementAtIndex(paramIndex);
					SerializedProperty paramProperty = parametersProperty.GetArrayElementAtIndex(paramIndex);
					InitializeParameter(parameters[paramIndex].ParameterType, paramProperty);
				}

			}
		}

		System.Reflection.MethodInfo GetMethodInfo(SerializedProperty property)
		{
			SerializedProperty objectProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetObject");
			SerializedProperty componentProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetComponent");
			SerializedProperty methodNameProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_targetMethodName");
			SerializedProperty parametersProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_parameters");

			if(string.IsNullOrEmpty(methodNameProperty.stringValue)  ||  objectProperty.objectReferenceValue == null)
			{
				return null;
			}
			
			System.Type declaringLeafType = (componentProperty.objectReferenceValue == null)? objectProperty.objectReferenceValue.GetType() : componentProperty.objectReferenceValue.GetType();

			System.Type[] types = new System.Type[parametersProperty.arraySize];
			for(int paramIndex = 0; paramIndex < parametersProperty.arraySize; ++paramIndex)
			{
				SerializedProperty paramProperty = parametersProperty.GetArrayElementAtIndex(paramIndex);

				EditorGUI.BeginProperty(new Rect(0, 0, 0, 0), new GUIContent(""), paramProperty);

				SerializedProperty paramObjectProperty = paramProperty.serializedObject.FindProperty(paramProperty.propertyPath + ".m_objectValue");
				if(paramObjectProperty.objectReferenceValue == null)
				{
					types[paramIndex] = typeof(void);
				}
				else
				{
					types[paramIndex] = paramObjectProperty.objectReferenceValue.GetType();
				}

				EditorGUI.EndProperty();
			}

			System.Reflection.MethodInfo[] potentialMethods = declaringLeafType.GetMethods(ScriptingEvent.BindingFlags);

			System.Reflection.MethodInfo method = null;
			int potentialMethodIndex = 0;
			while(method == null  &&  potentialMethodIndex < potentialMethods.Length)
			{
				if(potentialMethods[potentialMethodIndex].Name == methodNameProperty.stringValue  &&  IsValidMethod(potentialMethods[potentialMethodIndex]))
				{
					System.Reflection.ParameterInfo[] parameters = potentialMethods[potentialMethodIndex].GetParameters();
					if(parameters.Length == types.Length)
					{
						bool allParametersMatch = true;
						int paramIndex = 0;
						while(allParametersMatch  &&  paramIndex < parameters.Length)
						{
							allParametersMatch = parameters[paramIndex].ParameterType.IsAssignableFrom(types[paramIndex])  ||  types[paramIndex] == typeof(void);
							++paramIndex;
						}

						if(allParametersMatch)
						{
							method = potentialMethods[potentialMethodIndex];
						}
					}
				}
				
				++potentialMethodIndex;
			}
			return method;
		}

		float GetParameterHeight() { return 50.0f; }

		void InitializeParameter(System.Type a_type, SerializedProperty a_property)
		{
			EditorGUI.BeginProperty(new Rect(0, 0, 0, 0), new GUIContent(""), a_property);
			
			//SerializedProperty objectProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_object");
			
			// bool
			if(a_type == typeof(bool))
			{
				//objectProperty.objectReferenceValue = new ScriptingEvent.BoolObject(false);
			}
			
			EditorGUI.EndProperty();
		}

		void DisplayParameter(System.Type a_type, Rect a_rect, SerializedProperty a_property)
		{
			GUI.Box(a_rect, GUIContent.none);

			const float labelWidth = 100.0f;

			Rect kindLabelRect = new Rect(a_rect.x, a_rect.y + 5.0f, labelWidth, 20.0f);
			Rect kindRect = new Rect(kindLabelRect.x + labelWidth, kindLabelRect.y, a_rect.width - labelWidth, kindLabelRect.height);
			SerializedProperty kindProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_setKind");
			EditorGUI.LabelField(kindLabelRect, "Set Method", "");
			EditorGUI.PropertyField(kindRect, kindProperty, GUIContent.none);

			if(kindProperty.enumValueIndex == 0)
			{
				Rect valueLabelRect = new Rect(a_rect.x, kindLabelRect.y + kindRect.height, labelWidth, 20.0f);
				Rect valueRect = new Rect(valueLabelRect.x + labelWidth, valueLabelRect.y, a_rect.width - labelWidth, valueLabelRect.height);
				EditorGUI.LabelField(valueLabelRect, "Value", "");
				// boolean
				if(a_type == typeof(bool))
				{
					SerializedProperty valueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_boolValue");
					EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
				}
				else if(a_type == typeof(int))
				{
					SerializedProperty valueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_intValue");
					EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
				}
				else if(a_type == typeof(float))
				{
					SerializedProperty valueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_floatValue");
					EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
				}
				else if(a_type == typeof(Object))
				{
					SerializedProperty valueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_objectValue");
					EditorGUI.ObjectField(valueRect, valueProperty.objectReferenceValue, a_type, true);
				}
			}
			else
			{
				Rect objectRect = new Rect(a_rect.x, kindLabelRect.y + kindLabelRect.height, a_rect.width / 2, 20.0f);
				Rect propertyNameRect = new Rect(objectRect.x + objectRect.width, objectRect.y, a_rect.width - objectRect.width, objectRect.height);

				SerializedProperty valueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_objectRef");
				EditorGUI.PropertyField(objectRect, valueProperty, GUIContent.none, true);

				SerializedProperty propertyNameProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + ".m_propertyName");
				EditorGUI.PropertyField(propertyNameRect, propertyNameProperty, GUIContent.none, true);
			}
		}

		bool IsValidMethod(System.Reflection.MethodInfo a_method)
		{
			return a_method.IsConstructor == false
				&&  a_method.ReturnType == typeof(void)
				// TODO To Be Removed for arguments handling
				&&  a_method.GetParameters().Length == 0
				&&  a_method.ContainsGenericParameters == false;
		}
#endregion
	}
}
