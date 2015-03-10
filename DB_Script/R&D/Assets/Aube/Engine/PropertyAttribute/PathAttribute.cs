using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class PathAttribute
	//!
	//! @brief base class for a property to add on a string attribute representing a path
	public abstract class PathAttribute : PropertyAttribute
	{
		public enum PathType
		{
			None,
			Assets,
			Resources,
		}
		
		//! @brief Constructor
		public PathAttribute() : this(PathType.None) {}
		
		//! @brief Constructor
		//!
		//! @param	a_type		type of path
		public PathAttribute(PathType a_type)
		{
			type = a_type;
		}
		
		//! @brief Method to check if a path is valid
		public bool ValidatePath(ref string a_path)
		{
			// accepts empty path
			if(string.IsNullOrEmpty(a_path))
			{
				return true;
			}
			
			// check path restriction (under Assets folder, under Resources folder...)
			if(type == PathType.None)
			{
				return true;
			}
			
			bool noError = true;
			switch(type)
			{
			    case PathType.Assets:
			    {
				    string absolutePath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
				    a_path = a_path.Substring(absolutePath.Length, a_path.Length - absolutePath.Length);
			    }
				break;
			    case PathType.Resources:
			    {
				    string absolutePath = Application.dataPath.Substring(0, Application.dataPath.Length);
				    a_path = a_path.Substring(absolutePath.Length, a_path.Length - absolutePath.Length);
                    noError = MakePathRelativeToResourcesFolder(ref a_path);
			    }
				break;
			}
			
			return noError;
		}
		
		public readonly PathType type;

        //! @brief Method to make a path relative to the resource folder
        public static bool MakePathRelativeToResourcesFolder(ref string a_path)
		{
			const string resourceFolderName = "Resources/";
			int resourceFolderLength = resourceFolderName.Length;
			
			int lastIndex = a_path.LastIndexOf(resourceFolderName);
			
			if(lastIndex >= 0  &&  lastIndex < a_path.Length - resourceFolderLength)
			{
				int removeEndIndex = lastIndex + resourceFolderLength;
				a_path = a_path.Substring(removeEndIndex, a_path.Length - removeEndIndex);
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
