using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Aube
{
	//! @class AubeEditor
	//!
	//! @brief Utility class to retrieve the editor elements for Aube.
	public static class AubeEditor
	{
		public static GUISkin skin
		{
			get
			{
				if(ms_skin == null)
				{
					ms_skin = AssetDatabase.LoadAssetAtPath(ms_skinPath, typeof(GUISkin)) as GUISkin;
				}
				return ms_skin;
			}
		}

#region Private
		private static string ms_skinPath = "Assets/Aube/Editor/Skin/AubeEditor.guiskin";
		private static GUISkin ms_skin;
#endregion
	}
}