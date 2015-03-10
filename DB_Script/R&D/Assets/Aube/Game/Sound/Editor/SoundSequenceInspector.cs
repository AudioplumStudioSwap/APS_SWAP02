using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class SoundSequenceInspector
	//!
	//! @brief Custom Inspector of a Sound Sequence
	[CustomPropertyDrawer(typeof(SoundSequence), true)]
	public class SoundSequenceInspector : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty elementsArrayProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_nextElements");

			float height =  7 * lineHeight
				+ lineHeight * (elementsArrayProperty.arraySize + 1);

			GUIContent labelElementContent = new GUIContent("Element x");
			SerializedProperty defaultElementProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_defaultElement");
			height += EditorGUI.GetPropertyHeight(defaultElementProperty, labelElementContent);

			for(int elementIndex = 0; elementIndex < elementsArrayProperty.arraySize; ++elementIndex)
			{
				SerializedProperty elementProperty = elementsArrayProperty.GetArrayElementAtIndex(elementIndex);
				height += EditorGUI.GetPropertyHeight(elementProperty, labelElementContent);
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			SerializedProperty defaultElementProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_defaultElement");
			SerializedProperty elementsArrayProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_nextElements");
			SerializedProperty offsetArrayProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_offsets");
			SerializedProperty delayBeginProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_delayBegin");
			SerializedProperty delayEndProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_delayEnd");
			SerializedProperty fadeInProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_fadeInDuration");
			SerializedProperty fadeOutProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_fadeOutDuration");
			SerializedProperty useRandomPitchProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_useRandomPitch");
			SerializedProperty randomPitchBoundProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_randomPitchBoundaries");

			Rect delayBeginPropertyRect = new Rect(position.x, position.y, position.width, lineHeight);
			EditorGUI.PropertyField(delayBeginPropertyRect, delayBeginProperty, new GUIContent("Delay before sequence"));
			Rect delayEndPropertyRect = new Rect(position.x, delayBeginPropertyRect.y + delayBeginPropertyRect.height, position.width, lineHeight);
			EditorGUI.PropertyField(delayEndPropertyRect, delayEndProperty, new GUIContent("Delay after sequence"));

			Rect fadeInPropertyRect = new Rect(position.x, delayEndPropertyRect.y + delayEndPropertyRect.height, position.width, lineHeight);
			EditorGUI.PropertyField(fadeInPropertyRect, fadeInProperty, new GUIContent("Fade In Duration"));
			Rect fadeOutPropertyRect = new Rect(position.x, fadeInPropertyRect.y + fadeInPropertyRect.height, position.width, lineHeight);
			EditorGUI.PropertyField(fadeOutPropertyRect, fadeOutProperty, new GUIContent("Fade Out Duration"));

			Rect useRandomPitchRect = new Rect(position.x, fadeOutPropertyRect.y + fadeOutPropertyRect.height, position.width, lineHeight);
			EditorGUI.PropertyField(useRandomPitchRect, useRandomPitchProperty);
			EditorGUI.BeginDisabledGroup(useRandomPitchProperty.boolValue == false);
			Rect randomPitchBoundRect = new Rect(position.x, useRandomPitchRect.y + useRandomPitchRect.height, position.width, lineHeight * 2);
			EditorGUI.PropertyField(randomPitchBoundRect, randomPitchBoundProperty);
			Vector2 result = randomPitchBoundProperty.vector2Value;
			if(result.x < 1e-6f) { result.x = 1e-6f; }
			if(result.x > 3.0f) { result.x = 3.0f; }
			if(result.y < 1e-6f) { result.y = 1e-6f; }
			if(result.y > 3.0f) { result.y = 3.0f; }
			if(result.x > result.y) { result.x = result.y; }
			randomPitchBoundProperty.vector2Value = result;
			EditorGUI.EndDisabledGroup();

			float top = randomPitchBoundRect.y + randomPitchBoundRect.height;
			GUIContent labelElementContent = new GUIContent("Element 0");
			float elementRectHeight = EditorGUI.GetPropertyHeight(defaultElementProperty, labelElementContent);
			Rect elementRect = new Rect(position.x, top, position.width, elementRectHeight);
			EditorGUI.PropertyField(elementRect, defaultElementProperty, labelElementContent);

			top += elementRect.height;

			int elementToRemoveIndex = -1;
			for(int elementIndex = 0; elementIndex < elementsArrayProperty.arraySize; ++elementIndex)
			{
				SerializedProperty offsetProperty = offsetArrayProperty.GetArrayElementAtIndex(elementIndex);
				SerializedProperty elementProperty = elementsArrayProperty.GetArrayElementAtIndex(elementIndex);
			
				float elementYPosition = top;

				labelElementContent = new GUIContent("Element " + (elementIndex + 1));
				elementRectHeight = EditorGUI.GetPropertyHeight(elementProperty, labelElementContent);
				elementRect = new Rect(position.x, top, position.width - removeButtonWidth, elementRectHeight);
				EditorGUI.PropertyField(elementRect, elementProperty, labelElementContent);

				top += elementRect.height;

				Rect transitionRect = new Rect(position.x + elementInnerParamIndent, top, position.width - elementInnerParamIndent - removeButtonWidth, lineHeight);
				EditorGUI.PropertyField(transitionRect, offsetProperty, new GUIContent("Transition (s)"));

				top += transitionRect.height;

				Rect removeButtonRect = new Rect(position.x + position.width - removeButtonWidth, elementYPosition, removeButtonWidth, lineHeight);
				if(GUI.Button(removeButtonRect, "-"))
				{
					elementToRemoveIndex = elementIndex;
				}
			}

			if(elementToRemoveIndex >= 0)
			{
				elementsArrayProperty.DeleteArrayElementAtIndex(elementToRemoveIndex);
				offsetArrayProperty.DeleteArrayElementAtIndex(elementToRemoveIndex);
			}

			Rect addButtonRect = new Rect(position.x, top, labelOffset, lineHeight);
			if(GUI.Button(addButtonRect, "+"))
			{
				++offsetArrayProperty.arraySize;
				++elementsArrayProperty.arraySize;
			}

			EditorGUI.EndProperty();
		}

		public static float lineHeight = 20.0f;
		public static float elementInnerParamIndent = 20.0f;

#region Private
		static float labelOffset = 120.0f;
		static float removeButtonWidth = 20.0f;
#endregion
	}

	[CustomPropertyDrawer(typeof(SoundSequence.Sound))]
	public class SoundSequenceSoundInspector : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty clipResourceProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_audioClipResource");
			float resourceHeight = EditorGUI.GetPropertyHeight(clipResourceProperty, GUIContent.none);

			SerializedProperty garbagePolicyProperty = property.FindPropertyRelative("m_garbagePolicy");
			float garbagePolicyHeight = EditorGUI.GetPropertyHeight(garbagePolicyProperty, GUIContent.none);

			return SoundSequenceInspector.lineHeight + resourceHeight + garbagePolicyHeight;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent a_label)
		{
			Rect labelRect = new Rect(position.x, position.y, position.width, SoundSequenceInspector.lineHeight);
			EditorGUI.LabelField(labelRect, a_label, EditorStyles.boldLabel);
			
			SerializedProperty clipResourceProperty = property.serializedObject.FindProperty(property.propertyPath + ".m_audioClipResource");
			float resourceHeight = EditorGUI.GetPropertyHeight(clipResourceProperty, GUIContent.none);
			Rect clipResourceRect = new Rect(position.x + SoundSequenceInspector.elementInnerParamIndent, labelRect.y + labelRect.height, position.width - SoundSequenceInspector.elementInnerParamIndent, resourceHeight);
			EditorGUI.PropertyField(clipResourceRect, clipResourceProperty);

			SerializedProperty resourceLoadKindProperty = clipResourceProperty.FindPropertyRelative("m_loadKind");

			IResourcePointer.LoadKind[] values = (IResourcePointer.LoadKind[])System.Enum.GetValues(typeof(IResourcePointer.LoadKind));
			EditorGUI.BeginDisabledGroup(IResourcePointer.LoadKind.LoadOnDemand != values[resourceLoadKindProperty.enumValueIndex]);
			SerializedProperty garbagePolicyProperty = property.FindPropertyRelative("m_garbagePolicy");
			float garbagePolicyHeight = EditorGUI.GetPropertyHeight(garbagePolicyProperty, GUIContent.none);
			Rect garbagePolicyRect = new Rect(position.x + SoundSequenceInspector.elementInnerParamIndent, clipResourceRect.y + clipResourceRect.height, position.width - SoundSequenceInspector.elementInnerParamIndent, garbagePolicyHeight);
			EditorGUI.PropertyField(garbagePolicyRect, garbagePolicyProperty);
			EditorGUI.EndDisabledGroup();
		}
	}
}