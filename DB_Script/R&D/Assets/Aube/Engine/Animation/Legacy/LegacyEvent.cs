using UnityEngine;
using System.Collections;

namespace Aube
{
    //! @class LegacyEvent
    //!
    //! @brief Defines an animation event.
    [System.Serializable]
    public class LegacyEvent
    {
    #region Attributes
    #region Private
        [SerializeField][Range(0, 100)]
        private float m_time = 0;
        [SerializeField]
        private string m_method = null;
        [SerializeField]
        private string m_param = null;
    #endregion
    #endregion

    #region Methods
    #region Public
        public float Time
        {
            get { return m_time; }
        }

        public void AddTo(AnimationClip clip)
        {
            AnimationEvent animEvent = new AnimationEvent();
            animEvent.functionName = m_method;
            animEvent.stringParameter = m_param;
            animEvent.time = m_time / 100.0f;
            clip.AddEvent(animEvent);
        }
    #endregion
    #endregion
    }
}
