using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class AudioSourceManager
	//!
	//! @brief Manager that save preferences for volume for each audio source category.
	public static class AudioSourceManager
	{
		static AudioSourceManager()
		{
			int categoryCount = System.Enum.GetValues(typeof(AudioSourceCategory)).Length;

			m_volumes = new float[categoryCount];
			m_volumes.Populate(1.0f);
            m_volumeCallbacks = new HashSet<System.Action<float>>[categoryCount];
            m_volumeCallbacks.Populate(null);
            m_pauseCallbacks = new HashSet<System.Action<bool>>[categoryCount];
            m_pauseCallbacks.Populate(null);

			m_preferencesLoaded = false;
		}

		public static void SavePreferences()
		{
			foreach(AudioSourceCategory category in System.Enum.GetValues(typeof(AudioSourceCategory)))
			{
				PlayerPrefs.SetFloat("Sounds|Volume|" + category.ToString(), m_volumes[(int)category]);
			}
		}

		public static void LoadPreferences()
		{
			foreach(AudioSourceCategory category in System.Enum.GetValues(typeof(AudioSourceCategory)))
			{
				m_volumes[(int)category] = PlayerPrefs.GetFloat("Sounds|Volume|" + category.ToString(), 1.0f);
			}

			m_preferencesLoaded = true;
		}

		public static void SetVolume(AudioSourceCategory a_category, float a_volume)
		{
			float volume = Mathf.Clamp(a_volume, 0.0f, 1.0f);
			m_volumes[(int)a_category] = volume;

            if (m_volumeCallbacks[(int)a_category] != null)
			{
                foreach (System.Action<float> callback in m_volumeCallbacks[(int)a_category])
				{
					callback(volume);
				}
			}
		}

		public static float GetVolume(AudioSourceCategory a_category)
		{
			if(m_preferencesLoaded == false)
			{
				LoadPreferences();
			}

			return m_volumes[(int)a_category];
		}

        public static void Pause(AudioSourceCategory a_category, bool pause)
        {
            if (m_pauseCallbacks[(int)a_category] != null)
            {
                foreach (System.Action<bool> callback in m_pauseCallbacks[(int)a_category])
                {
                    callback(pause);
                }
            }
        }

		public static void RegisterCallbackVolume(AudioSourceCategory a_category, System.Action<float> a_callback)
		{
            if (m_volumeCallbacks[(int)a_category] == null)
			{
                m_volumeCallbacks[(int)a_category] = new HashSet<System.Action<float>>();
			}

            m_volumeCallbacks[(int)a_category].Add(a_callback);
		}

		public static void UnregisterCallbackVolume(AudioSourceCategory a_category, System.Action<float> a_callback)
		{
            m_volumeCallbacks[(int)a_category].Remove(a_callback);
		}

        public static void RegisterCallbackPause(AudioSourceCategory a_category, System.Action<bool> a_callback)
        {
            if (m_pauseCallbacks[(int)a_category] == null)
            {
                m_pauseCallbacks[(int)a_category] = new HashSet<System.Action<bool>>();
            }

            m_pauseCallbacks[(int)a_category].Add(a_callback);
        }

        public static void UnregisterCallbackPause(AudioSourceCategory a_category, System.Action<bool> a_callback)
        {
            m_pauseCallbacks[(int)a_category].Remove(a_callback);
        }

#region Private
		private static bool m_preferencesLoaded;

		private static float[] m_volumes;
		private static HashSet<System.Action<float>>[] m_volumeCallbacks;
        private static HashSet<System.Action<bool>>[] m_pauseCallbacks;
#endregion
	}
}
