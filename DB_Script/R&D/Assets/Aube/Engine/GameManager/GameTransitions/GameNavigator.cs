using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aube
{
    namespace GameTransition
    {
        public class GameNavigator
        {
        #region Declarations
        #region Private
            private class Transition
            {
                public GameTarget m_target = null;
                public Condition m_condition = null;
                public string m_scene = null;
                public string m_loading = null;

                public Transition(Condition condition, GameTarget target, string scene, string loading)
                {
                    m_target = target;
                    m_condition = condition;
                    m_scene = scene;
                    m_loading = loading;

                    if (condition.GameMode != target.GameMode)
                    {
                        Assertion.Check(scene != null, ToString() + ": no scene");
                        Assertion.Check(loading != null, ToString() + ": no loading screen");
                    }
                }

                public bool IsEqual(Transition other)
                {
                    return m_target.IsEqual(other.m_target)
                        && m_condition.IsMatch(other.m_condition)
                        && m_scene == other.m_scene
                        && m_loading == other.m_loading;
                }

                public bool ApplyTransition(GameMode currentMode)
                {
                    return m_target.ApplyTransition(currentMode, m_scene, m_loading);
                }

                public override string ToString()
                {
                    return "Transition(" + m_condition.ToString() + "->" + m_target.ToString() + ")";
                }
            }

            private class Loading
            {
                public System.Type m_source = null;
                public System.Type m_target = null;
                public string m_loadingScreen = null;

                public Loading(System.Type source, System.Type target, string loadingScreen)
                {
                    m_source = source;
                    m_target = target;
                    m_loadingScreen = loadingScreen;
                }
            }
        #endregion
        #endregion

        #region Attributes
        #region Private
            private List<Transition> m_transitions = new List<Transition>();
            private Dictionary<System.Type, string> m_defaultGameModeScenes = new Dictionary<System.Type, string>();
            private string m_defaultLoadingScreen = null;
            private Transition m_currentTransition = null;            
        #endregion
        #endregion

        #region Methods
        #region Public
            public string DefaultLoadingScreen
            {
                get { return m_defaultLoadingScreen; }
                set { m_defaultLoadingScreen = value; }
            }

            public bool AddTransition(Condition condition, GameTarget target, string scene = null, string loading = null)
            {
                if (scene == null)
                {
                    scene = GetDefaultGameModeScene(target.GameMode);
                }

                if (loading == null)
                {
                    loading = m_defaultLoadingScreen;
                }

                Transition transition = new Transition(condition, target, scene, loading);

                for (int i = 0, count = m_transitions.Count; i < count; ++i)
                {
                    if (m_transitions[i].IsEqual(transition))
                    {
                        Log.Warning("Transition (" + transition.ToString() + ") already added");
                        return false;
                    }
                }
                
                m_transitions.Add(transition);
                return true;
            }

            public bool ApplyTransition(GameEvent gameEvent)
            {
                GameMode gameMode = GameManager.Instance.GameMode;
                System.Type modeType = (gameMode) ? GameManager.Instance.GameMode.GetType() : null;
                System.Type stateType = (gameMode != null && gameMode.CurrentState != null) ? gameMode.CurrentState.GetType() : null;
                if (ApplyTransition(new Condition(gameEvent, modeType, stateType)))
                {
                    return true;
                }
                Log.Error("No transition available for the game event " + gameEvent.ToString());
                return false;
            }

            public void SetDefaultGameModeScene<GAME_MODE>(string scene)
            {
                m_defaultGameModeScenes.Add(typeof(GAME_MODE), scene);
            }
            
            public System.Type[] GetGameStates(Aube.GameMode gameMode)
            {
                System.Type modeType = gameMode.GetType();
                List<System.Type> states = new List<System.Type>();

                for (int i = 0, count = m_transitions.Count; i < count; ++i)
                {
                    if (modeType == m_transitions[i].m_target.GameMode)
                    {
                        if (!states.Contains(m_transitions[i].m_target.GameState))
                        {
                            states.Add(m_transitions[i].m_target.GameState);
                        }
                    }                        
                }

                return states.ToArray();
            }

            public bool StartInitialState(Aube.GameMode gameMode)
            {
                System.Type modeType = gameMode.GetType();

                if (m_currentTransition != null && modeType == m_currentTransition.m_target.GameMode)
                {
                    m_currentTransition.ApplyTransition(gameMode);
                    return true;
                }
                return false;
            }
        #endregion
        #region Private
            private bool ApplyTransition(Condition condition)
            {
                for (int i = 0, count = m_transitions.Count; i < count; ++i)
                {
                    if (m_transitions[i].m_condition.IsMatch(condition))
                    {
                        m_currentTransition = m_transitions[i];
                        return m_currentTransition.ApplyTransition(GameManager.Instance.GameMode);
                    }
                }
                return false;
            }
            
            private string GetDefaultGameModeScene(System.Type gameMode)
            {
                string scene;
                if (m_defaultGameModeScenes.TryGetValue(gameMode, out scene))
                {
                    return scene;
                }
                return null;
            }
        #endregion
        #endregion
        }
    }
}
