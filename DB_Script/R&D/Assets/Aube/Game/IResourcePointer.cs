using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aube
{
	//! @class IResourcePointer
	//!
	//! @brief 	Base class for a resource pointer
	[System.Serializable]
	public abstract class IResourcePointer
    {
        public enum LoadKind
        {
            LoadOnDemand,
            PreLoaded,            
        }

        [SerializeField]
        protected LoadKind m_loadKind = LoadKind.LoadOnDemand;

		public abstract Object ResourceObject{ get; }

#if UNITY_EDITOR
		public abstract System.Type ResourceType{ get; }
		public virtual string ResourceLabel
		{
			get{ return "Resource"; }
		}
#endif // UNITY_EDITOR
    }
}