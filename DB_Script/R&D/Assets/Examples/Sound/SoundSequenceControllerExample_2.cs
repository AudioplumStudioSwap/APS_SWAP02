using UnityEngine;
using System.Collections;

namespace Aube.Sandbox
{
	[Example("Sound/Sound Sequence Controller/Random loop/garbage policy")]
	[AddComponentMenu("")]
	
	//! @class SoundSequenceControllerExample_2
	//!
	//! @brief	Example with a SoundSequenceController component
	public class SoundSequenceControllerExample_2 : MonoBehaviour
	{
#region Unity Callbacks
		private void Start()
		{
			GameObject soundSequenceObject = new GameObject("SoundSequence");
			soundSequenceObject.SetActive(false);
			
			m_controller = soundSequenceObject.AddComponent<SoundSequenceController>();
			
			m_controller.playOnStart = true;
			m_controller.lastSequencePolicy = SoundSequenceController.LastSequencePolicy.Exclude;
			m_controller.loopPolicy = SoundSequenceController.LoopPolicy.Repeat;
			
			// sequence 0
			{
				SoundSequence sequence0 = m_controller.AddSequence(0.6f);
				sequence0.delayBegin = 0.0f;
				sequence0.delayEnd = 0.0f;
				sequence0.fadeInDuration = 0.0f;
				sequence0.fadeOutDuration = 0.0f;
				
				SoundSequence.Sound sequence0_sound0 = sequence0.GetElement(0);
				sequence0_sound0.SetClip("Sounds/sound_01");
				sequence0_sound0.garbagePolicy = SoundSequence.Sound.GarbagePolicy.OnSequenceFinished;
				SoundSequence.Sound sequence0_sound1 = sequence0.AddElement();
				sequence0_sound1.SetClip("Sounds/sound_02");
				sequence0_sound1.garbagePolicy = SoundSequence.Sound.GarbagePolicy.OnSequenceFinished;
				SoundSequence.Sound sequence0_sound2 = sequence0.AddElement();
				sequence0_sound2.SetClip("Sounds/sound_03");
				sequence0_sound2.garbagePolicy = SoundSequence.Sound.GarbagePolicy.OnSequenceFinished;
			}
			// sequence 1
			{
				SoundSequence sequence1 = m_controller.AddSequence(0.3f);
				sequence1.delayBegin = 0.0f;
				sequence1.delayEnd = 0.0f;
				sequence1.fadeInDuration = 0.0f;
				sequence1.fadeOutDuration = 0.0f;
				
				SoundSequence.Sound sequence1_sound0 = sequence1.GetElement(0);
				sequence1_sound0.SetClip("Sounds/sound_04");
				sequence1_sound0.garbagePolicy = SoundSequence.Sound.GarbagePolicy.OnSoundFinished;
				SoundSequence.Sound sequence1_sound1 = sequence1.AddElement();
				sequence1_sound1.SetClip("Sounds/sound_05");
				sequence1_sound1.garbagePolicy = SoundSequence.Sound.GarbagePolicy.OnSoundFinished;
			}
			// sequence 2
			{
				SoundSequence sequence2 = m_controller.AddSequence(0.1f);
				sequence2.delayBegin = 0.0f;
				sequence2.delayEnd = 0.0f;
				sequence2.fadeInDuration = 0.0f;
				sequence2.fadeOutDuration = 0.0f;
				
				SoundSequence.Sound sequence2_sound0 = sequence2.GetElement(0);
				sequence2_sound0.SetClip("Sounds/sound_06");
			}
			
			soundSequenceObject.SetActive(true);
		}

		private void OnGUI()
		{
			if(m_controller != null)
			{
				GUILayout.Label("Controller is playing : " + m_controller.isPlaying);
			}
		}
#endregion

		private SoundSequenceController m_controller;
	}
}