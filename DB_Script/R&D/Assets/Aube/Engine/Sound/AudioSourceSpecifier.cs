using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class AudioSourceSpecifier
	//!
	//! @brief Component that specifies the kind of sounds played with an Audio Source component.
	//! It allows the modification of volumes depending on game options.
	[RequireComponent(typeof(AudioSource))]
	public class AudioSourceSpecifier : MonoBehaviour
	{
		[SerializeField]
		private AudioSourceCategory m_category = AudioSourceCategory.General;

		public AudioSourceCategory category
		{
			get{ return m_category; }
			set
			{
				if(m_category != value)
				{
					AudioSourceManager.UnregisterCallbackVolume(m_category, OnVolumeChange);
                    AudioSourceManager.UnregisterCallbackPause(m_category, OnPauseChange);
					
                    m_category = value;

					AudioSourceManager.RegisterCallbackVolume(m_category, OnVolumeChange);
                    AudioSourceManager.RegisterCallbackPause(m_category, OnPauseChange);
				}
			}
		}

#region Unity Callbacks
		private void Awake()
		{
			m_audioSource = GetComponent<AudioSource>();
			m_audioSourceVolume = m_audioSource.volume;
			m_audioSource.volume = m_audioSourceVolume * AudioSourceManager.GetVolume(m_category);

			AudioSourceManager.RegisterCallbackVolume(m_category, OnVolumeChange);
            AudioSourceManager.RegisterCallbackPause(m_category, OnPauseChange);
		}

		private void OnDestroy()
		{
			AudioSourceManager.UnregisterCallbackVolume(m_category, OnVolumeChange);
            AudioSourceManager.UnregisterCallbackPause(m_category, OnPauseChange);
		}
#endregion

#region Private
	#region Methods
		private void OnVolumeChange(float a_volume)
		{
			m_audioSource.volume = m_audioSourceVolume * a_volume;
		}

        private void OnPauseChange(bool pause)
        {
            if(pause)
            {
                m_audioSource.Pause();
            }
            else if(m_audioSource.time != 0.0f)
            {
                m_audioSource.Play();
            }
        }
	#endregion

	#region Attributes
		private AudioSource m_audioSource;
		private float m_audioSourceVolume;
	#endregion
#endregion
	}
}
