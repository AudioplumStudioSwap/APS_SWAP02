using System;
using UnityEngine;
using UnityEditor;

namespace Aube
{
	[CustomPropertyDrawer(typeof(MaskFieldAttribute))]

	//!	@class	MaskFieldEditor
	//!
	//!	@brief	Property drawer for property MaskField
	public class MaskFieldEditor : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty a_property, GUIContent a_label)
		{
			if(IsEnumerationWithFlagsAttribute(a_property))
			{
				return base.GetPropertyHeight(a_property, a_label);
			}
			else
			{
				return 32.0f;
			}
		}

	    public override void OnGUI(Rect a_rect, SerializedProperty a_property, GUIContent a_label) 
	    {
			if(IsEnumerationWithFlagsAttribute(a_property))
			{
				a_property.intValue = EditorGUI.MaskField(a_rect, a_label, a_property.intValue, a_property.enumNames);
			}
			else
			{
				EditorGUI.HelpBox(a_rect, "The property attribute 'MaskField' can not be used for fields that are not an enumeration with System.Flags attribute.", MessageType.Error);
			}
	    }

#region Private
		private bool IsEnumerationWithFlagsAttribute(SerializedProperty a_property)
		{
			return a_property.propertyType == SerializedPropertyType.Enum
				&&  fieldInfo.FieldType.GetCustomAttributes(typeof(System.FlagsAttribute), true).Length > 0;
		}
#endregion
	}
} // namespace Aube