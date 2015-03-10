using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Aube
{
	public abstract class GameMode : MonoBehaviour
	{
        public enum StateAction
        {
            Change,
            Push,
            Pop
        };

    #region Attributes
    #region Protected
        protected Aube.FSM m_stateMachine;
    #endregion
    #region Private
        private bool m_loaded = false;
    #endregion
    #endregion

    #region Methods
    #region Public
        public bool Loaded
        {
            get { return m_loaded; }
        }

        public void ChangeState<T>(params object[] a_userdata) where T : GameState
        {
            if (m_stateMachine.Current == null)
            {
                m_stateMachine.SwitchRootState<T>(a_userdata);
            }
            else
            {
                object[] parameters = new object[a_userdata.Length + 1];
                parameters[0] = typeof(T);
                for (int paramIndex = 0; paramIndex < a_userdata.Length; ++paramIndex)
                {
                    parameters[1 + paramIndex] = a_userdata[paramIndex];
                }
                m_stateMachine.ProcessMessage(parameters);
            }
        }

        public void PushState<T>(params object[] a_userdata) where T : GameState
        {
            if (m_stateMachine.Current == null)
            {
                m_stateMachine.SwitchRootState<T>(a_userdata);
            }
            else
            {
                object[] parameters = new object[a_userdata.Length + 2];
                parameters[0] = StateAction.Push;
                parameters[1] = typeof(T);
                for (int paramIndex = 0; paramIndex < a_userdata.Length; ++paramIndex)
                {
                    parameters[2 + paramIndex] = a_userdata[paramIndex];
                }
                m_stateMachine.ProcessMessage(parameters);
            }
        }

        public void PopState()
        {
            if (m_stateMachine.Current == null)
            {
                return;
            }
            else
            {
                object[] parameters = new object[1];
                parameters[0] = StateAction.Pop;
                m_stateMachine.ProcessMessage(parameters);
            }
        }

        public virtual void StartScene()
        {
        }

        public virtual void Dispose()
        {
            m_stateMachine.Clear();
        }

        public T GetGameState<T>() where T : GameState
        {
            return CurrentState as T;
        }

        public GameState CurrentState
        {
            get { return m_stateMachine.Current as GameState; }
        }
    #endregion
    #region Protected
        protected virtual IEnumerator Load()
        {
            yield return null;
        }

        protected virtual void Awake()
        {
            // Will be activated by the loading manager
            enabled = false;

            m_stateMachine = new Aube.FSM(gameObject);
            StartCoroutine(Create());
        }

        protected virtual IEnumerator CreateStates(System.Type[] states)
        {
            for (int i = 0; i < states.Length; ++i)
            {
                if (!typeof(Aube.GameState).IsAssignableFrom(states[i]))
                {
                    Assertion.Check(false, "invalid game state");
                    states[i] = null;
                }
            }

            List<FSMState> gameStates = m_stateMachine.CreateStates(states);

            for (int i = 0, count = gameStates.Count; i < count; ++i)
            {
                GameState gameState = gameStates[i] as GameState;
                while (gameState != null && !gameState.Loaded)
                {
                    yield return null;
                }
            }
        }
    #endregion
    #region Private
        private IEnumerator Create()
        {
            yield return StartCoroutine(Load());
            m_loaded = true;
        }
    #endregion
    #endregion
	}
} // namespace Aube