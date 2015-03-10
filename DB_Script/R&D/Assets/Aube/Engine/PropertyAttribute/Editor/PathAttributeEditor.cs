using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	[CustomPropertyDrawer(typeof(PathAttribute), true)]

	//!	@class	PathAttributeDrawer
	//!
	//!	@brief	Property drawer of a PathAttribute.
	public class PathAttributeEditor : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			PathAttribute pathAttribute = attribute as PathAttribute;

			string path = "";
			if(pathAttribute is FilePathAttribute)
			{
				FilePathAttribute fileAttribute = pathAttribute as FilePathAttribute;
				path = Aube.EditorControls.File(position, label.text, property.stringValue, fileAttribute.extensions);
			}
			else
			{
				path = Aube.EditorControls.Path(position, label.text, property.stringValue);
			}

			if(path != property.stringValue)
			{
				if(pathAttribute.ValidatePath(ref path))
				{
					property.stringValue = path;
				}
			}
		}
	}
} // namespace Aube