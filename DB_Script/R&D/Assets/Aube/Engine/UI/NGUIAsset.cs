#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class NGUIAsset
	//!
	//! @brief Asset that defines all parameters to intialize NGUI
	public class NGUIAsset : ScriptableObject
	{
	//*************************************************************************
	// Attributes
	//*************************************************************************
		//! Root prefab
		[SerializeField]
		private GameObject m_RootPrefab = null;

        //! Base layer (display & input)
        [SerializeField]
        private int m_baseLayer = -1;

        //! Base layer (display only)
        [SerializeField]
        private int m_lockLayer = -1;

	//*************************************************************************
	// Getters
	//*************************************************************************
		public GameObject RootPrefab
		{
			get{ return m_RootPrefab; }
		}

        
        public int BaseLayer 
        { 
            get { return m_baseLayer; } 
        }

        //! @brief The custom ui layer
        //! must be set in the culling mask of the camera
        //! must not be set in the event mask of the UICamera        
        public int LockLayer 
        { 
            get { return m_lockLayer; } 
        }
	}
} // namespace Aube

#endif // !AUBE_NO_UI