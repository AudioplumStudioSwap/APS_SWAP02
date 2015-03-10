using UnityEngine;
using System.Collections;

namespace Aube
{
    //! @class FxBehaviour
    //!
    //! @brief Base class used to manage the components activated by an animation event.
    public abstract class FxBehaviour : MonoBehaviour
    {
    #region Methods
    #region Public
        public virtual void Activate(bool activate, FxCommand command)
        {
            if (activate && !gameObject.activeSelf)
            {
                gameObject.SetActive(activate);
            }
        }
    #endregion
    #endregion
    }
}