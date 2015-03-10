using UnityEngine;
using System.Collections;

namespace Aube
{
    //! @class FxBehaviourSound
    //!
    //! @brief Activate/deactivate a sound sequence controller.
    [RequireComponent(typeof(SoundSequenceController))]
    public class FxBehaviourSound : FxBehaviour
    {
    #region Methods
    #region Public
        private SoundSequenceController m_controller = null;
    #endregion
    #endregion

    #region Methods
    #region Public
        public override void Activate(bool activate, FxCommand command)
        {
            base.Activate(activate, command);

            if (m_controller != null)
            {
                if (activate)
                {
					m_controller.Stop();
                    m_controller.Play();
                }
                else
                {
                    m_controller.Stop();
                }
            }
        }
    #endregion
    #region Protected
        protected void Awake()
        {
            m_controller = GetComponent<SoundSequenceController>();
        }
    #endregion
    #endregion
    }
}