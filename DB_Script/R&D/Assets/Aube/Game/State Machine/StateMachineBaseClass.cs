using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class StateMachine
	//!
	//! @brief State Machine editable by users
	public class StateMachineBaseClass : ScriptableObject
	{
		[SerializeField][HideInInspector]
		private StateMachineState[] m_states;
		[SerializeField][HideInInspector]
		private StateMachineTransition[] m_transitions;

		[SerializeField][HideInInspector]
		private int m_defaultStateIndex;

		internal StateMachineState defaultState
		{
			get{ return (m_states != null  &&  m_defaultStateIndex < m_states.Length)? m_states[m_defaultStateIndex] : null; }
		}
	}
}