using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class TimeTrigger
	//!
	//! @brief Trigger that could execute a function periodically.
	[AddComponentMenu("Scripting/Trigger/Time Trigger")]
	public class TimeTrigger : MonoBehaviour
	{
		[SerializeField]
		float m_period;

		[SerializeField]
		float m_periodOffset;

		[SerializeField]
		ScriptingEvent[] m_onPeriodEvents;

#region Private
	#region Methods
		void OnEnable()
		{
			InitializeTimer();
		}

		void Update()
		{
			m_timer -= Time.deltaTime;

			if(m_timer <= 0.0f)
			{
				NotifyEvent(m_onPeriodEvents);
				InitializeTimer();
			}
		}

		void InitializeTimer()
		{
			m_timer = Random.Range(m_period - m_periodOffset, m_period + m_periodOffset);
		}
	#endregion

	#region Trigger Messages
		void NotifyEvent(ScriptingEvent[] a_notifiers)
		{
			foreach(ScriptingEvent scriptingEvent in a_notifiers)
			{
				scriptingEvent.Invoke();
			}
		}
	#endregion

	#region Attributes
		// time before next event
		float m_timer;
	#endregion
#endregion
	}
}