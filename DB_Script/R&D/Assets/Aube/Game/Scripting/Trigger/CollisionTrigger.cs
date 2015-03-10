using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	//! @class ScriptingCollisionTrigger
	//!
	//! @brief Trigger that could execute a function in the game object on collider events.
	[AddComponentMenu("Scripting/Trigger/Collision Trigger")]
	public class CollisionTrigger : MonoBehaviour
	{
		//! tag restrictions
		[SerializeField]
		string[] m_restrictionTags;

		//! events to execute on enter
		[SerializeField]
		ScriptingEvent[] m_onEnterEvents;
		//! events to execute on stay
		[SerializeField]
		ScriptingEvent[] m_onStayEvents;
		//! events to execute on exit
		[SerializeField]
		ScriptingEvent[] m_onExitEvents;

#region Private
		void Awake()
		{
			bool ok = true;
			Collider collider = GetComponent<Collider>();
			if(collider == null)
			{
				ok = false;
				Log.Error("The trigger " + name + " has no collider.");
			}
			else if(collider.isTrigger == false)
			{
				ok = false;
				Log.Error("The trigger " + name + " is using a collider that is not marked as isTrigger.");
			}

			if(ok == false)
			{
				enabled = false;
			}
		}

	#region Trigger Messages
		void OnTriggerEnter(Collider a_collider)
		{
			if(a_collider.gameObject != gameObject  &&  (m_restrictionTags == null  ||  m_restrictionTags.Length == 0  ||  m_restrictionTags.Contains(a_collider.tag)))
			{
				NotifyEvent(m_onEnterEvents);
			}
		}

		void OnTriggerExit(Collider a_collider)
		{
			if(a_collider.gameObject != gameObject  &&  (m_restrictionTags == null  ||  m_restrictionTags.Length == 0  ||  m_restrictionTags.Contains(a_collider.tag)))
			{
				NotifyEvent(m_onExitEvents);
			}
		}

		void OnTriggerStay(Collider a_collider)
		{
			if(a_collider.gameObject != gameObject  &&  (m_restrictionTags == null  ||  m_restrictionTags.Length == 0  ||  m_restrictionTags.Contains(a_collider.tag)))
			{
				NotifyEvent(m_onStayEvents);
			}
		}
	#endregion

	#region Notify Event
		void NotifyEvent(ScriptingEvent[] a_notifiers)
		{
			foreach(ScriptingEvent scriptingEvent in a_notifiers)
			{
				scriptingEvent.Invoke();
			}
		}
	#endregion
#endregion
	}
}