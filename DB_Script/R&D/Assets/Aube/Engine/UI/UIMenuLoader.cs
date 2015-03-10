#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class UIMenuLoader
	//!
	//! @brief Component that loads a menu when enabled
	public class UIMenuLoader : MonoBehaviour
	{
		[SerializeField]
		public int m_menuIndex;

#region Private
		void OnEnable()
		{
            StartCoroutine(ShowMenu());
		}

		void OnDisable()
		{
            StopCoroutine(ShowMenu());
			UIManager.Instance.ShowMenu((uint)m_menuIndex, false);
		}

        private IEnumerator ShowMenu()
        {
            yield return StartCoroutine(UIManager.Instance.LoadMenu((uint)m_menuIndex));
            UIManager.Instance.ShowMenu((uint)m_menuIndex, true);
        }
#endregion
	}
}

#endif // !AUBE_NO_UI