#if !AUBE_NO_UI

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Aube
{
	namespace UI
	{
		//! @class Page
		//!
		//! @brief Page of a menu interface
		[AddComponentMenu("Scripts/Aube/UI/Menu/Page")]
		public class Page : Element
        {
        #region Declarations
        #region Public
            [System.Flags]
            public enum Option
            {
                PAUSE = 0x01,
                MODAL = 0x02,

                //! Create a backup (when the page is activated) in order to return to this state
                NAVIGATION_STATE = 0x04,
                HIDE_OTHERS = 0x08,
            }
        #endregion
        #endregion

        #region Attributes
        #region Private
            [SerializeField][MaskFieldAttribute]
            private Option m_options = 0;

			[SerializeField]
			private PagePlaceholder[] m_prefabPages = null;

            //! page stamp
            private static long m_stamp = 0;
            private long m_displayStamp = InvalidDisplayStamp;

            //! children
            private List<Page> m_children = new List<Page>();

            //! input
            private List<object> m_inputLocks = new List<object>();
        #endregion
        #endregion

        #region Methods
        #region Public
            //! @brief A unique page value, incremented each time the page is activated
            public long DisplayStamp { get {return m_displayStamp; } }

            public static long InvalidDisplayStamp { get { return -1; } }
            
            public bool HasOption(Option option)
            {
                return (m_options & option) != 0;
            }

            //! @brief Enable or disable the inputs on the page 
            //! (the inputs are disabled while one lock exists)
            //!
            //! @param lockItem an identifier which set the state
            public void SetInputEnabled(bool enabled, object lockItem)
            {
                if (LockLayer < 0)
                {
                    return;
                }

                if (enabled)
                {
                    m_inputLocks.Remove(lockItem);
                }
                else
                {
                    if (!m_inputLocks.Contains(lockItem))
                    {
                        m_inputLocks.Add(lockItem);
                    }                    
                }
                
                Transform[] transforms = GetComponentsInChildren<Transform>(true);
                int layer = (m_inputLocks.Count == 0)? BaseLayer : LockLayer;

                for (int i = 0; i < transforms.Length; ++i)
                {
                    transforms[i].gameObject.layer = layer;
                }
            }

            //! @brief Return true if the inputs are enabled (no lock item)
            public bool IsInputEnabled()
            {
                return m_inputLocks.Count == 0;
            }

            //! @brief Set modal option
            public void SetModal(bool modal)
            {
                SetOption(Option.MODAL, modal);
                UIManager.UpdateModal();
            }

            //! @brief Open the page (activate the gameobject)
			public void Open()
			{
				gameObject.SetActive(true);
			}

            //! @brief Close the page
            //! if the page is a navigation state, then we will try to go back to the previous state
            //! else the page will just be disabled
            public void Close()
            {
               Close(true);
            }

            //! @brief Close the page
            //!
            //! @param navigationEnabled if true then we will use the navigation system, otherwise the page will just be disabled
			public void Close(bool navigationEnabled)
			{
				if(enabled)
				{
					if(ParentPage == null)
					{
                        UIManager.CloseMenu(this, navigationEnabled);
					}
					else
					{
						gameObject.SetActive(false);
					}                    
				}
			}

            //! @brief Load the page content
            //!
            //! @param coroutineSupport allow to start new coroutines
            public virtual IEnumerator Load(MonoBehaviour coroutineSupport)
            {
                yield return null;
            }

            //! @brief called when a page is opened or closed
            //!
            //! @param parent if null then the page is managed by the UIManager
            //! @param childIndex index relative to the parent (or UIManager is the parent is null)
            public virtual void OnPageActivation(Page parent, uint childIndex, bool activated)
            {
                for (int i = 0, count = m_children.Count; i < count; ++i)
                {
                    if (m_children[i].enabled && (parent != this || i != childIndex))
                    {
                        m_children[i].OnPageActivation(parent, childIndex, activated);
                    }
                }
            }     

            public Page GetModalPageInHierarchy()
            {
                UI.Page candidate = null;

                for (int i = -1, count = m_children.Count; i < count; ++i)
                {
                    UI.Page page = (i < 0)? this : m_children[i];

                    if (page != null && page.gameObject.activeSelf && page.HasOption(UI.Page.Option.MODAL))
                    {
                        UI.Page modal = (i < 0)? this : m_children[i].GetModalPageInHierarchy();

                        if (modal != null && (candidate == null || modal.IsInFrontOf(candidate)))
                        {
                            candidate = modal;
                        }
                    }
                }

                return candidate;
            }

            //! @brief A page is in front of an other if it was displayed after (Cf. DisplayStamp)
            public bool IsInFrontOf(Page page)
            {
                return DisplayStamp > page.DisplayStamp;
            }

            public uint FindChildIndex(UI.Page page)
            {
                for (int i = 0, count = m_children.Count; i < count; ++i)
                {
                    if (m_children[i] == page)
                    {
                        return (uint)i;
                    }
                }
                return (uint)m_children.Count;
            }
        #endregion
        #region Protected
            protected static int BaseLayer
            {
                get { return Aube.UIManager.Instance.BaseLayer; }
            }

            protected static int LockLayer
            {
                get { return Aube.UIManager.Instance.LockLayer; }
            }

            protected UIManager UIManager
            {
                get { return Aube.UIManager.Instance; }
            }

            protected override void Awake()
            {
                base.Awake();

                foreach (PagePlaceholder prefab in m_prefabPages)
                {
                    if (prefab.Page != null && prefab.Placeholder != null)
                    {
                        Page page = GameObject.Instantiate(prefab.Page) as Page;
                        page.transform.parent = prefab.Placeholder.transform;
                        page.transform.localPosition = Vector3.zero;
                        page.transform.localRotation = Quaternion.identity;
                        page.transform.localScale = Vector3.one;

                        switch (prefab.StartPolicy)
                        {
                            case PagePlaceholder.EStartPolicy.StartOpened: { page.gameObject.SetActive(true); } break;
                            case PagePlaceholder.EStartPolicy.StartClosed: { page.gameObject.SetActive(false); } break;
                        }
                    }
                    else
                    {
                        Log.Warning("Wrong placeholder in menu page " + name + ".");
                    }
                }

                UpdateChildren();
            }

            protected virtual void OnEnable()
			{
                m_displayStamp = m_stamp++;
                
                if (HasOption(Option.PAUSE))
				{
                    UIManager.RequestPause(true);
				}

                if (HasOption(Option.HIDE_OTHERS))
				{
                    HideOthers();
				}
                
                UIManager.OnPageActivation(this, true);   
			}
			
			protected virtual void OnDisable()
			{
                if (HasOption(Option.PAUSE))
				{
                    UIManager.RequestPause(false);
				}

                UIManager.OnPageActivation(this, false);
			}

            //! @brief Hide the pages which don't contain this page in their hierarchy
            protected void HideOthers()
            {
                UI.Page root = this;

                while (root.ParentPage != null)
                {
                    root = root.ParentPage;
                }

                for (uint i = 0; i < UIManager.MenusCount; ++i)
                {
                    if (root != UIManager.GetMenu<Page>(i))
                    {
                        UIManager.ShowMenu(i, false);
                    }
                }
            }
        #endregion
        #region Private
            private void SetOption(Option option, bool set)
            {
                if (set)
                {
                    m_options = m_options | option;
                }
                else
                {
                    m_options = m_options & ~option;
                }
            }

			private void UpdateChildren()
			{
				m_children.Clear();

				Queue<Transform> hierarchyIterators = new Queue<Transform>();
				foreach(Transform child in transform)
				{
					hierarchyIterators.Enqueue(child);
				}

				while(hierarchyIterators.Count > 0)
				{
					Transform iterator = hierarchyIterators.Dequeue();

					Page menuPage = iterator.GetComponent<Page>();
					if(menuPage == null)
					{
						foreach(Transform child in iterator)
						{
							hierarchyIterators.Enqueue(child);
						}
					}
					else
					{
						m_children.Add(menuPage);
					}
				}
			}
        #endregion
        #endregion
		}
	}
}

#endif // !!AUBE_NO_UI