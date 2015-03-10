using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class StateMachineState
	//!
	//! @brief State of an editable state machine
	public class StateMachineState : StateMachineBaseClass
	{
#if UNITY_EDITOR
		[SerializeField][HideInInspector]
		private Vector2 m_position;
#endif // UNITY_EDITOR

		[SerializeField][HideInInspector]
		private StateMachineTransition[] m_nextTransitions;
		
		[SerializeField][HideInInspector]
		private GameObject m_componentHolder;

		public StateMachineTransition[] nextTransitions
		{
			get{ return m_nextTransitions; }
		}

		public GameObject componentHolder
		{
			get{ return m_componentHolder; }
		}
	}
}
