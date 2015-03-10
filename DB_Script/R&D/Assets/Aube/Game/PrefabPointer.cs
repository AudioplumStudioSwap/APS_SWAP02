using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aube
{
    //! @class PrefabPointer
	//!
	//! @brief 	ResourcePointer for a prefab
	[System.Serializable]
	public class PrefabPointer : ResourcePointer<GameObject>
    {
        //! @brief return the prefab (load it if needed)
        public GameObject Prefab
		{
            get { return Resource; }            
        }
    }
}