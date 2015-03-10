using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class EditorControls
	//!
	//! @brief list of simple Editor Controls extension
	public static class EditorControls
	{
		//! @brief Displays a path control for files
		//! @details The control is composed of a preffix label, a label displaying the value of the property
		//! and a Browse button
		//!
		//! @param	a_position	position of the control
		//! @param	a_name		value of the preffix label
		//! @param	a_path		current value of the path
		//!
		//! @return the new value of the path
		public static string File(Rect a_position, string a_name, string a_path, string a_extensions)
		{
			string filePath = a_path;

			Rect labelRect = new Rect(a_position.x, a_position.y, Mathf.Max(0, a_position.width - 60), a_position.height);
			Rect buttonRect = new Rect(a_position.x + labelRect.width, a_position.y, 60, a_position.height);

			EditorGUI.LabelField(labelRect, a_name, a_path);
				
			if(GUI.Button(buttonRect, "Browse"))
			{
				if(string.IsNullOrEmpty(a_path))
				{
					a_path = Application.dataPath;
				}

				filePath = EditorUtility.OpenFilePanel(a_name, a_path, a_extensions);
			}
			
			return filePath;
		}

		//! @brief Displays a path control for folders
		//! @details The control is composed of a preffix label, a label displaying the value of the property
		//! and a Browse button
		//!
		//! @param	a_position	position of the control
		//! @param	a_name		value of the preffix label
		//! @param	a_path		current value of the path
		//!
		//! @return the new value of the path
		public static string Path(Rect a_position, string a_name, string a_path)
		{
			string filePath = a_path;

			Rect labelRect = new Rect(a_position.x, a_position.y, Mathf.Max(0, a_position.width - 60), a_position.height);
			Rect buttonRect = new Rect(a_position.x + labelRect.width, a_position.y, 60, a_position.height);
			
			EditorGUI.LabelField(labelRect, a_name, a_path);

			if(GUI.Button(buttonRect, "Browse"))
			{
				if(string.IsNullOrEmpty(a_path))
				{
					a_path = Application.dataPath;
				}
			
				filePath = EditorUtility.OpenFolderPanel(a_name, a_path, "Folder");
			}

			if(string.IsNullOrEmpty(filePath) == false  &&  filePath.EndsWith("/") == false)
			{
				filePath += "/";
			}

			return filePath;
		}
	}
} // namespace Aube