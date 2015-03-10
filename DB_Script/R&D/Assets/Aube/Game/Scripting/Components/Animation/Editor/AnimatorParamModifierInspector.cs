using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

namespace Aube
{
	[CustomEditor(typeof(AnimatorParamModifier))]
	public class AnimatorParamModifierInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			AnimatorParamModifier component = serializedObject.targetObject as AnimatorParamModifier;
			Animator[] availableAnimators = component.GetComponentsInChildren<Animator>();

			Animator currentAnimator = m_animatorProperty.objectReferenceValue as Animator;
			int currentSelection = System.Array.IndexOf(availableAnimators, currentAnimator);

			GUIContent[] popupAnimatorContent = new GUIContent[availableAnimators.Length];
			for(int animatorIndex = 0; animatorIndex < availableAnimators.Length; ++animatorIndex)
			{
				Animator animator = availableAnimators[animatorIndex];
				AnimatorController animatorController = AnimatorController.GetEffectiveAnimatorController(animator);

				string animatorLabel = animator.name + " (" + (animatorController == null? "no controller" : animatorController.name) + ")";
				popupAnimatorContent[animatorIndex] = new GUIContent(animatorLabel);
			}

			int newSelection = EditorGUILayout.Popup(currentSelection, popupAnimatorContent);
			if(newSelection != currentSelection)
			{
				if(newSelection == -1)
				{
					m_animatorProperty.objectReferenceValue = null;
				}
				else
				{
					m_animatorProperty.objectReferenceValue = availableAnimators[newSelection];
				}
			}

			if(m_animatorProperty.objectReferenceValue != null)
			{
				currentAnimator = m_animatorProperty.objectReferenceValue as Animator;
				AnimatorController animatorController = AnimatorController.GetEffectiveAnimatorController(currentAnimator);
				if(animatorController != null)
				{
					EditorGUILayout.LabelField("Parameter count : " + animatorController.parameterCount);

					int selectedParamIndex = -1;
					GUIContent[] popupParameterContent = new GUIContent[animatorController.parameterCount];
					for(int paramIndex = 0; paramIndex < animatorController.parameterCount; ++paramIndex)
					{
						AnimatorControllerParameter animParam = animatorController.GetParameter(paramIndex);
						popupParameterContent[paramIndex] = new GUIContent(animParam.name + " (" + System.Enum.GetName(typeof(AnimatorControllerParameterType), animParam.type) + ")");

						if(animParam.name == m_animatorParamNameProperty.stringValue)
						{
							selectedParamIndex = paramIndex;
						}
					}

					int newParamIndex = EditorGUILayout.Popup(selectedParamIndex, popupParameterContent);
					if(newParamIndex != selectedParamIndex)
					{
						if(newParamIndex == - 1)
						{
							m_animatorParamNameProperty.stringValue= "";
						}
						else
						{
							AnimatorControllerParameter animParam = animatorController.GetParameter(newParamIndex);
							m_animatorParamNameProperty.stringValue = animParam.name;
						}
					}

					if(newParamIndex != -1)
					{
						AnimatorControllerParameter animParam = animatorController.GetParameter(newParamIndex);
						switch(animParam.type)
						{
							case AnimatorControllerParameterType.Int:
							{
								m_animatorParamTypeProperty.enumValueIndex = (int)AnimatorParamModifier.ParameterType.Int;
								EditorGUILayout.PropertyField(m_animatorParamDoIntProperty, new GUIContent("Do Value"));
								EditorGUILayout.PropertyField(m_animatorParamUndoIntProperty, new GUIContent("Undo Value"));
							}
							break;
							case AnimatorControllerParameterType.Float:
							{
								m_animatorParamTypeProperty.enumValueIndex = (int)AnimatorParamModifier.ParameterType.Float;
								EditorGUILayout.PropertyField(m_animatorParamDoFloatProperty, new GUIContent("Value"));
								EditorGUILayout.PropertyField(m_animatorParamUndoFloatProperty, new GUIContent("Undo Value"));
							}
							break;
							case AnimatorControllerParameterType.Bool:
							{
								m_animatorParamTypeProperty.enumValueIndex = (int)AnimatorParamModifier.ParameterType.Bool;
								EditorGUILayout.PropertyField(m_animatorParamDoBoolProperty, new GUIContent("Value"));
								EditorGUILayout.PropertyField(m_animatorParamUndoBoolProperty, new GUIContent("Undo Value"));
							}
							break;
							case AnimatorControllerParameterType.Trigger:
							{
								m_animatorParamTypeProperty.enumValueIndex = (int)AnimatorParamModifier.ParameterType.Trigger;
							}
							break;
						}
					}
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

#region Private
		void OnEnable()
		{
			m_animatorProperty = serializedObject.FindProperty("m_animator");
			m_animatorParamNameProperty = serializedObject.FindProperty("m_animatorParameterName");
			m_animatorParamTypeProperty = serializedObject.FindProperty("m_animatorParameterType");

			m_animatorParamDoIntProperty = serializedObject.FindProperty("m_animatorParameterDoInt");
			m_animatorParamDoFloatProperty = serializedObject.FindProperty("m_animatorParameterDoFloat");
			m_animatorParamDoBoolProperty = serializedObject.FindProperty("m_animatorParameterDoBool");

			m_animatorParamUndoIntProperty = serializedObject.FindProperty("m_animatorParameterUndoInt");
			m_animatorParamUndoFloatProperty = serializedObject.FindProperty("m_animatorParameterUndoFloat");
			m_animatorParamUndoBoolProperty = serializedObject.FindProperty("m_animatorParameterUndoBool");
		}

		SerializedProperty m_animatorProperty;
		SerializedProperty m_animatorParamNameProperty;
		SerializedProperty m_animatorParamTypeProperty;

		SerializedProperty m_animatorParamDoIntProperty;
		SerializedProperty m_animatorParamDoFloatProperty;
		SerializedProperty m_animatorParamDoBoolProperty;

		SerializedProperty m_animatorParamUndoIntProperty;
		SerializedProperty m_animatorParamUndoFloatProperty;
		SerializedProperty m_animatorParamUndoBoolProperty;
#endregion
	}
}