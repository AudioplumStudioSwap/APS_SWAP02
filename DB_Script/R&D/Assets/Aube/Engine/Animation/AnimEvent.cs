using System.Collections.Generic;
using UnityEngine;
using System;

namespace Aube
{
    //! @class AnimEvent
    //!
    //! @brief  Intermediary between an animator and the game.
    //!         Must be placed near an animator to catch the events.
    public class AnimEvent : MonoBehaviour
    {
    #region Attributes
    #region Public
        [SerializeField]
        private FxResources m_resources = null;
    #endregion
    #endregion

    #region Methods
    #region Public
        public FxResources FxResources
        {
            get { return m_resources; }
            set { m_resources = value; }
        }

        //! @brief Command to activate an fx (listed in the FxResources).
        public void StartFx(AnimationEvent animEvent)
        {
            ActivateFx(animEvent, true);
        }

        //! @brief Command to deactivate an fx (listed in the FxResources).
        public void StopFx(AnimationEvent animEvent)
        {
            ActivateFx(animEvent, false);
        }
    #endregion
    #region Private
        private void ActivateFx(AnimationEvent animEvent, bool activate)
        {
            FxCommand command = new FxCommand(animEvent.stringParameter);
            FxCommand.Param name = command.GetParam(FxCommand.MAIN_PARAM);

            if (name == null)
            {
                LogError(animEvent, "invalid command");
            }
            else if (m_resources == null || !m_resources.Activate(name.Value, activate, command))
            {
                LogError(animEvent, "no resource");
            }
        }

        private void LogInfo(AnimationEvent animEvent, string suffix)
        {
            LogEvent(animEvent, suffix, Aube.Log.Verbosity.Info);
        }

        private void LogWarning(AnimationEvent animEvent, string suffix)
        {
            LogEvent(animEvent, suffix, Aube.Log.Verbosity.Warning);
        }

        private void LogError(AnimationEvent animEvent, string suffix)
        {
            LogEvent(animEvent, suffix, Aube.Log.Verbosity.Error);
        }

        private void LogEvent(AnimationEvent animEvent, string suffix, Aube.Log.Verbosity verbosity)
        {
            string msg = typeof(AnimEvent).ToString() + " (" + gameObject.name + "): " + animEvent.functionName + "(";
            List<string> paramList = new List<string>();

            if (animEvent.objectReferenceParameter != null)
            {
                paramList.Add(animEvent.objectReferenceParameter.name);
            }
            if (!string.IsNullOrEmpty(animEvent.stringParameter))
            {
                paramList.Add(animEvent.stringParameter);
            }
            if (animEvent.floatParameter != 0)
            {
                paramList.Add(animEvent.floatParameter.ToString());
            }
            if (animEvent.intParameter != 0)
            {
                paramList.Add(animEvent.intParameter.ToString());
            }

            for (int i = 0, count = paramList.Count; i < count; ++i)
            {
                msg += (i > 0) ? "; " : "";
                msg += paramList[i];
            }

            msg += ")" + (string.IsNullOrEmpty(suffix) ? "" : " - " + suffix);

            Aube.Log.Message(verbosity, msg);
        }
    #endregion
    #endregion
    }
}