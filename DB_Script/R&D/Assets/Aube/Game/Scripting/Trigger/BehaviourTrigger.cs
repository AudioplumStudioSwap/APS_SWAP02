using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class ScriptingBehaviourTrigger
	//!
	//! @brief Trigger that could execute a function in the game object on behaviour events.
	[AddComponentMenu("Scripting/Trigger/Behaviour Trigger")]
	public class BehaviourTrigger : MonoBehaviour
	{
		[SerializeField]
		private ScriptingEvent[] m_onStartEvents = null;
		[SerializeField]
		private ScriptingEvent[] m_onEnableEvents = null;
		[SerializeField]
		private ScriptingEvent[] m_onDisableEvents = null;

		void Start()
		{
			NotifyEvent(m_onStartEvents);
		}

		void OnEnable()
		{
			NotifyEvent(m_onEnableEvents);
		}

		void OnDisable()
		{
			NotifyEvent(m_onDisableEvents);
		}

#region Private
	#region Methods
		void NotifyEvent(ScriptingEvent[] a_notifiers)
		{
			if (a_notifiers != null)
			{
				foreach(ScriptingEvent scriptingEvent in a_notifiers)
				{
					scriptingEvent.Invoke();
				}
			}
		}
	#endregion
#endregion
	}
}
