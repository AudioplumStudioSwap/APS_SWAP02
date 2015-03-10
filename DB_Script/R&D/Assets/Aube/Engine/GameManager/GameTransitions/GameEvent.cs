using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Aube
{
    namespace GameTransition
    {
        public class GameEvent
        {
        #region Attributes
        #region Private
            private string m_value;
        #endregion
        #endregion

        #region Methods
        #region Public
            public GameEvent(string value)
            {
                m_value = value;
            }

            public bool IsMatch(GameEvent other)
            {
                return other.m_value == m_value;
            }

            public override string ToString()
            {
                return m_value;
            }
        #endregion
        #endregion
        }
    }
}
