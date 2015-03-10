using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace Aube
{
	//! @class GameState
	//!
	//! @brief State of the game
	public abstract class GameState : Aube.FSMState
	{
        public bool Loaded
        {
            get { return m_loaded; }
        }

#region Protected
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected virtual IEnumerator Load()
        {
            yield return null;
        }

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(Create());
        }

        protected override bool OnMessage(params object[] a_userdata)
        {
            if (base.OnMessage(a_userdata))
            {
                return true;
            }

            if (a_userdata.Length > 0 && a_userdata[0] is GameMode.StateAction)
            {
                if ((GameMode.StateAction)a_userdata[0] == GameMode.StateAction.Push && a_userdata.Length > 1  && typeof(GameState).IsAssignableFrom(a_userdata[1] as System.Type))
                {
                    System.Type gameStateType = a_userdata[1] as System.Type;

                    MethodInfo method = typeof(Aube.FSMState).GetMethod("PushState", BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo genericMethod = (method != null) ? method.MakeGenericMethod(gameStateType) : null;
                    Aube.Assertion.Check(genericMethod != null, "method not found");

                    List<object> parametersList = new List<object>(a_userdata);
                    parametersList.RemoveAt(0);
                    parametersList.RemoveAt(0);

                    genericMethod.Invoke(this, new object[] { parametersList.ToArray() });
                }
                else if ((GameMode.StateAction)a_userdata[0] == GameMode.StateAction.Pop)
                {
                    PopState();
                }
                
                return true;
            }
            else if (a_userdata.Length > 0 && a_userdata[0] is System.Type && typeof(GameState).IsAssignableFrom(a_userdata[0] as System.Type))
            {
                System.Type gameStateType = a_userdata[0] as System.Type;

                MethodInfo method = typeof(Aube.FSMState).GetMethod("ChangeState", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo genericMethod = (method != null) ? method.MakeGenericMethod(gameStateType) : null;
                Aube.Assertion.Check(genericMethod != null, "method not found");

                List<object> parametersList = new List<object>(a_userdata);
                parametersList.RemoveAt(0);

                genericMethod.Invoke(this, new object[] { parametersList.ToArray() });

                return true;
            }

            return false;
        }
#endregion
#region Private
        private IEnumerator Create()
        {
            yield return StartCoroutine(Load());
            m_loaded = true;
        }

		private bool m_loaded = false;
#endregion
	}
} // namespace Aube