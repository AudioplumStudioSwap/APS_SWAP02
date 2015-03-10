using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class FSMState
	//!
	//! @brief State of a Finite State Machine
	public abstract class HsmState
	{
		public bool isActive
		{
			get{ return m_stateMachine.currentState == this; }
		}

		public GameObject gameObject
		{
			get{ return m_stateMachine.gameObject; }
		}

#region Protected
	#region State Machine Requests
		//! @brief	ask for State Machine to pop this state
		protected void PopState()
		{
			m_stateMachine.PopState(this);
		}

		//! @brief	ask for State Machine to push the state
		//!
		//! @param	a_userdata	parameters to give to the state pushed
		protected void PushState<t_State>(params object[] a_userdata) where t_State : HsmState, new()
		{
			m_stateMachine.PushState<t_State>(this, a_userdata);
		}

		//! @brief	ask for State Machine to switch to state
		//!
		//! @param	a_userdata	parameters to give to the next state
		protected void ChangeState<t_State>(params object[] a_userdata) where t_State : HsmState, new()
		{
			m_stateMachine.ChangeState<t_State>(this, a_userdata);
		}
	#endregion

	#region State Management
		//! @brief	called when the state is being stacked
		//!
		//! @param a_userdata	parameters to initialize the state
		//!
		//! @return true if the state was successfully initialized.
		//!			If not, the method Release() will be called and the state will be destroyed.
		protected virtual bool Init(params object[] a_userdata)
		{
			return true;
		}

		//! @brief	called when the state is removing from stack.
		//!			The state will be destroyed after the end of this method.
		protected virtual void Release()
		{
		}

		//! @brief	called when the state becomes active
		//!
		//! @param a_stateFrom	state from which the HSM is transiting
		protected virtual void Enter(HsmState a_stateFrom)
		{
			Assertion.Check(isActive, "HSM : Enter a state that is not active : " + GetType().Name);
		}

		//! @brief	called when the state becomes inactive
		//!
		//! @param a_stateTo	state to which the HSM is transiting
		protected virtual void Exit(HsmState a_stateTo)
		{
			Assertion.Check(isActive, "HSM : Exit a state that is not active : " + GetType().Name);
		}

		//! @brief	called when a message is processed by the State Machine
		//!
		//! @param	a_userdata	parameters of the message
		//!
		//! @return true if the message has been processed
		protected virtual bool OnMessage(params object[] a_userdata)
		{
			return false;
		}

		//! @brief	called on Update
		protected virtual void Update() {}
	#endregion
#endregion

#region Internal
		//! @brief call init
		internal bool CallInit(params object[] userdata)
		{
#if _DEBUG
			CheckStatus(Status.Created, Status.Initialized);
#endif // _DEBUG
			return Init(userdata);
		}

		//! @brief call release
		internal void CallRelease()
		{
#if _DEBUG
			CheckStatus(Status.Initialized, Status.Released);
#endif // _DEBUG
			Release();
		}

		//! @brief call enter
		internal void CallEnter(HsmState stateFrom)
		{
#if _DEBUG
			CheckStatus(Status.Initialized, Status.Active);
#endif // _DEBUG
			Enter(stateFrom);
		}

		//! @brief call exit
		internal void CallExit(HsmState stateTo)
		{
#if _DEBUG
			CheckStatus(Status.Active, Status.Initialized);
#endif // _DEBUG
			Exit(stateTo);
		}

		//! @brief call update
		internal void CallUpdate()
		{
			Update();
		}

		//! @brief called by the HSM on initialization
		//
		//! @param	a_stateMachine	state machine in which the state is
		internal void SetStateMachine(HierarchicalStateMachine a_stateMachine)
		{
			m_stateMachine = a_stateMachine;
		}

		//! @brief called when a message is processed by the State Machine
		//!
		//! @param	a_userdata	parameters of the message
		//!
		//! @return true if the message has been processed
		internal virtual bool CallOnMessage(params object[] a_userdata)
		{
			return OnMessage(a_userdata);
		}
#endregion

#region Private
	#region Attributes
		//! HSM
		HierarchicalStateMachine m_stateMachine;

#if _DEBUG
		private enum Status
		{
			Created,
			Initialized,
			Active,
			Released,
		}

		private Status m_status = Status.Created;

		private void CheckStatus(Status a_statusFrom, Status a_statusTo)
		{
			Aube.Assertion.Check(m_status == a_statusFrom, "HSM : invalid call. The state is not in the correct status.");
			m_status = a_statusTo;
		}
#endif // _DEBUG

	#endregion
#endregion
	}
} // namespace Aube