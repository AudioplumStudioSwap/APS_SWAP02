using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aube
{
    namespace GameTransition
    {
        public class Condition
        {
        #region Attributes
        #region Private
            private GameEvent m_event = null;
            private System.Type m_gameMode = null;
            private System.Type m_gameState = null;
        #endregion
        #endregion

        #region Methods
        #region Public
            public static Condition Create(GameEvent gameEvent)
            {
                return new Condition(gameEvent, null, null);
            }

            public static Condition Create<GAME_MODE, GAME_STATE>(GameEvent gameEvent)
                where GAME_MODE : GameMode
                where GAME_STATE : GameState
            {
                return new Condition(gameEvent, typeof(GAME_MODE), typeof(GAME_STATE));
            }

            public System.Type GameMode { get { return m_gameMode; } }
            public System.Type GameState { get { return m_gameState; } }

            public Condition(GameEvent gameEvent, System.Type gameMode, System.Type gameState)
            {
                m_event = gameEvent;
                m_gameMode = gameMode;
                m_gameState = gameState;

                Assertion.Check(m_gameMode == null || typeof(Aube.GameMode).IsAssignableFrom(m_gameMode), "Invalid game mode");
                Assertion.Check(m_gameState == null || typeof(Aube.GameState).IsAssignableFrom(m_gameState), "Invalid game state");
            }

            public bool IsMatch(Condition condition)
            {
                return (m_gameMode == condition.m_gameMode  ||  (m_gameMode != null  &&  m_gameMode.IsAssignableFrom(condition.m_gameMode)))
				        && (m_gameState == condition.m_gameState  ||  (m_gameState != null  &&  m_gameState.IsAssignableFrom(condition.m_gameState)))
				    	&& condition.m_event.IsMatch(m_event);
            }

            public override string ToString()
            {
                string gameMode = (m_gameMode != null) ? m_gameMode.Name : "null";
                string gameState = (m_gameState != null) ? m_gameState.Name : "null";
                string gameEvent = (m_event != null) ? m_event.ToString() : "null";
                return "Condition(" + gameMode + ";" + gameState + ":" + gameEvent + ")";
            }            
        #endregion
        #endregion
        }
    }
}
