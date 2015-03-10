#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;

namespace Aube
{
	namespace UI
	{
		//! @class Element
		//!
		//! @brief Element of an user interface
		public abstract class Element : MonoBehaviour
		{
			public Page ParentPage
			{
				get{ return m_parent; }
			}

			public static Page FindParentPage(Element a_element)
			{
				Page parent = null;
				
				Transform hierarchyIterator = a_element.transform.parent;
				while(hierarchyIterator != null  &&  parent == null)
				{
					parent = hierarchyIterator.GetComponent<Page>();
					hierarchyIterator = hierarchyIterator.parent;
				}

				return parent;
			}

#region Protected
			protected virtual void Awake()
			{
				m_parent = FindParentPage(this);
			}
#endregion

#region Private
			void UpdateParent()
			{
				m_parent = null;
				
				Transform hierarchyIterator = transform.parent;
				while(hierarchyIterator != null  &&  m_parent == null)
				{
					m_parent = hierarchyIterator.GetComponent<Page>();
					hierarchyIterator = hierarchyIterator.parent;
				}
			}

			//! parent page
			Page m_parent = null;
#endregion
		}
	}
}

#endif // !AUBE_NO_UI