using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class SoundSequence
	//!
	//! @brief Sequence of sounds identified by their name with a transition offset between each of them.
	[System.Serializable]
	public class SoundSequence
	{
		[System.Serializable]
		public class SoundResource : ResourcePointer<AudioClip> {}
		
		[System.Serializable]
		public class Sound : ISerializationCallbackReceiver
		{
			[SerializeField][System.Obsolete("Version")]
			private string m_key;

			[SerializeField]
			private SoundResource m_audioClipResource = new SoundResource();

			public enum GarbagePolicy
			{
				Default,
				OnSequenceFinished,
				OnSoundFinished
			}

			[SerializeField]
			private GarbagePolicy m_garbagePolicy = GarbagePolicy.Default;

			public AudioClip clip
			{
				get{ return m_audioClipResource.Resource; }
			}

			public void SetClip(AudioClip a_clip)
			{
				m_audioClipResource.Set(a_clip);
			}

			public void SetClip(string a_clipPath)
			{
				m_audioClipResource.Set(a_clipPath);
			}

			public GarbagePolicy garbagePolicy
			{
				get{ return m_garbagePolicy; }
				set{ m_garbagePolicy = value; }
			}

			[System.Obsolete("Version")]
			public void OnBeforeSerialize()
			{

			}

			[System.Obsolete("Version")]
			public void OnAfterDeserialize()
			{
				if(string.IsNullOrEmpty(m_key) == false)
				{
					SetClip(m_key);
					m_key = "";
				}
			}
		}

		[SerializeField]
		private Sound m_defaultElement = new Sound();

		[SerializeField]
		private Sound[] m_nextElements = new Sound[0];

		[SerializeField]
		private float[] m_offsets = new float[0];

		[SerializeField]
		private float m_delayBegin;
		[SerializeField]
		private float m_delayEnd;

		[SerializeField]
		private float m_fadeInDuration;
		[SerializeField]
		private float m_fadeOutDuration;

		[SerializeField]
		private bool m_useRandomPitch = false;
		[SerializeField]
		private Vector2 m_randomPitchBoundaries = Vector2.one;			// each value must be between 1e-6f and 3.0f (Unity is broken with negative pitches)

		public float delayBegin
		{
			get{ return m_delayBegin; }
			set{ m_delayBegin = value; }
		}

		public float delayEnd
		{
			get{ return m_delayEnd; }
			set{ m_delayEnd = value; }
		}

		public float fadeInDuration
		{
			get{ return m_fadeInDuration; }
			set{ m_fadeInDuration = value; }
		}

		public float fadeOutDuration
		{
			get{ return m_fadeOutDuration; }
			set{ m_fadeOutDuration = value; }
		}

		public bool useRandomPitch
		{
			get{ return m_useRandomPitch; }
		}

		public float minRandomPitch
		{
			get{ return m_randomPitchBoundaries.x; }
		}

		public float maxRandomPitch
		{
			get{ return m_randomPitchBoundaries.y; }
		}

		public void UseRandomPitch(float a_minPitch, float a_maxPitch)
		{
			m_useRandomPitch = true;
			m_randomPitchBoundaries.y = Mathf.Clamp(a_maxPitch, 1e-6f, 3.0f);
			m_randomPitchBoundaries.x = Mathf.Clamp(a_minPitch, 1e-6f, m_randomPitchBoundaries.y);
		}

		public Sound AddElement()
		{
			System.Array.Resize(ref m_nextElements, m_nextElements.Length + 1);
			System.Array.Resize(ref m_offsets, m_offsets.Length + 1);

			m_nextElements[m_nextElements.Length - 1] = new Sound();
			return m_nextElements[m_nextElements.Length - 1];
		}

		public Sound GetElement(int a_index)
		{
			Aube.Assertion.Check(a_index >= 0  &&  a_index < length, "Invalid index.");
			if(a_index == 0)
			{
				return m_defaultElement;
			}
			else
			{
				return m_nextElements[a_index - 1];
			}
		}

		public float GetTransitionOffsetBefore(int a_index)
		{
			Aube.Assertion.Check(a_index >= 0  &&  a_index < length, "Invalid index.");
			return (a_index == 0)? 0.0f : m_offsets[a_index - 1];
		}

		public int length
		{
			get{ return m_nextElements.Length + 1; }
		}
	}
}
