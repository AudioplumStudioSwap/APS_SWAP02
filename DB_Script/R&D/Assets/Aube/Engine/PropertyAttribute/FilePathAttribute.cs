using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class FilePath
	//!
	//! @brief Property to add on a string attribute representing a path to a file
	public class FilePathAttribute : PathAttribute
	{
		//! @brief Constructor
		public FilePathAttribute() : this("") {}

		//! @brief Constructor
		//!
		//! @param 	a_pathType	type of path
		public FilePathAttribute(string a_extensions)
		{
			extensions = a_extensions;
		}

		//! @brief Constructor
		//!
		//! @param 	a_pathType	type of path
		public FilePathAttribute(PathType a_type) : this(a_type, "") {}

		//! @brief Constructor
		//!
		//! @param 	a_pathType	type of path
		public FilePathAttribute(PathType a_type, string a_extensions) :
			base(a_type)
		{
			extensions = a_extensions;
		}

		public readonly string extensions;
	}
} // namespace Aube
