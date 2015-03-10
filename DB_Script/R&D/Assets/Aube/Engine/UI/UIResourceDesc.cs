#if !AUBE_NO_UI
using UnityEngine;
using System.Collections;

namespace Aube
{
    //! @class UIResourceDesc
    //!
    //! @brief NGUIMenuDesc implementation which manages an array of ui pages
    //! ENUM_ARRAY must be declared as "[Serializable] public class UIArray : EnumArray<YOUR_ENUM, UIResourceDescBase.Resource>"
    //! The child class must be declared as "public class UIDesc: UIResourceDesc<YOUR_ENUM, UIArray>"
    public abstract class UIResourceDesc<ENUM, ENUM_ARRAY> : UIResourceDescBase, NGUIMenuDesc
        where ENUM : struct, System.IConvertible
        where ENUM_ARRAY : EnumArray<ENUM, UIResourceDescBase.Resource>, new()
    {
    #region Attributes
    #region Private
        [SerializeField]
        private ENUM_ARRAY m_resources = new ENUM_ARRAY();
    #endregion
    #endregion

    #region Methods
    #region Public
        public uint Count { get { return (uint)m_resources.Length; } }
        
        public virtual GameObject GetMenu(uint index)
        {
            if (m_resources[index].m_instance == null)
            {
                Aube.Log.Warning(GetType().Name + "(" + GetResourceName(index) + "): resource not loaded");
            }
            return m_resources[index].m_instance;
        }
        
        public virtual IEnumerator Load(uint index)
        {
            if (m_resources[index].m_instance == null)
            {
                PrefabPointer.Request request = new PrefabPointer.Request();
                yield return StartCoroutine(m_resources[index].m_prefab.LoadResourceAsync(request));

                if (request.m_asset != null)
                {
                    // Deactivate the prefab in order to create an instance without activate it
                    bool active = request.m_asset.activeSelf;
                    request.m_asset.SetActive(false);

                    // Instanciate
                    m_resources[index].m_instance = GameObject.Instantiate(request.m_asset) as GameObject;
                    OnMenuHidden(index);

                    // Restore the prefab state
                    request.m_asset.SetActive(active);
                }
            }

            if (m_resources[index].m_instance == null)
            {
                Log.Error(GetType().Name + "(" + GetResourceName(index) + "): invalid resource");
            }
        }

        public virtual void Unload(uint index)
        {
            if (m_resources[index].m_instance != null)
            {
                GameObject.DestroyImmediate(m_resources[index].m_instance);
                m_resources[index].m_instance = null;
            }
        }

        public virtual void OnMenuHidden(uint index)
        {
            if (m_resources[index].m_instance != null)
            {
                m_resources[index].m_instance.transform.parent = transform;
            }
        }

        public void LoadAll()
        {
            for (uint i = 0; i < m_resources.Length; ++i)
            {
                Load(i);
            }
        }

        public void UnloadAll()
        {
            for (uint i = 0; i < m_resources.Length; ++i)
            {
                Unload(i);
            }
        }
    #endregion
    #region Protected
        protected string GetResourceName(uint index)
        {
            return System.Enum.GetName(typeof(ENUM), index);
        }
    #endregion
    #endregion
    }
}
#endif // !AUBE_NO_UI