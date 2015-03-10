#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;

namespace Aube
{
	namespace UI
	{
		//! @class Button
		//!
		//! @brief Button of an user interface
		[AddComponentMenu("Scripts/Aube/UI/Menu/Button")]
		public class Button : Element
		{
			public void OpenPage(Page a_page)
			{
				a_page.Open();
			}

			public void ClosePage(Page a_page)
			{
				a_page.Close();
			}
		}
	}
}

#endif // !AUBE_NO_UI