using UnityEngine;
using System.Collections;

namespace Aube
{
    //! @class FxResource
    //!
    //! @brief Defines a prefab to instanciate in current hierarchy.
    [System.Serializable]
    public class FxResource
    {
    #region Attributes
    #region Private
        [SerializeField]
        private string m_ident = null;
        [SerializeField]
        private GameObject m_prefab = null;
        [SerializeField]
        private string m_parent = null;

#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        private bool m_foldout = true;
        [SerializeField]
        [HideInInspector]
        private bool m_hasParent = false;
#endif

        private GameObject m_instance = null;
        private FxBehaviour[] m_behaviours = null;
    #endregion
    #endregion

    #region Methods
    #region Public
        public string Ident
        {
            get { return m_ident; }
        }

        public string Parent
        {
            get { return m_parent; }
        }

        public bool IsInstanced()
        {
            return (m_instance != null);
        }

        public void RefreshInstance(Transform parent)
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Instantiate(m_prefab) as GameObject;
                m_instance.SetActive(false);
                m_behaviours = m_instance.GetComponentsInChildren<FxBehaviour>(true);
            }

            if (parent != null)
            {
                Vector3 position = m_instance.transform.localPosition;
                Quaternion rotation = m_instance.transform.localRotation;
                Vector3 scale = m_instance.transform.localScale;
                m_instance.transform.parent = parent;
                m_instance.transform.localPosition = position;
                m_instance.transform.localRotation = rotation;
                m_instance.transform.localScale = scale;
            }
        }

        public bool Activate(bool activate, FxCommand command)
        {
            if (m_instance != null)
            {
                if (m_behaviours.Length > 0)
                {
                    for (int i = 0; i < m_behaviours.Length; ++i)
                    {
                        m_behaviours[i].Activate(activate, command);
                    }
                }
                else
                {
                    m_instance.SetActive(activate);
                }
                return true;
            }

            Aube.Log.Warning(typeof(FxResource).Name + ": " + m_ident + "(" + ((m_prefab != null)? m_prefab.name : "?") + ") not instanced");
            return false;
        }
    #endregion
    #endregion
    }
}
