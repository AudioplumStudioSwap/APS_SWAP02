using UnityEngine;
using System.Collections;

namespace Aube
{
    namespace GameTransition
    {
        public class GameTargetReload : Aube.GameTransition.GameTarget
        {
        #region Attributes
        #region Private
            private bool m_reloaded = false;
        #endregion
        #endregion

        #region Methods
        #region Public
            public static GameTargetReload Create<GAME_STATE>() where GAME_STATE : Aube.GameState
            {
                return new GameTargetReload(typeof(GAME_STATE));
            }

            public override bool ApplyTransition(Aube.GameMode currentMode, string scene, string loadingScreen)
            {
                bool result = false;

                m_gameMode = currentMode.GetType();

                if (m_reloaded)
                {
                    result = SetGameState(currentMode);
                }
                else
                {
                    result = LoadGameMode(Application.loadedLevelName, loadingScreen);
                }

                if (result)
                {
                    m_reloaded = !m_reloaded;
                }
                return result;
            }
        #endregion
        #region Protected
            protected GameTargetReload(System.Type gameState) : base(typeof(Aube.GameMode), gameState)
            {
            }
        #endregion
        #endregion
        }
    }
}
