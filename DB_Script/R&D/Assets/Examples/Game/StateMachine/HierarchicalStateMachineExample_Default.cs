using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Aube.Sandbox
{
	[Example("Game/State Machine/HSM/Default")]
	[AddComponentMenu("")]

	//! @class HierarchicalStateMachineExample_Default
	//!
	//! @brief Example that show a hierarchical state machine
	class HierarchicalStateMachineExample_Default : MonoBehaviour
	{
		public void OnInit(string a_stateName)
		{
			m_states.Add(a_stateName);
			m_activeFlags.Add(false);
		}

		public void OnRelease(string a_stateName)
		{
			int index = m_states.IndexOf(a_stateName);
			Assertion.Check(index >= 0  &&  index < m_states.Count  &&  index >= m_states.Count - 2, "Error in example.");
			m_states.RemoveAt(index);
			m_activeFlags.RemoveAt(index);
		}

		public void OnEnter(string a_stateName)
		{
			int index = m_states.IndexOf(a_stateName);
			Assertion.Check(index >= 0  &&  index < m_states.Count  &&  index >= m_states.Count - 2, "Error in example.");
			Assertion.Check(m_activeFlags.Count > 0  &&  m_activeFlags[index] == false, "Error in example.");
			m_activeFlags[index] = true;
		}

		public void OnExit(string a_stateName)
		{
			int index = m_states.IndexOf(a_stateName);
			Assertion.Check(index >= 0  &&  index < m_states.Count  &&  index >= m_states.Count - 2, "Error in example.");
			Assertion.Check(m_activeFlags.Count > 0  &&  m_activeFlags[index], "Error in example.");
			m_activeFlags[index] = false;
		}

#region Unity Callbacks
		private IEnumerator Start()
		{
			m_states = new List<string>();
			m_activeFlags = new List<bool>();

			m_stateMachine = new HierarchicalStateMachine(gameObject);
			m_stateMachine.SwitchRootState<HsmExampleStateDefault>("Root");
			// Root

			yield return new WaitForSeconds(3.0f);

			m_stateMachine.ProcessMessage(Message.PushState, "A");
			// Root
			//	|_A

			yield return new WaitForSeconds(3.0f);

			m_stateMachine.ProcessMessage(Message.PushState, "B");
			// Root
			//   |_A
			//     |_ B

			yield return new WaitForSeconds(3.0f);
			
			m_stateMachine.ProcessMessage(Message.ChangeState, "C");
			// Root
			//   |_A
			//     |_ C

			yield return new WaitForSeconds(3.0f);
			
			m_stateMachine.ProcessMessage(Message.PopState);
			// Root
			//   |_A

			yield return new WaitForSeconds(3.0f);
			
			m_stateMachine.ProcessMessage(Message.ChangeState, "D");
			// Root
			//   |_D

			yield return new WaitForSeconds(3.0f);
			
			m_stateMachine.ProcessMessage(Message.PopState);
			// Root

			yield return new WaitForSeconds(3.0f);
			
			m_stateMachine.ProcessMessage(Message.ChangeState, "Example has ended correctly");
			// Example has ended correctly
		}

		private void OnGUI()
		{
			string preffix = "";
			for(int index = 0; index < m_states.Count; ++index)
			{
				string label = preffix;
				if(index > 0)
				{
					label += "|_ ";
				}

				label += m_states[index];
				preffix += "  ";

				GUILayout.Label(label + " (" + (m_activeFlags[index]? "Active" : "Stacked") + ")");
			}
		}
#endregion

#region Private
	#region Attributes
		private HierarchicalStateMachine m_stateMachine;

		private List<string> m_states;
		private List<bool> m_activeFlags;
	#endregion
#endregion
	}

#region HSM States
	enum Message
	{
		PushState,
		ChangeState,
		PopState
	}


	class HsmExampleStateDefault : HsmState
	{
		protected override bool Init(params object[] a_userdata)
		{
			if(base.Init(a_userdata) == false)
			{
				return false;
			}

			m_name = a_userdata[0] as string;
			gameObject.GetComponent<HierarchicalStateMachineExample_Default>().OnInit(m_name);

			return true;
		}

		protected override void Release()
		{
			gameObject.GetComponent<HierarchicalStateMachineExample_Default>().OnRelease(m_name);

			base.Release();
		}

		protected override void Enter(HsmState a_stateFrom)
		{
			base.Enter(a_stateFrom);

			gameObject.GetComponent<HierarchicalStateMachineExample_Default>().OnEnter(m_name);
		}

		protected override void Exit(HsmState a_stateTo)
		{
			gameObject.GetComponent<HierarchicalStateMachineExample_Default>().OnExit(m_name);

			base.Exit(a_stateTo);
		}

		protected override bool OnMessage(params object[] a_userdata)
		{
			if(base.OnMessage(a_userdata))
			{
				return true;
			}

			if(a_userdata.Length >= 1  &&  a_userdata[0] is Message)
			{
				Message message = (Message)a_userdata[0];
				switch(message)
				{
					case Message.PushState:
					{
						if(a_userdata.Length == 2  &&  a_userdata[1] is string)
						{
							PushState<HsmExampleStateDefault>(a_userdata[1]);
							return true;
						}
					}
					break;
					case Message.ChangeState:
					{
						if(a_userdata.Length == 2  &&  a_userdata[1] is string)
						{
							ChangeState<HsmExampleStateDefault>(a_userdata[1]);
							return true;
						}
					}
					break;
					case Message.PopState:
					{
						PopState();
						return true;
					}
				}
			}
			return false;
		}

		private string m_name;
	}
#endregion
}
