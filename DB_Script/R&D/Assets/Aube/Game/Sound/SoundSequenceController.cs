using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Aube
{
	//! @class SoundSequenceController
	//!
	//! @brief Controller of Sound Sequences.
	//! @details When enabled, this component will pick a random sequence between the one set, play it and then, disables itself.
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("Scripts/Sound/Sound Sequence Controller")]
	public class SoundSequenceController : MonoBehaviour
	{
		//! policy about the last sequence played
		public enum LastSequencePolicy
		{
			None,				//! the last sequence played can be picked again
			Exclude,			//!	the last sequence playerd can not be picked for the current choice
			ExcludePermanent,	//! the last sequence sequence can not be picked ever again
		}

		//! policy about how to loop
		public enum LoopPolicy
		{
			None,				//! no loop
			Repeat,				//! loop with a new pick
			RepeatSamePick,		//! loop keeping the sequence chosen
		}

		[SerializeField]
		private bool m_playOnStart;

		[SerializeField]
		private LastSequencePolicy m_lastSequencePolicy;
		
		[SerializeField]
		private LoopPolicy m_loopPolicy;

		[SerializeField]
		private SoundSequence[] m_sequences;

		[SerializeField][Range(0.0f, 1.0f)]
		private float[] m_weigths;

		[SerializeField]
		ScriptingEvent[] m_onSequenceBeginEvents = new ScriptingEvent[0];

		[SerializeField]
		ScriptingEvent[] m_onSequenceEndEvents = new ScriptingEvent[0];

		public bool playOnStart
		{
			get{ return m_playOnStart; }
			set{ m_playOnStart = value; }
		}

		public LastSequencePolicy lastSequencePolicy
		{
			get{ return m_lastSequencePolicy; }
			set{ m_lastSequencePolicy = value; }
		}

		public LoopPolicy loopPolicy
		{
			get{ return m_loopPolicy; }
			set{ m_loopPolicy = value; }
		}

		public SoundSequence AddSequence(float a_weight)
		{
			if(m_sequences == null)
			{
				m_sequences = new SoundSequence[1];
				m_weigths = new float[1];
			}
			else
			{
				System.Array.Resize(ref m_sequences, m_sequences.Length + 1);
				System.Array.Resize(ref m_weigths, m_weigths.Length + 1);
			}

			m_weigths[m_weigths.Length - 1] = Mathf.Clamp(a_weight, 0.0f, 1.0f);
			m_sequences[m_sequences.Length - 1] = new SoundSequence();
			return m_sequences[m_sequences.Length - 1];
		}

		public void Play()
		{
			PlayRandomSequence();
		}

		public void Stop()
		{
			m_audioSource.Stop();
			StopAllCoroutines();
			m_audioSource.volume = m_audioSourceVolume;
			NotifyEvent(m_onSequenceEndEvents);
		}

		public bool isPlaying
		{
			get{ return m_isPlaying; }
		}

#region Private
	#region Methods
		private void Awake()
		{
			m_audioSource = GetComponent<AudioSource>();
			m_audioSourceVolume = m_audioSource.volume;
			m_audioSourcePitch = m_audioSource.pitch;

			m_isPlaying = false;
			m_lastSequenceIndex = -1;

			m_clipToUnload_onSoundFinished = null;
			m_clipToUnload_onSequenceFinished = new List<AudioClip>();
		}

		private void Start()
		{
			if(m_playOnStart)
			{
				PlayRandomSequence();
			}
		}

		private void PlayRandomSequence()
		{
			int sequenceIndex = PickRandomSequence();
			StartCoroutine(PlaySequence(sequenceIndex));
		}

		private int PickRandomSequence()
		{
			Aube.Assertion.Check(m_sequences.Length == m_weigths.Length, "Synchronization problems between sequences and weights.");

			float totalWeight = 0.0f;
			int sequenceIndex = 0;
			for(sequenceIndex = 0; sequenceIndex < m_weigths.Length; ++sequenceIndex)
			{
				if(sequenceIndex != m_lastSequenceIndex  ||  m_lastSequencePolicy != LastSequencePolicy.Exclude)
				{
					totalWeight += m_weigths[sequenceIndex];
				}
			}

			if(totalWeight <= 0.0f)
			{
				return -1;
			}

			float random = Random.Range(0.0f, totalWeight);
			sequenceIndex = 0;
			float sequenceWeightCumul = 0.0f;
			while(random > sequenceWeightCumul + m_weigths[sequenceIndex]
			      ||  (m_lastSequencePolicy == LastSequencePolicy.Exclude  &&  sequenceIndex == m_lastSequenceIndex))
			{
				if(sequenceIndex != m_lastSequenceIndex  ||  m_lastSequencePolicy != LastSequencePolicy.Exclude)
				{
					sequenceWeightCumul += m_weigths[sequenceIndex];
				}
				++sequenceIndex;
			}

			return sequenceIndex;
		}

		private IEnumerator PlaySequence(int a_sequenceIndex)
		{
			m_isPlaying = true;
			NotifyEvent(m_onSequenceBeginEvents);

			if(a_sequenceIndex >= 0  &&  a_sequenceIndex < m_sequences.Length)
			{
				Aube.SoundSequence currentSequence = m_sequences[a_sequenceIndex];
				m_lastSequenceIndex = a_sequenceIndex;

				if(currentSequence.useRandomPitch)
				{
					m_audioSource.pitch = Random.Range(currentSequence.minRandomPitch, currentSequence.maxRandomPitch);
				}
				else
				{
					m_audioSource.pitch = m_audioSourcePitch;
				}

				if(currentSequence.delayBegin > 0.0f)
				{
					yield return new WaitForSeconds(currentSequence.delayBegin);
				}

				if(currentSequence.fadeInDuration > 0.0f)
				{
					StartCoroutine(FadeIn(currentSequence.fadeInDuration));
				}
				else
				{
					m_audioSource.volume = m_audioSourceVolume;
				}

				int currentSoundIndex = -1;
				while(currentSoundIndex < currentSequence.length)
				{
					if(m_audioSource.isPlaying)
					{
						yield return new WaitForFixedUpdate();
					}
					else
					{
						if(m_clipToUnload_onSoundFinished != null)
						{
							Aube.Assertion.Check(m_clipToUnload_onSoundFinished == m_audioSource.clip, "Clips are not equal.");
							Resources.UnloadAsset(m_audioSource.clip);
							m_clipToUnload_onSoundFinished = null;
						}
						m_audioSource.clip = null;
						
						++currentSoundIndex;
						if(currentSoundIndex < currentSequence.length)
						{
							float transition = currentSequence.GetTransitionOffsetBefore(currentSoundIndex);
							if(transition > 0.0f)
							{
								yield return new WaitForSeconds(transition);
							}
							SoundStart(currentSequence.GetElement(currentSoundIndex));

							if(currentSoundIndex == currentSequence.length - 1  &&  currentSequence.fadeOutDuration > 0.0f)
							{
								StartCoroutine(FadeOut(m_audioSource.clip.length, currentSequence.fadeOutDuration));
							}
						}
					}
				}

				if(currentSequence.delayEnd > 0.0f)
				{
					yield return new WaitForSeconds(currentSequence.delayEnd);
				}
				
				if(m_lastSequencePolicy == LastSequencePolicy.ExcludePermanent)
				{
					List<SoundSequence> sequenceList = new List<SoundSequence>(m_sequences);
					List<float> weightList = new List<float>(m_weigths);
					
					sequenceList.RemoveAt(m_lastSequenceIndex);
					weightList.RemoveAt(m_lastSequenceIndex);
					
					m_sequences = sequenceList.ToArray();
					m_weigths = weightList.ToArray();
					
					m_lastSequenceIndex = -1;
				}
			}
			else
			{
				m_lastSequenceIndex = -1;
			}

			while(m_clipToUnload_onSequenceFinished.Count > 0)
			{
				AudioClip audioClip = m_clipToUnload_onSequenceFinished[m_clipToUnload_onSequenceFinished.Count - 1];
				m_clipToUnload_onSequenceFinished.RemoveAt(m_clipToUnload_onSequenceFinished.Count - 1);
				if(audioClip != null)
				{
					Resources.UnloadAsset(audioClip);
				}
			}

			NotifyEvent(m_onSequenceEndEvents);

			switch(m_loopPolicy)
			{
				case LoopPolicy.RepeatSamePick:
				{
					if(m_lastSequenceIndex == -1)
					{
						Log.Warning(name + " : the previous sequence does not exist.");
						m_isPlaying = false;
					}
					else
					{
						StartCoroutine(PlaySequence(m_lastSequenceIndex));
					}
				}
				break;
				case LoopPolicy.Repeat:
				{
					if(m_sequences.Length > 0)
					{
						PlayRandomSequence();
					}
					else
					{
						m_isPlaying = false;
					}
				}
				break;
				default: m_isPlaying = false; break;
			}
		}

		private void SoundStart(SoundSequence.Sound a_sound)
		{
			AudioClip currentAudioClip = a_sound.clip;
			if(currentAudioClip == null)
			{
				Aube.Log.Warning("No audio clip found.");
			}
			else
			{
				switch(a_sound.garbagePolicy)
				{
					case SoundSequence.Sound.GarbagePolicy.Default: break;
					case SoundSequence.Sound.GarbagePolicy.OnSequenceFinished:
					{
						m_clipToUnload_onSequenceFinished.Add(currentAudioClip);
					}
					break;
					case SoundSequence.Sound.GarbagePolicy.OnSoundFinished:
					{
						m_clipToUnload_onSoundFinished = currentAudioClip;
					}
					break;
					default: Assertion.UnreachableCode(); break;
				}

				m_audioSource.clip = currentAudioClip;
				m_audioSource.Play();
			}
		}

		private void NotifyEvent(ScriptingEvent[] a_notifiers)
		{
			foreach(ScriptingEvent scriptingEvent in a_notifiers)
			{
				scriptingEvent.Invoke();
			}
		}

		private IEnumerator FadeIn(float a_duration)
		{
			m_audioSource.volume = 0.0f;

			float timeElapsed = 0.0f;
			while(timeElapsed < a_duration)
			{
				yield return null;
				timeElapsed += Time.deltaTime;
				m_audioSource.volume = Mathf.Clamp(m_audioSourceVolume * timeElapsed / a_duration, 0.0f, 1.0f);
			}

			m_audioSource.volume = m_audioSourceVolume;
		}

		private IEnumerator FadeOut(float clipDuration, float a_duration)
		{
			m_audioSource.volume = m_audioSourceVolume;
			
			float timeElapsed = clipDuration;
			while(timeElapsed > 0.0f)
			{
				yield return null;
				timeElapsed -= Time.deltaTime;

				float fadeOutTimeElapsed = timeElapsed - clipDuration + a_duration;
				m_audioSource.volume = Mathf.Clamp(m_audioSourceVolume * fadeOutTimeElapsed / a_duration, 0.0f, 1.0f);
			}
			
			m_audioSource.volume = 0.0f;
		}
	#endregion

	#region Attributes
		//! audio source
		private AudioSource m_audioSource;
		private float m_audioSourceVolume;
		private float m_audioSourcePitch;

		//! is playing
		private bool m_isPlaying;

		//! last sequence
		private int m_lastSequenceIndex;

		//! resource garbage
		private AudioClip m_clipToUnload_onSoundFinished;
		private List<AudioClip> m_clipToUnload_onSequenceFinished;
	#endregion
#endregion
	}
}
