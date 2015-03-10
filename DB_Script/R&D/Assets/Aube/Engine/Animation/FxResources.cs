using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Aube
{
    //! @class FxResources
    //!
    //! @brief Contains a FxResource list whose the elements can be activated with an FxCommand.
    public class FxResources : MonoBehaviour
    {
    #region Attributes
    #region Private
        [SerializeField]
        private FxResource[] m_resources = new FxResource[0];
        [SerializeField]
        private Transform m_root = null;
    #endregion
    #endregion

    #region Methods
    #region Public
        public Transform Root
        {
            get { return m_root; }
            set 
            {
                if (m_root != value)
                {
                    m_root = value;
                    RefreshInstances();
                }                 
            }
        }

        public void RefreshInstances()
        {
            for (int i = 0; i < m_resources.Length; ++i)
            {
                Transform parent = GetParent(i);
                m_resources[i].RefreshInstance((parent != null)? parent : transform);
            }
        }

        public bool Activate(string name, bool activate, FxCommand command)
        {
            FxResource resource = FindResource(name);

            if (resource != null)
            {
                return resource.Activate(activate, command);
            }

            Aube.Log.Warning(typeof(FxResources).Name + ": " + name + " not found");
            return false;
        }
    #endregion
    #region Private
        private void Awake()
        {
            RefreshInstances();
        }

        private FxResource FindResource(string name)
        {
            for (int i = 0; i < m_resources.Length; ++i)
            {
                if (string.Equals(m_resources[i].Ident, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return m_resources[i];
                }
            }
            return null;
        }

        private Transform GetParent(int index)
        {
            Transform parent = null;
            FxResource resource = m_resources[index];
            
            if (m_root != null && !string.IsNullOrEmpty(resource.Parent))
            {
                parent = m_root.Find(resource.Parent);

                if (parent == null)
                {
                    parent = m_root.FindInHierarchy(resource.Parent);
                }
            }
            return parent;
        }
    #endregion
    #endregion
    }
}