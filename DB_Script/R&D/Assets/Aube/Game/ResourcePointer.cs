using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aube
{
	//! @class ResourcePointer
	//!
	//! @brief 	Pointer to a resource that may be loaded at the loading of the container of this instance or on demand.
	[System.Serializable]
	public class ResourcePointer<t_Class> : IResourcePointer where t_Class : UnityEngine.Object
    {
        public class Request
        {
			public t_Class m_asset = null;
        }

		[SerializeField]
		private t_Class m_resourceAsset = null;
		[SerializeField]
		private string m_resourcePath = null;

#if UNITY_EDITOR
        [HideInInspector]
        public bool m_foldout = true;
#endif

        //! @brief return the resource (load it if needed)
		public t_Class Resource
		{
			get { return GetResource(m_loadKind, m_resourceAsset, m_resourcePath); }
        }

		public override Object ResourceObject
		{
			get{ return Resource; }
		}

		public void Set(t_Class a_resource)
		{
			m_loadKind = LoadKind.PreLoaded;
			m_resourceAsset = a_resource;
			m_resourcePath = "";
		}

		public void Set(string a_resourcePath)
		{
			m_loadKind = LoadKind.LoadOnDemand;
			m_resourceAsset = null;
			m_resourcePath = a_resourcePath;
		}

#if UNITY_EDITOR
		public override System.Type ResourceType
		{
			get{ return typeof(t_Class); }
		}
#endif // UNITY_EDITOR

        //! @brief return the prefab (load it asynchronously if needed)
        public IEnumerator LoadResourceAsync(Request a_request)
        {
            switch (m_loadKind)
            {
                case LoadKind.LoadOnDemand:
                {
					if (string.IsNullOrEmpty(m_resourcePath))
                    {
						a_request.m_asset = null;
                    }
                    else
                    {
						ResourceRequest request = Resources.LoadAsync<GameObject>(m_resourcePath);
                        yield return request;
						a_request.m_asset = request.asset as t_Class;
                    }
                    break;
                }
                default:
                {
					a_request.m_asset = Resource;
                    break;
                }
            }
        }

        public static t_Class GetResource(PrefabPointer.LoadKind a_kind, t_Class a_prefab, string a_prefabPath)
        {
			switch(a_kind)
            {
                case LoadKind.PreLoaded:
                {
					return a_prefab;
                }
                case LoadKind.LoadOnDemand:
                {
					return (string.IsNullOrEmpty(a_prefabPath))? null : Resources.Load<t_Class>(a_prefabPath);
                }
                default:
                {
                    Assertion.UnreachableCode();
                    break;
                }
            }
            return null;
        }
    }
}