#if _DEBUG  ||  UNITY_EDITOR
#define FSM_LOG
#endif // UNITY_EDITOR

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Aube
{
	//! @class FSM
	//!
	//! @brief Finite State Machine
	[Obsolete]
	public class FSM 
	{
		public enum ProcessMessageOptions
		{
			RequireReceiver,
			DoNotRequireReceiver,
		}

	//*************************************************************************
	// Constructors
	//*************************************************************************
		//! @brief constructs a FSM for the game object
		public FSM(GameObject go)
		{
			Assertion.Check(go != null, "Invalid FSM initialization.");

			// init attributes
			m_gameObject = go;
			m_cachedStates = new Dictionary<Type, FSMState>();
			m_stackedStates = new Stack<FSMState>();
			m_busy = false;
			m_queuedEvent = null;
		}

		//! @brief initialize the state machine
		public void Init<t_State>() where t_State : FSMState
		{
			// init from game object
			t_State[] states = m_gameObject.GetComponents<t_State>();
			foreach(FSMState state in states)
			{
				m_cachedStates.Add(state.GetType(), state);
				state.SetFSM(this);
			}
		}

        //! @brief add the states in the state machine
        public List<FSMState> CreateStates(System.Type[] states)
        {
            List<FSMState> result = new List<FSMState>();

            for (int i = 0; i < states.Length; ++i)
            {
                if (typeof(Aube.GameState).IsAssignableFrom(states[i]))
                {
                    FSMState state;

                    if (!m_cachedStates.TryGetValue(states[i], out state))
                    {
                        state = m_gameObject.AddComponent(states[i].ToString()) as FSMState;
                        state.SetFSM(this);
                        m_cachedStates.Add(states[i], state);
                    }
                }
                else
                {
                    Assertion.Check(false, "invalid state");
                }
            }

            foreach(KeyValuePair<Type, FSMState> state in m_cachedStates)
            {
                result.Add(state.Value);
            }
            return result;
        }

	//*************************************************************************
	// Public Methods
	//*************************************************************************
		//! @brief	switch root state
		//!
		//! @tparam t_State		the state to which the FSM is transiting
		//! @params userdata 	user data
		public void SwitchRootState<t_State>(params object[] userdata) where t_State : FSMState
		{
			if(m_busy)
			{
				EnqueueEvent(Curry.Bind(_SwitchRootState<t_State>, userdata));
			}
			else
			{
				_SwitchRootState<t_State>(userdata);
			}
		}

		//! @brief send a message to the current state
		//!
		//! @params userdata	user data
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
			string currentStateName = Current.GetType().Name;

			bool messageProcessed = Current.CallOnMessage(a_userdata);
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
				Log.Error("FSM (" + m_gameObject.name + ") : a message request was not processed current state " + currentStateName + ".\nmessage : " + parameters);
			}
			return messageProcessed;
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

	//*************************************************************************
	// Public Properties
	//*************************************************************************
		public FSMState Current
		{
			get{ return (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null; }
		}

		public bool IsActivated<T>() where T : FSMState
		{
			foreach (FSMState state in m_stackedStates)
			{
				if (typeof(T) == state.GetType())
				{
					return true;
				}
			}
			return false;
		}


	//*************************************************************************
	// Internal Methods
	//*************************************************************************
		//! @brief	push a state over the current state
		//!
		//! @tparam t_State			the state to which the FSM is transiting
		//! @param	stateAsking		state asking this change
		//! @params userdata		user data 
		internal void PushState<t_State>(FSMState stateAsking, params object[] userdata) where t_State : FSMState
		{
			Assertion.Check(Current == stateAsking, "Invalid request from state " + stateAsking.GetType().Name);
			if(m_busy)
			{
				EnqueueEvent(Curry.Bind(_PushState<t_State>, userdata));
			}
			else
			{
				_PushState<t_State>(userdata);
			}
		}

		//! @brief	push a state over the current state
		//!
		//! @param	stateAsking		state asking this change
		internal void PopState(FSMState stateAsking)
		{
			Assertion.Check(Current == stateAsking, "Invalid request from state " + stateAsking.GetType().Name);
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
		//! @tparam t_State			the state to which the FSM is transiting
		//! @param	stateAsking		state asking this change
		//! @params userdata 		user data
		internal void ChangeState<t_State>(FSMState stateAsking, params object[] userdata) where t_State : FSMState
		{
			Assertion.Check(Current == stateAsking, "Invalid request from state " + stateAsking.GetType().Name);
			if(m_busy)
			{
				EnqueueEvent(Curry.Bind(_ChangeState<t_State>, userdata));
			}
			else
			{
				_ChangeState<t_State>(userdata);
			}
		}

	//*************************************************************************
	// Private Methods
	//*************************************************************************
		//! @brief	push a state over the current state
		//!
		//! @tparam t_State		the state to which the FSM is transiting
		//! @params userdata		user data 
		void _PushState<t_State>(params object[] userdata) where t_State : FSMState
		{
			Assertion.Check(m_busy == false, "The State Machine is not in a valid state to push a state.");
			m_busy = true;

			FSMState stateTo = GetState<t_State>();

			if(stateTo.CallInit(userdata))
			{
				FSMState stateFrom = (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null;
				if(stateFrom != null)
				{
					stateFrom.CallExit(stateTo);
				}
				m_stackedStates.Push(stateTo);
				stateTo.CallEnter(stateFrom);

#if FSM_LOG
				Log.Info("FSM (" + m_gameObject.name + ") : Push State : " + LogStateName(stateFrom) + " -> " + LogStateName(stateTo) + ".");
#endif // FSM_LOG
			}
			else
			{
				string parameters = "";
				for(int paramIndex = 0; paramIndex < userdata.Length; ++paramIndex)
				{
					parameters += userdata[paramIndex] != null? userdata[paramIndex].ToString() : "null";
					if(paramIndex < userdata.Length - 1)
					{
						parameters += " | ";
					}
				}
				Log.Error("FSM (" + m_gameObject.name + ") : state " + typeof(t_State).Name + " failed to initialize with parameters (" + parameters + ")");
				stateTo.CallRelease();
			}

			m_busy = false;
			ProcessEvents();
		}

		//! @brief pop the current state from the stack
		void _PopState()
		{
			Assertion.Check(m_busy == false, "The State Machine is not in a valid state to pop a state.");
			m_busy = true;

			Assertion.Check(m_stackedStates.Count > 0, "There is no current state.");
			FSMState stateFrom = m_stackedStates.Pop();

			FSMState stateTo = (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null;

			// we must push the state from again to be sure it is the current one during Exit calls
			m_stackedStates.Push(stateFrom);
			stateFrom.CallExit(stateTo);
			m_stackedStates.Pop();

			if (stateTo != null)
			{
				stateTo.CallEnter(stateFrom);
			}
			stateFrom.CallRelease();

#if FSM_LOG
			Log.Info("FSM (" + m_gameObject.name + ") : Pop State : " + LogStateName(stateFrom) + " -> " + LogStateName(stateTo) + ".");
#endif // FSM_LOG

			m_busy = false;
			ProcessEvents();
		}

		//! @brief change the current state
		//!
		//! @tparam t_State		the state to which the FSM is transiting
		//! @params	userdata	user data
		void _ChangeState<t_State>(params object[] userdata) where t_State : FSMState
		{
			Assertion.Check(m_busy == false, "The State Machine is not in a valid state to change a state.");
			m_busy = true;

			FSMState stateTo = GetState<t_State>();

			if(stateTo.CallInit(userdata))
			{
				FSMState stateFrom = (m_stackedStates.Count > 0)? m_stackedStates.Peek() : null;
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

#if FSM_LOG
				Log.Info("FSM (" + m_gameObject.name + ") : Change State : " + LogStateName(stateFrom) + " -> " + LogStateName(stateTo) + ".");
#endif // FSM_LOG
			}
			else
			{
				Log.Error("FSM : state " + typeof(t_State).Name + " failed to initialize.");
				stateTo.CallRelease();
			}

			m_busy = false;
			ProcessEvents();
		}

		//! @brief clear the stack
		void _Clear()
		{
            if (m_stackedStates.Count > 0)
            {
                FSMState currentState = m_stackedStates.Pop();
                currentState.CallExit(null);
                currentState.CallRelease();

                while (m_stackedStates.Count > 0)
                {
                    FSMState stateFrom = m_stackedStates.Pop();
                    stateFrom.CallRelease();
                }
            }
		}

		//! @brief switch root state
		void _SwitchRootState<t_State>(params object[] a_userdata) where t_State : FSMState 
		{
			_Clear();
			_PushState<t_State>(a_userdata);
		}

		//! @brief enqueue a request
		void EnqueueEvent(System.Action action)
		{
			Assertion.Check(HasQueuedEvent() == false, "Invalid request");
			m_queuedEvent = action;
		}

		//! @brief process the queued event
		void ProcessEvent()
		{
			Assertion.Check(HasQueuedEvent(), "Invalid request");
			System.Action queuedEvent = m_queuedEvent;
			m_queuedEvent = null;

			queuedEvent();
		}

		//! @brief check if there is a queued event
		bool HasQueuedEvent()
		{
			return m_queuedEvent != null;
		}

		//! @brief process all queued events
		void ProcessEvents()
		{
			while(HasQueuedEvent())
			{
				ProcessEvent();
			}
		}

		//! @brief get the state from his type
		FSMState GetState<t_State>() where t_State : FSMState
		{
			FSMState state;
			bool stateFound = m_cachedStates.TryGetValue(typeof(t_State), out state);
			if(stateFound == false)
			{
				state = m_gameObject.AddComponent<t_State>() as FSMState;
				state.SetFSM(this);
				m_cachedStates.Add(typeof(t_State), state);
			}

			return state;
		}

#if FSM_LOG
		static string LogStateName(FSMState state)
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
#endif // FSM_LOG

	//*************************************************************************
	// Private Attributes
	//*************************************************************************
		//! owner
		GameObject m_gameObject;
		Dictionary<Type, FSMState> m_cachedStates;

		//! stack of states
		Stack<FSMState> m_stackedStates;

		//! queued events
		bool m_busy;
		System.Action m_queuedEvent;
	}
} // namespace Aube
