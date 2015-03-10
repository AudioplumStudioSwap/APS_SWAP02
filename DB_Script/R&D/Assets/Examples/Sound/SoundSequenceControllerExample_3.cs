using UnityEngine;
using System.Collections;

namespace Aube.Sandbox
{
	[Example("Sound/Sound Sequence Controller/Random loop/Random pitch")]
	[AddComponentMenu("")]
	
	//! @class SoundSequenceControllerExample_3
	//!
	//! @brief	Example with a SoundSequenceController component
	public class SoundSequenceControllerExample_3 : MonoBehaviour
	{
#region Unity Callbacks
		private void Start()
		{
			GameObject soundSequenceObject = new GameObject("SoundSequence");
			soundSequenceObject.SetActive(false);
			
			m_controller = soundSequenceObject.AddComponent<SoundSequenceController>();
			
			m_controller.playOnStart = true;
			m_controller.lastSequencePolicy = SoundSequenceController.LastSequencePolicy.None;
			m_controller.loopPolicy = SoundSequenceController.LoopPolicy.Repeat;
			
			// sequence 0
			{
				SoundSequence sequence0 = m_controller.AddSequence(1.0f);
				sequence0.delayBegin = 0.0f;
				sequence0.delayEnd = 0.0f;
				sequence0.fadeInDuration = 0.0f;
				sequence0.fadeOutDuration = 0.0f;
				sequence0.UseRandomPitch(1e-6f, 3.0f);
				
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