#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;

namespace Aube
{
	namespace UI
	{
		//! @class PagePlaceholder
		//!
		//! @brief Placeholder reference for a menu page
		[System.Serializable]
		internal class PagePlaceholder
		{
			internal enum EStartPolicy
			{
				None,
				StartOpened,
				StartClosed,
			}

			[SerializeField]
			private Page m_page;
			[SerializeField]
			private GameObject m_placeholder;
			[SerializeField]
			private EStartPolicy m_startPolicy;

			internal Page Page
			{
				get{ return m_page; }
			}

			internal GameObject Placeholder
			{
				get{ return m_placeholder; }
			}

			internal EStartPolicy StartPolicy
			{
				get{ return m_startPolicy; }
			}
		}
	}
}

#endif // !!AUBE_NO_UI