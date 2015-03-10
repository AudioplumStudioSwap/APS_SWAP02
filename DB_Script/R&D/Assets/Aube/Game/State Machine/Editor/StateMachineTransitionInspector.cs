using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	[CustomEditor(typeof(StateMachineTransition))]

	//!	@class	StateMachineTransitionInspector
	//!
	//!	@brief	Custom inspector for class StateMachineTransition
	public class StateMachineTransitionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			m_stateMachineObject.Update();

			GUILayout.Label("Transition", "OL Title");
			EditorGUILayout.BeginHorizontal(GUI.skin.box);
			{
				GUILayout.Label(m_stateFromProperty.objectReferenceValue.name, EditorStyles.boldLabel);
				GUILayout.Label(" => ", EditorStyles.boldLabel);
				GUILayout.Label(m_stateToProperty.objectReferenceValue.name, EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
			}
			EditorGUILayout.EndHorizontal();

			EditorCollection.Option options = EditorCollection.Option.ElementAdd | EditorCollection.Option.ElementRemove | EditorCollection.Option.CollectionLabel;
			EditorCollection.Show(m_conditionSetArrayProperty, options, null, OnConditionSetAdded, null, OnConditionSetGUI);

			serializedObject.ApplyModifiedProperties();
		}

#region Unity Callbacks
		private void OnEnable()
		{
			m_stateFromProperty = serializedObject.FindProperty("m_stateFrom");
			m_stateToProperty = serializedObject.FindProperty("m_stateTo");
			m_conditionSetArrayProperty = serializedObject.FindProperty("m_conditionSets");

			StateMachine stateMachine = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)) as StateMachine;
			m_stateMachineObject = new SerializedObject(stateMachine);
			m_stateMachineBooleanArrayProperty = m_stateMachineObject.FindProperty("m_booleanNames");
			m_stateMachineIntegerArrayProperty = m_stateMachineObject.FindProperty("m_integerNames");
			m_stateMachineFloatArrayProperty = m_stateMachineObject.FindProperty("m_floatNames");
			m_stateMachineTriggerArrayProperty = m_stateMachineObject.FindProperty("m_triggerNames");
		}
#endregion

#region Private
	#region Methods
		private void OnConditionSetAdded(int a_index, SerializedProperty a_property)
		{
			SerializedProperty conditionArrayProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionArrayAttributeName);
			conditionArrayProperty.arraySize = 0;
		}

		private void OnConditionSetGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			EditorGUILayout.BeginVertical(GUI.skin.box);
			{
				SerializedProperty conditionArrayProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionArrayAttributeName);
				EditorCollection.Show(conditionArrayProperty, EditorCollection.Option.Alternative, null, OnConditionAdded, null, OnConditionGUI);
			}
			EditorGUILayout.EndVertical();
		}

		private void OnConditionAdded(int a_index, SerializedProperty a_property)
		{
			SerializedProperty parameterNameProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionParameterName);
			SerializedProperty parameterHashedNameProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionParameterHashedName);
			SerializedProperty parameterTypeProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionParameterType);
			SerializedProperty comparisonValueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionComparisonValue);
			SerializedProperty otherValueBoolProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionOtherValueBool);
			SerializedProperty otherValueIntProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionOtherValueInt);
			SerializedProperty otherValueFloatProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionOtherValueFloat);

			parameterNameProperty.stringValue = string.Empty;
			parameterHashedNameProperty.intValue = StateMachine.StringToHash(parameterNameProperty.stringValue);
			parameterTypeProperty.intValue = -1;
			comparisonValueProperty.intValue = 0;
			otherValueBoolProperty.boolValue = false;
			otherValueIntProperty.intValue = 0;
			otherValueFloatProperty.floatValue = 0.0f;
		}

		private void OnConditionGUI(int a_index, string a_label, SerializedProperty a_property)
		{
			EditorGUILayout.BeginHorizontal();
			{
				SerializedProperty[] parameterProperties = new SerializedProperty[4];
				parameterProperties[0] = m_stateMachineBooleanArrayProperty;
				parameterProperties[1] = m_stateMachineIntegerArrayProperty;
				parameterProperties[2] = m_stateMachineFloatArrayProperty;
				parameterProperties[3] = m_stateMachineTriggerArrayProperty;

				string[] pathFolder = new string[4];
				pathFolder[0] = "Boolean/";
				pathFolder[1] = "Integer/";
				pathFolder[2] = "Float/";
				pathFolder[3] = "Trigger/";

				int parameterCount = m_stateMachineBooleanArrayProperty.arraySize
									+ m_stateMachineIntegerArrayProperty.arraySize
									+ m_stateMachineFloatArrayProperty.arraySize
									+ m_stateMachineTriggerArrayProperty.arraySize;

				string[] parametersName = new string[parameterCount];
				string[] parameterPath = new string[parameterCount];

				int selection = -1;
				SerializedProperty parameterNameProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionParameterName);
				SerializedProperty parameterHashedNameProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionParameterHashedName);
				SerializedProperty parameterTypeProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionParameterType);

				for(int parameterIndex = 0; parameterIndex < parameterCount; ++parameterIndex)
				{
					int loopIndex = 0;
					SerializedProperty elementProperty = null;
					int arrayIndex = parameterIndex;
					while(loopIndex < parameterProperties.Length  &&  elementProperty == null)
					{
						if(arrayIndex < parameterProperties[loopIndex].arraySize)
						{
							elementProperty = parameterProperties[loopIndex].GetArrayElementAtIndex(arrayIndex);
						}
						else
						{
							arrayIndex -= parameterProperties[loopIndex].arraySize;
							++loopIndex;
						}
					}

					parametersName[parameterIndex] = elementProperty.stringValue;
					parameterPath[parameterIndex] = pathFolder[loopIndex] + elementProperty.stringValue;

					if(selection == -1
					   &&  parameterNameProperty.stringValue == parametersName[parameterIndex]
					   &&  parameterTypeProperty.intValue == loopIndex)
					{
						selection = parameterIndex;
					}
				}

				int newSelection = EditorGUILayout.Popup(selection, parameterPath, GUILayout.Width(175.0f));
				if(newSelection != selection)
				{
					if(newSelection >= 0)
					{
						parameterNameProperty.stringValue = parametersName[newSelection];
						parameterHashedNameProperty.intValue = StateMachine.StringToHash(parameterNameProperty.stringValue);

						int loopIndex = 0;
						int arrayIndex = newSelection;
						while(arrayIndex >= parameterProperties[loopIndex].arraySize)
						{
							arrayIndex -= parameterProperties[loopIndex].arraySize;
							++loopIndex;
						}
						parameterTypeProperty.intValue = loopIndex;
					}
					else
					{
						parameterNameProperty.stringValue = string.Empty;
						parameterHashedNameProperty.intValue = StateMachine.StringToHash(parameterNameProperty.stringValue);
						parameterTypeProperty.intValue = -1;
					}
				}

				switch(parameterTypeProperty.intValue)
				{
					case 0:
					{
						SerializedProperty otherValueBoolProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionOtherValueBool);
						EditorGUILayout.PropertyField(otherValueBoolProperty, GUIContent.none);
					}
					break;
					case 1:
					case 2:
					{
						SerializedProperty comparisonValueProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionComparisonValue);
						string[] comparisonNames = new string[]{ "Equal", "Less", "Less or equal", "Greater", "Greater or equal" };
						comparisonValueProperty.intValue = EditorGUILayout.Popup("", comparisonValueProperty.intValue, comparisonNames, GUILayout.Width(125.0f));

						if(parameterTypeProperty.intValue == 1)
						{
							SerializedProperty otherValueIntProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionOtherValueInt);
							EditorGUILayout.PropertyField(otherValueIntProperty, GUIContent.none);
						}
						else
						{
							SerializedProperty otherValueFloatProperty = a_property.serializedObject.FindProperty(a_property.propertyPath + "." + ms_conditionOtherValueFloat);
							EditorGUILayout.PropertyField(otherValueFloatProperty, GUIContent.none);
						}
					}
					break;
					case 3: break;
				}
			}
			EditorGUILayout.EndHorizontal();
		}
	#endregion

	#region Attributes
		private SerializedProperty m_stateFromProperty;
		private SerializedProperty m_stateToProperty;
		private SerializedProperty m_conditionSetArrayProperty;

		private SerializedObject m_stateMachineObject;
		private SerializedProperty m_stateMachineBooleanArrayProperty;
		private SerializedProperty m_stateMachineIntegerArrayProperty;
		private SerializedProperty m_stateMachineFloatArrayProperty;
		private SerializedProperty m_stateMachineTriggerArrayProperty;

		private static string ms_conditionArrayAttributeName = "m_conditions";

		private static string ms_conditionParameterName = "m_parameterName";
		private static string ms_conditionParameterHashedName = "m_parameterHashedName";
		private static string ms_conditionParameterType = "m_parameterType";
		private static string ms_conditionComparisonValue = "m_comparisonValue";
		private static string ms_conditionOtherValueBool = "m_otherValueBool";
		private static string ms_conditionOtherValueInt = "m_otherValueInt";
		private static string ms_conditionOtherValueFloat = "m_otherValueFloat";
	#endregion
#endregion
	}
}