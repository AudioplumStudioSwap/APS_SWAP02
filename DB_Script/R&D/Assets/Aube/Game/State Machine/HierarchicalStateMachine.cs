#if _DEBUG  ||  UNITY_EDITOR
#define HSM_LOG
#endif // UNITY_EDITOR

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Aube
{
	//! @class HierarchicalStateMachine
	//!
	//! @brief State Machine with hierarchical states
	public sealed class HierarchicalStateMachine 
	{
		public enum ProcessMessageOptions
		{
			RequireReceiver,
			DoNotRequireReceiver,
		}

		//! @brief constructs a State Machine for the game object
		//!
		//! @param	a_gameObject	game object in which the state machine is	
		public HierarchicalStateMachine(GameObject a_gameObject)
		{
			Assertion.Check(a_gameObject != null, "Invalid HSM initialization.");

			// init attributes
			m_gameObject = a_gameObject;
			m_stackedStates = new Stack<HsmState>();
			m_busy = false;
			m_queuedEvent = null;
		}

		//! @brief	switch root state
		//!
		//! @tparam t_State		the state to which the HSM is transiting
		//! @params a_userdata 	user data
		public void SwitchRootState<t_State>(params object[] a_userdata) where t_State : HsmState, new()
		{
			if(m_busy)
			{
				EnqueueEvent(Curry.Bind(_SwitchRootState<t_State>, a_userdata));
			}
			else
			{
				_SwitchRootState<t_State>(a_userdata);
			}
		}

		//! @brief send a message to the current state
		//!
		//! @params a_userdata	user data
		//!
		//! @return true if the message has been processed, false otherwise
		public bool ProcessMessage(params object[] a_userdata)
		{
			return ProcessMessage(ProcessMessageOptions.RequireReceiver, a_userdata);
		}

		//! @brief send a message to the current state
		//!
		//! @params a_options	process options
		//! @params a_userdata	user data
		//!
		//! @return true if the message has been processed, false otherwise
		public bool ProcessMessage(ProcessMessageOptions a_options, params object[] a_userdata)
		{
			string currentStateName = currentState.GetType().Name;

			bool messageProcessed = currentState.CallOnMessage(a_userdata);
			if(messageProcessed == false  &&  a_options == ProcessMessageOptions.RequireReceiver)
			{
				string parameters = "";
				for(int paramIndex = 0; paramIndex < a_userdata.Length; ++paramIndex)
				{
					parameters += a_userdata[paramIndex] != null? a_userdata[paramIndex].ToString() : "null";
					if(paramIndex < a_userdata.Length - 1)
					{
						parameters += " | ";
					}
				}
				Log.Error("HSM (" + m_gameObject.name + ") : a message request was not processed current state " + currentStateName + ".\nmessage : " + parameters);
			}
			return messageProcessed;
		}

		public void Update()
		{
			if(currentState != null)
			{
				currentState.CallUpdate();
			}
		}

		//! @brief Clear the stack
		public void Clear()
		{
			if(m_busy)
			{
				EnqueueEvent(_Clear);
			}
			else
			{
				_Clear();
			}
		}

		public HsmState currentState
		{
			get{ return (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null; }
		}

		public GameObject gameObject
		{
			get{ return m_gameObject; }
		}

#region Internal
		//! @brief	push a state over the current state
		//!
		//! @tparam t_State			the state to which the HSM is transiting
		//! @param	a_stateAsking	state asking this change
		//! @params a_userdata		user data 
		internal void PushState<t_State>(HsmState a_stateAsking, params object[] a_userdata) where t_State : HsmState, new()
		{
			Assertion.Check(currentState == a_stateAsking, "Invalid request from state " + a_stateAsking.GetType().Name);
			if(m_busy)
			{
				EnqueueEvent(Curry.Bind(_PushState<t_State>, a_userdata));
			}
			else
			{
				_PushState<t_State>(a_userdata);
			}
		}

		//! @brief	push a state over the current state
		//!
		//! @param	stateAsking		state asking this change
		internal void PopState(HsmState a_stateAsking)
		{
			Assertion.Check(currentState == a_stateAsking, "Invalid request from state " + a_stateAsking.GetType().Name);
			if(m_busy)
			{
				EnqueueEvent(_PopState);
			}
			else
			{
				_PopState();
			}
		}

		//! @brief	change the current state to this state
		//!
		//! @tparam t_State			the state to which the HSM is transiting
		//! @param	a_stateAsking	state asking this change
		//! @params a_userdata 		user data
		internal void ChangeState<t_State>(HsmState a_stateAsking, params object[] a_userdata) where t_State : HsmState, new()
		{
			Assertion.Check(currentState == a_stateAsking, "Invalid request from state " + a_stateAsking.GetType().Name);
			if(m_busy)
			{
				EnqueueEvent(Curry.Bind(_ChangeState<t_State>, a_userdata));
			}
			else
			{
				_ChangeState<t_State>(a_userdata);
			}
		}
#endregion

#region Private
		//! @brief	push a state over the current state
		//!
		//! @tparam t_State		the state to which the HSM is transiting
		//! @params a_userdata	user data 
		private void _PushState<t_State>(params object[] a_userdata) where t_State : HsmState, new()
		{
			Assertion.Check(m_busy == false, "The State Machine is not in a valid state to push a state.");
			m_busy = true;

			HsmState stateTo = GetState<t_State>();

			if(stateTo.CallInit(a_userdata))
			{
				HsmState stateFrom = (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null;
				if(stateFrom != null)
				{
					stateFrom.CallExit(stateTo);
				}
				m_stackedStates.Push(stateTo);
				stateTo.CallEnter(stateFrom);

#if HSM_LOG
				Log.Info("HSM (" + m_gameObject.name + ") : Push State : " + LogStateName(stateFrom) + " -> " + LogStateName(stateTo) + ".");
#endif // HSM_LOG
			}
			else
			{
				string parameters = "";
				for(int paramIndex = 0; paramIndex < a_userdata.Length; ++paramIndex)
				{
					parameters += a_userdata[paramIndex] != null? a_userdata[paramIndex].ToString() : "null";
					if(paramIndex < a_userdata.Length - 1)
					{
						parameters += " | ";
					}
				}
				Log.Error("HSM (" + m_gameObject.name + ") : state " + typeof(t_State).Name + " failed to initialize with parameters (" + parameters + ")");
				stateTo.CallRelease();
			}

			m_busy = false;
			ProcessEvents();
		}

		//! @brief pop the current state from the stack
		private void _PopState()
		{
			Assertion.Check(m_busy == false, "The State Machine is not in a valid state to pop a state.");
			m_busy = true;

			Assertion.Check(m_stackedStates.Count > 0, "There is no current state.");
			HsmState stateFrom = m_stackedStates.Pop();

			HsmState stateTo = (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null;

			// we must push the state from again to be sure it is the current one during Exit calls
			m_stackedStates.Push(stateFrom);
			stateFrom.CallExit(stateTo);
			m_stackedStates.Pop();

			if (stateTo != null)
			{
				stateTo.CallEnter(stateFrom);
			}
			stateFrom.CallRelease();

#if HSM_LOG
			Log.Info("HSM (" + m_gameObject.name + ") : Pop State : " + LogStateName(stateFrom) + " -> " + LogStateName(stateTo) + ".");
#endif // HSM_LOG

			m_busy = false;
			ProcessEvents();
		}

		//! @brief change the current state
		//!
		//! @tparam t_State		the state to which the HSM is transiting
		//! @params	userdata	user data
		private void _ChangeState<t_State>(params object[] userdata) where t_State : HsmState, new()
		{
			Assertion.Check(m_busy == false, "The State Machine is not in a valid state to change a state.");
			m_busy = true;

			HsmState stateTo = GetState<t_State>();

			if(stateTo.CallInit(userdata))
			{
				HsmState stateFrom = (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null;
				if(stateFrom != null)
				{
					stateFrom.CallExit(stateTo);
					m_stackedStates.Pop();
				}

				m_stackedStates.Push(stateTo);
				stateTo.CallEnter(stateFrom);

				if(stateFrom != null)
				{
					stateFrom.CallRelease();
				}

#if HSM_LOG
				Log.Info("HSM (" + m_gameObject.name + ") : Change State : " + LogStateName(stateFrom) + " -> " + LogStateName(stateTo) + ".");
#endif // HSM_LOG
			}
			else
			{
				Log.Error("HSM : state " + typeof(t_State).Name + " failed to initialize.");
				stateTo.CallRelease();
			}

			m_busy = false;
			ProcessEvents();
		}

		//! @brief clear the stack
		private void _Clear()
		{
			while(m_stackedStates.Count > 0)
			{
				_PopState();
			}
		}

		//! @brief switch root state
		private void _SwitchRootState<t_State>(params object[] a_userdata) where t_State : HsmState, new()
		{
			_Clear();
			_PushState<t_State>(a_userdata);
		}

		//! @brief enqueue a request
		private void EnqueueEvent(System.Action a_action)
		{
			Assertion.Check(HasQueuedEvent() == false, "Invalid request");
			m_queuedEvent = a_action;
		}

		//! @brief process the queued event
		private void ProcessEvent()
		{
			Assertion.Check(HasQueuedEvent(), "Invalid request");
			System.Action queuedEvent = m_queuedEvent;
			m_queuedEvent = null;

			queuedEvent();
		}

		//! @brief check if there is a queued event
		private bool HasQueuedEvent()
		{
			return m_queuedEvent != null;
		}

		//! @brief process all queued events
		private void ProcessEvents()
		{
			while(HasQueuedEvent())
			{
				ProcessEvent();
			}
		}

		//! @brief get the state from his type
		private HsmState GetState<t_State>() where t_State : HsmState, new()
		{
			HsmState state = new t_State();
			state.SetStateMachine(this);
			return state;
		}

#if HSM_LOG
		private static string LogStateName(HsmState state)
		{
			if(state == null)
			{
				return "NONE";
			}
			else
			{
				return state.GetType().Name;
			}
		}
#endif // HSM_LOG

	#region Attributes
		//! owner
		GameObject m_gameObject;

		//! stack of states
		Stack<HsmState> m_stackedStates;

		//! queued events
		bool m_busy;
		System.Action m_queuedEvent;
	#endregion
#endregion
	}
} // namespace Aube