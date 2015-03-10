using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
	[AddComponentMenu("Miscellaneous/State Machine")]

	//! @class StateMachineController
	//!
	//! @brief Component that runs a state machine
	public class StateMachineComponent : MonoBehaviour
	{
		[SerializeField]
		private StateMachine m_stateMachine;

		public void SetBool(string a_parameterName, bool a_value)
		{
			SetBool(StateMachine.StringToHash(a_parameterName), a_value);
		}

		public void SetBool(int a_parameterIdentifier, bool a_value)
		{
			if(m_booleanParameters.ContainsKey(a_parameterIdentifier))
			{
				m_booleanParameters[a_parameterIdentifier] = a_value;

				ChangeStateIfNeeded();
			}
			else if(m_stateMachine != null)
			{
				Debug.LogError("There is no boolean parameter named '" + a_parameterIdentifier + "' in the state machine '" + m_stateMachine.name + "'.");
			}
		}

		public void SetInt(string a_parameterName, int a_value)
		{
			SetInt(StateMachine.StringToHash(a_parameterName), a_value);
		}

		public void SetInt(int a_parameterIdentifier, int a_value)
		{
			if(m_integerParameters.ContainsKey(a_parameterIdentifier))
			{
				m_integerParameters[a_parameterIdentifier] = a_value;

				ChangeStateIfNeeded();
			}
			else if(m_stateMachine != null)
			{
				Debug.LogError("There is no integer parameter named '" + a_parameterIdentifier + "' in the state machine '" + m_stateMachine.name + "'.");
			}
		}

		public void SetFloat(string a_parameterName, float a_value)
		{
			SetFloat(StateMachine.StringToHash(a_parameterName), a_value);
		}

		public void SetFloat(int a_parameterIdentifier, float a_value)
		{
			if(m_floatParameters.ContainsKey(a_parameterIdentifier))
			{
				m_floatParameters[a_parameterIdentifier] = a_value;

				ChangeStateIfNeeded();
			}
			else if(m_stateMachine != null)
			{
				Debug.LogError("There is no float parameter named '" + a_parameterIdentifier + "' in the state machine '" + m_stateMachine.name + "'.");
			}
		}

		public void Trigger(string a_parameterName)
		{
			Trigger(StateMachine.StringToHash(a_parameterName));
		}

		public void Trigger(int a_parameterIdentifier)
		{
			if(m_triggerParameters.ContainsKey(a_parameterIdentifier))
			{
				m_triggerParameters[a_parameterIdentifier] = true;

				ChangeStateIfNeeded();
			}
			else if(m_stateMachine != null)
			{
				Debug.LogError("There is no trigger named '" + a_parameterIdentifier + "' in the state machine '" + m_stateMachine.name + "'.");
			}
		}

#region Unity Callbacks
		private void Awake()
		{
			if(m_stateMachine != null)
			{
				int[] booleanParameters = m_stateMachine.booleanParameters;
				bool[] booleanValues = m_stateMachine.booleanValues;
				m_booleanParameters = new Dictionary<int, bool>(booleanParameters.Length);
				for(int paramIndex = 0; paramIndex < booleanParameters.Length; ++paramIndex)
				{
					m_booleanParameters.Add(booleanParameters[paramIndex], booleanValues[paramIndex]);
				}

				int[] integerParameters = m_stateMachine.integerParameters;
				int[] integerValues = m_stateMachine.integerValues;
				m_integerParameters = new Dictionary<int, int>(integerParameters.Length);
				for(int paramIndex = 0; paramIndex < integerParameters.Length; ++paramIndex)
				{
					m_integerParameters.Add(integerParameters[paramIndex], integerValues[paramIndex]);
				}

				int[] floatParameters = m_stateMachine.floatParameters;
				float[] floatValues = m_stateMachine.floatValues;
				m_floatParameters = new Dictionary<int, float>(floatParameters.Length);
				for(int paramIndex = 0; paramIndex < floatParameters.Length; ++paramIndex)
				{
					m_floatParameters.Add(floatParameters[paramIndex], floatValues[paramIndex]);
				}

				int[] triggerParameters = m_stateMachine.triggerParameters;
				m_triggerParameters = new Dictionary<int, bool>(triggerParameters.Length);
				foreach(int parameterIdentifier in triggerParameters)
				{
					m_triggerParameters.Add(parameterIdentifier, false);
				}
			}

			m_activeStates = new Deque<ActiveState>();
		}

		private void OnDestroy()
		{
			while(m_activeStates.Count > 0)
			{
				DeactivateLastState();
			}
		}

		private void Start()
		{
			Aube.Assertion.Check(m_activeStates.Count == 0, "Error");
			if(m_stateMachine != null)
			{
				StateMachineState defaultState = m_stateMachine.defaultState;
				if(defaultState != null)
				{
					ActivateState(defaultState, gameObject);
				}
			}
		}

		private void OnEnable()
		{
			for(int activeStateIndex = 0; activeStateIndex < m_activeStates.Count - 1; ++activeStateIndex)
			{
				ActiveState activeState = m_activeStates[activeStateIndex];
				if(activeState.componentHolder != null)
				{
					activeState.componentHolder.SetActive(true);
				}
			}
		}

		private void OnDisable()
		{
			for(int activeStateIndex = m_activeStates.Count - 1; activeStateIndex >= 0; --activeStateIndex)
			{
				ActiveState activeState = m_activeStates[activeStateIndex];
				if(activeState.componentHolder != null)
				{
					activeState.componentHolder.SetActive(false);
				}
			}
		}

		private void Update()
		{
            ChangeStateIfNeeded();

            if(m_triggerParameters != null)
            {
                foreach(int key in m_triggerParameters.Keys)
			    {
			    	m_triggerParameters[key] = false;
                }
			}
		}
#endregion

#region Private
	#region Declarations
		private class ActiveState
		{
			public StateMachineState state;
			public GameObject componentHolder;
		}
	#endregion

	#region Methods
		private void ActivateState(StateMachineState a_state, GameObject a_parent)
		{
			Aube.Assertion.Check(a_state != null, "The state to activate is null.");

			ActiveState activeState = new ActiveState();
			activeState.state = a_state;
            // TEMP until the prefab is an automatic child of the state
            if(a_state.componentHolder == null)
            {
                activeState.componentHolder = new GameObject();
            }
            else
            {
                activeState.componentHolder = GameObject.Instantiate(a_state.componentHolder) as GameObject;
			}
            activeState.componentHolder.name = a_state.name;
            activeState.componentHolder.transform.parent = a_parent.transform;

			m_activeStates.AddToBack(activeState);

			// activate child default state
			if(a_state.defaultState != null)
			{
                ActivateState(a_state.defaultState, activeState.componentHolder);
			}
		}

		private void DeactivateState(StateMachineState a_state)
		{
			Aube.Assertion.Check(a_state != null, "The state to deactivate is null.");
			Aube.Assertion.Check(m_activeStates.Count > 0, "A state deactivation is requested but there is no active state.");

			while(m_activeStates[m_activeStates.Count - 1].state != a_state)
			{
				DeactivateLastState();
				Aube.Assertion.Check(m_activeStates.Count > 0, "The state '" + a_state.name + "' was not active but a deactivation request has been sent.");
			}

			DeactivateLastState();
		}

		private void DeactivateLastState()
		{
			Aube.Assertion.Check(m_activeStates.Count > 0, "A state deactivation is requested but there is no active state.");

			ActiveState activeState = m_activeStates[m_activeStates.Count - 1];
			if(activeState.componentHolder != null)
			{
				GameObject.Destroy(activeState.componentHolder);
				activeState.componentHolder = null;
			}
			
			m_activeStates.RemoveFromBack();
		}

		private void ChangeStateIfNeeded()
		{
			StateMachineTransition transition = null;

			int activeStateIndex = 0;
			while(activeStateIndex < m_activeStates.Count  &&  transition == null)
			{
				int transitionIndex = 0;
				while(transitionIndex < m_activeStates[activeStateIndex].state.nextTransitions.Length  &&  transition == null)
				{
					if(StateMachineTransition.Match(m_activeStates[activeStateIndex].state.nextTransitions[transitionIndex],
					                             m_booleanParameters, m_integerParameters, m_floatParameters, m_triggerParameters))
					{
						transition = m_activeStates[activeStateIndex].state.nextTransitions[transitionIndex];
					}
					++transitionIndex;
				}

				if(transition == null)
				{
					++activeStateIndex;
				}
			}

			if(transition != null)
			{
				while(activeStateIndex < m_activeStates.Count)
				{
					DeactivateLastState();
				}

                ActivateState(transition.stateTo, m_activeStates[m_activeStates.Count - 1].componentHolder);
			}
		}
	#endregion

	#region Attributes
		private Dictionary<int, bool> m_booleanParameters;
		private Dictionary<int, int> m_integerParameters;
		private Dictionary<int, float> m_floatParameters;
		private Dictionary<int, bool> m_triggerParameters;

		private Deque<ActiveState> m_activeStates;
	#endregion
#endregion
	}
}
