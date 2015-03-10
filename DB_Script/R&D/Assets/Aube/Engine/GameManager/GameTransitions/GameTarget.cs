using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aube
{
    namespace GameTransition
    {
        public class GameTarget
        {                    
        #region Attributes
        #region Private
            protected System.Type m_gameMode = null;
            protected System.Type m_gameState = null;
        #endregion
        #endregion

        #region Mehods
        #region Public
            public static GameTarget Create<GAME_MODE, GAME_STATE>() 
                where GAME_MODE : GameMode 
                where GAME_STATE : GameState
            {
                return new GameTarget(typeof(GAME_MODE), typeof(GAME_STATE));
            }

            public GameTarget(System.Type gameMode, System.Type gameState)
            {
                m_gameMode = gameMode;
                m_gameState = gameState;

                Assertion.Check(m_gameMode != null && typeof(Aube.GameMode).IsAssignableFrom(m_gameMode), ToString() + ": invalid game mode");
                Assertion.Check(m_gameState != null && typeof(Aube.GameState).IsAssignableFrom(m_gameState), ToString() + ": invalid game state");
            }

            public System.Type GameMode {get {return m_gameMode;}}

            public System.Type GameState {get {return m_gameState;}}

            public virtual bool ApplyTransition(GameMode currentMode, string scene, string loadingScreen)
            {
                System.Type modeType = (currentMode) ? currentMode.GetType() : null;
                System.Type stateType = (currentMode != null && currentMode.CurrentState != null) ? currentMode.CurrentState.GetType() : null;

                if (modeType != m_gameMode)
                {
                    return LoadGameMode(scene, loadingScreen);
                }
                else if (currentMode != null && stateType != m_gameState)
                {
                    return SetGameState(currentMode);
                }
                return false;
            }

            public virtual bool IsEqual(GameTarget other)
            {
                return m_gameMode == other.m_gameMode && m_gameState == other.m_gameState;
            }

            public override string ToString()
            {
                string mode = (m_gameMode != null)? m_gameMode.Name : "";
                string state = (m_gameState != null)? m_gameState.Name : "";
                return "Target(" + mode + ";" + state + ")";
            }
        #endregion
        #region Protected
            public virtual bool LoadGameMode(string scene, string loadingScreen)
            {
                if (scene != null)
                {
                    Aube.LoadingManager.LoadLevel(scene, m_gameMode, loadingScreen);
                    return true;
                }
                Assertion.Check(false, ToString() + ": no scene");
                return false;
            }

            public virtual bool SetGameState(GameMode currentMode)
            {
                MethodInfo method = typeof(Aube.GameMode).GetMethod("ChangeState", BindingFlags.Instance | BindingFlags.Public);
                MethodInfo genericMethod = (method != null) ? method.MakeGenericMethod(m_gameState) : null;
                Aube.Assertion.Check(genericMethod != null, "method not found");
                genericMethod.Invoke(currentMode, new object[] { new object[] { } });
                return true;
            }
        #endregion
        #endregion
        }
    }
}
