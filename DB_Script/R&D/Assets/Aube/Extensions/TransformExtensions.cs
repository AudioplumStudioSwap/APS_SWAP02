using UnityEngine;
using System.Collections.Generic;

//! @class TransformExtensions
//!
//! @brief extension methods for UnityEngine.Transform
public static class TransformExtensions
{
	//! @brief Find a child in the hierarchy of the transform
	//!
	//! @this	a_root		transform root (start of the algorithm)
	//! @param	a_name		name of the child to find
	public static Transform FindInHierarchy(this Transform a_root, string a_name)
	{
		if(a_root.name == a_name)
		{
			return a_root;
		}

		Queue<Transform> transformToCheck = new Queue<Transform>();
		foreach(Transform child in a_root)
		{
			transformToCheck.Enqueue(child);
		}

		Transform result = null;
		while(result == null  &&  transformToCheck.Count > 0)
		{
			Transform childTransform = transformToCheck.Dequeue();
			if(childTransform.name == a_name)
			{
				result = childTransform;
			}
			else
			{
				foreach(Transform child in childTransform)
				{
					transformToCheck.Enqueue(child);
				}
			}
		}

		return result;
	}
}
