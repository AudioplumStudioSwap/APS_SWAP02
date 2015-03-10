#if !AUBE_NO_UI
using UnityEngine;
using System.Collections;

namespace Aube
{
    //! @class UIResourceDescBase
    //!
    //! @brief Base class to propose a custom property drawer
    public abstract class UIResourceDescBase : MonoBehaviour
    {
    #region Declarations
    #region Public
        [System.Serializable]
        public class Resource
        {
            [SerializeField]
            public PrefabPointer m_prefab = new PrefabPointer();
            [System.NonSerialized]
            public GameObject m_instance = null;
#if UNITY_EDITOR
            [HideInInspector]
            public bool m_foldout = true;
#endif
        }
    #endregion
    #endregion
    }
}
#endif // !AUBE_NO_UI