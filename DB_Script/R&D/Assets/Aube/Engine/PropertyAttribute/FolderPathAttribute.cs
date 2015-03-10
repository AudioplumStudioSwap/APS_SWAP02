using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class FolderPath
	//!
	//! @brief Property to add on a string attribute representing a path to a fodler
	public class FolderPathAttribute : PathAttribute
	{
		//! @brief Constructor
		public FolderPathAttribute() {}

		//! @brief Constructor
		//!
		//! @param 	a_pathType	type of path
		public FolderPathAttribute(PathType a_type) : base(a_type) {}
	}
}
