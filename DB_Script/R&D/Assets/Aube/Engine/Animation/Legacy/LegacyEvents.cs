using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Aube
{
    //! @class LegacyEvents
    //!
    //! @brief Used to create events in a legacy animation.
    public class LegacyEvents : MonoBehaviour
    {
    #region Declarations
    #region Public
        //! @class AnimClipEvent
        //!
        //! @brief Contains the events for an animation clip.
        [System.Serializable]
        public class AnimClipEvent
        {
            [SerializeField]
            private string m_clipName = null;
            [SerializeField]
            private LegacyEvent[] m_events = new LegacyEvent[0];
#if UNITY_EDITOR
            [SerializeField]
            [HideInInspector]
            private bool m_foldout = true;
#endif
            public string ClipName
            {
                get { return m_clipName; }
            }

            public AnimClipEvent(string clipName)
            {
                m_clipName = clipName;
            }

            public void Apply(Animation animation)
            {
                if (animation != null)
                {
                    for (int i = 0; i < m_events.Length; ++i)
                    {
                        m_events[i].AddTo(animation.GetClip(m_clipName));
                    }
                }
            }

            public void SortEvents()
            {
                System.Array.Sort(m_events, CompareEvent);
            }

            public static int CompareEvent(LegacyEvent item1, LegacyEvent item2)
            {
                return (int)(item1.Time - item2.Time);
            }
        }
    #endregion
    #endregion

    #region Attributes
    #region Private
        [SerializeField]
        private Animation m_animation = null;
        [SerializeField]
        private AnimClipEvent[] m_clips = new AnimClipEvent[0];
    #endregion
    #endregion

    #region Methods
    #region Private
        private void Awake()
        {
            for (int i = 0; i < m_clips.Length; ++i)
            {
                m_clips[i].Apply(m_animation);
            }
        }

        private void Synchronize()
        {
            List<AnimClipEvent> clips = new List<AnimClipEvent>();

            if (m_animation != null)
            {
                foreach (AnimationState state in m_animation) 
                {
                    bool found = false;

                    for (int i = 0; i < m_clips.Length && !found; ++i)
                    {
                        found = m_clips[i].ClipName == state.clip.name;

                        if (found)
                        {
                            clips.Add(m_clips[i]);
                        }
                    }

                    if (!found)
                    {
                        clips.Add(new AnimClipEvent(state.clip.name));
                    }
                }
            }

            m_clips = clips.ToArray();
        }

        private void SortEvents()
        {
            for (int i = 0; i < m_clips.Length; ++i)
            {
                m_clips[i].SortEvents();
            }
        }
    #endregion
    #endregion
    }
}