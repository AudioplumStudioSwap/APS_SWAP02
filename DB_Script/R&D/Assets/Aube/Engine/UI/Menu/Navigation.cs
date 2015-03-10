#if !AUBE_NO_UI

using UnityEngine;
using System.Collections.Generic;

namespace Aube
{
    namespace UI
    {
        //! @class Backup
        //!
        //! @brief Backup of the menus
        public class Navigation
        {
        #region Declarations
        #region Private
            private class PageState
            {
                private int m_index = 0;
                private bool m_active = false;
                private long m_order = 0;

                public int Index { get { return m_index; } }

                public bool Active { set { m_active = value; } }

                public PageState(int index, Page page)
                {
                    m_index = index;
                    m_active = page != null && page.gameObject.activeSelf;
                    m_order = (m_active)? page.DisplayStamp : UI.Page.InvalidDisplayStamp;
                }

                public void Restore()
                {
                    UIManager.Instance.ShowMenu((uint)m_index, m_active);
                }

                public static int Compare(PageState item1, PageState item2)
                {
                    return (int)(item1.m_order - item2.m_order);
                }
            }

            private class GameState
            {
                private List<PageState> m_pages = null;

                public uint Ident { get; private set; }

                public GameState(uint ident, Page[] pages)
                {
                    Ident = ident;
                    Setup(pages);
                }

                public void Setup(Page[] pages)
                {
                    m_pages = new List<PageState>();

                    long stamp = pages[Ident].DisplayStamp;

                    for (int i = 0; i < pages.Length; ++i)
                    {
                        Page page = (pages[i] != null && stamp >= pages[i].DisplayStamp) ? pages[i] : null;
                        m_pages.Add(new PageState(i, page));
                    }

                    // Sort by apparition time in order to reactivate the page in the same order
                    m_pages.Sort(PageState.Compare);
                }

                public void RemovePage(uint index)
                {
                    for (int i = 0, count = m_pages.Count; i < count; ++i)
                    {
                        if (m_pages[i].Index == index)
                        {
                            m_pages[i].Active = false;
                            return;
                        }
                    }                 
                }

                public void Restore()
                {
                    for (int i = 0, count = m_pages.Count; i < count; ++i)
                    {
                        m_pages[i].Restore();
                    }
                }
            }
        #endregion
        #endregion

        #region Attributes
        #region Private
            private List<GameState> m_states = new List<GameState>();
            private int m_locked = 0;
        #endregion
        #endregion

        #region Methods
        #region Public
            //! @brief lock the modification of the navigation (used when a state is restored)
            public bool Locked
            {
                get { return m_locked > 0; }
                private set 
                {
                    m_locked = Mathf.Max((value)? m_locked + 1 : m_locked - 1, 0);
                }
            }

            //! @brief save a menu state (the visibility of the pages)
            //!
            //! @param ident a state identification (used to pop the state)
            //! @param pages pages whose the state must be saved
            public void PushState(uint ident, Page[] pages)
            {
                if (pages == null || ident >= pages.Length)
                {
                    Aube.Log.Warning(typeof(Navigation).Name + ": can not push state (" + ident.ToString() + ")");
                    return;
                }

                if (!Locked)
                {
                    // Keep only one backup for each identifier
                    for (int i = 0, count = m_states.Count; i < count; ++i)
                    {
                        if (m_states[i].Ident == ident)
                        {
                            m_states.RemoveAt(i);
                            break;
                        }
                    }

                    m_states.Add(new GameState(ident, pages));
                }
            }

            //! @brief pop the given state and restore the previous state
            //! @param ident state to pop
            public bool PopState(uint ident)
            {
                for (int i = m_states.Count - 1; i >= 0; --i)
                {
                    if (m_states[i].Ident == ident)
                    {
                        return Restore(i - 1);
                    }
                }
                return false;
            }
            

            //! @brief remove a menu
            //!
            //! @param ident a state identification
            public void RemoveMenu(uint ident)
            {
                if (!Locked)
                {
                    for (int i = 0; i < m_states.Count; ++i)
                    {
                        if (m_states[i].Ident == ident)
                        {
                            m_states.RemoveAt(i--);
                        }
                        else
                        {
                            m_states[i].RemovePage(ident);
                        }
                    }
                }
            }

            //! @brief restore the given state
            //! @param ident state to restore
            public bool RestoreState(uint ident)
            {
                for (int i = m_states.Count - 1; i >= 0; --i)
                {
                    if (m_states[i].Ident == ident)
                    {
                        return Restore(i);
                    }
                }
                return false;
            }

            public void Clear()
            {
                if (!Locked)
                {
                    m_states.Clear();
                }
            }
        #endregion
        #region Private
            private bool Restore(int index)
            {
                if (index < 0)
                {
                    UIManager.Instance.CloseMenus();
                }
                else
                {
                    int nextIdx = index + 1;
                    if (nextIdx < m_states.Count)
                    {                        
                        m_states.RemoveRange(nextIdx, m_states.Count - nextIdx);
                    }
                    Locked = true;
                    m_states[index].Restore();
                    Locked = false;
                }
                return true;
            }
        #endregion
        #endregion
        }
    }
}
#endif // !!AUBE_NO_UI
