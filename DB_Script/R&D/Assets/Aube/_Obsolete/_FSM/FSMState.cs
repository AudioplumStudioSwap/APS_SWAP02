using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class FSMState
	//!
	//! @brief State of a Finite State Machine
	[System.Obsolete]
	public abstract class FSMState : MonoBehaviour
	{
	//*************************************************************************
	// Public Properties
	//*************************************************************************
		public bool IsActive
		{
			get{ return m_fsm.Current == this; }
		}

	//*************************************************************************
	// Protected Methods
	//*************************************************************************
		//! called on Awake

        protected virtual void OnDestroy()
        {
        }
        
		protected virtual void Awake()
		{
			// force disable
			enabled = false;
		}

		//! ask for State Machine to pop this state
		protected void PopState()
		{
			m_fsm.PopState(this);
		}

		//! ask for State Machine to push the state
		protected void PushState<t_State>(params object[] userdata) where t_State : FSMState
		{
			m_fsm.PushState<t_State>(this, userdata);
		}

		//! ask for State Machine to switch to state
		protected void ChangeState<t_State>(params object[] userdata) where t_State : FSMState
		{
			m_fsm.ChangeState<t_State>(this, userdata);
		}

	//*************************************************************************
	// Protected Methods
	//*************************************************************************
		//! called when the state is being stacked
		//!
		//! @param userdata		user data
		protected virtual bool Init(params object[] userdata)
		{
			return true;
		}

		//! called when the state is removing from stack
		protected virtual void Release()
		{
		}

		//! called when the state becomes active
		//!
		//! @param stateFrom	state from which the FSM is transiting
		protected virtual void Enter(FSMState stateFrom)
		{
			Assertion.Check(enabled == false, "FSM : Enter a state already enabled : " + GetType().Name);
			enabled = true;
		}

		//! called when the state becomes inactive
		//!
		//! @param stateTo		state to which the FSM is transiting
		protected virtual void Exit(FSMState stateTo)
		{
			// TODO re-activate this assertion when the state is not a behaviour anymore
			//Assertion.Check(enabled, "FSM : Exit a state not enabled : " + GetType().Name);
			enabled = false;
		}

		//! called when a message is processed by the State Machine
		//!
		//! @return true if the message has been processed
		protected virtual bool OnMessage(params object[] userdata)
		{
			return false;
		}

	//*************************************************************************
	// Internal Methods
	//*************************************************************************
		//! call init
		internal bool CallInit(params object[] userdata)
		{
			return Init(userdata);
		}

		//! call release
		internal void CallRelease()
		{
			Release();
		}

		//! call enter
		internal void CallEnter(FSMState stateFrom)
		{
			Enter(stateFrom);
		}

		//! call init
		internal void CallExit(FSMState stateTo)
		{
			Exit(stateTo);
		}

		//! called by the FSM on initialization
		internal void SetFSM(FSM fsm)
		{
			m_fsm = fsm;
		}

		//! called when a message is processed by the State Machine
		//!
		//! @return true if the message has been processed
		internal virtual bool CallOnMessage(params object[] userdata)
		{
			return OnMessage(userdata);
		}

	//*************************************************************************
	// Private Methods
	//*************************************************************************
#if UNITY_EDITOR
		//! this call is to avoid the sue of this method
		private void OnEnable()
		{

		}

		//! this call is to avoid the sue of this method
		private void OnDisable()
		{
			
		}
#endif // UNITY_EDITOR

	//*************************************************************************
	// Private Attributes
	//*************************************************************************
		//! FSM
		FSM m_fsm;
	}
} // namespace Aube