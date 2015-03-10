#if !AUBE_NO_UI

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Aube
{
	//! @class UIManager
	//!
	//! @brief Manager that treats UI requests
	//!	This class is using NGUI as UI plugin.
	public class UIManager : Singleton<UIManager>
	{
    //*************************************************************************
    // Declarations
    //*************************************************************************
        //! events
        public delegate bool PauseDelegate(bool paused);

	//*************************************************************************
	// Attributes
	//*************************************************************************
		//! hierarchy
        private NGUIAsset m_asset;
        private GameObject m_root;
        private Transform m_subHierarchyRoot;
        
		//! ui assets
		private NGUIMenuDesc m_menuAsset;
		private NGUIHudDesc m_hudAsset;
        private MonoBehaviour m_coroutineSupport;

		//! ui elements
        private UI.Page[] m_menus;
		private GameObject[] m_huds;

        //! ui navigation
        private UI.Navigation m_navigation = new UI.Navigation();

        //! pause
        private PauseDelegate m_pauseDelegate = null;
        private uint m_pauseRequests;

	//*************************************************************************
	// Public Initialization Methods
	//*************************************************************************
		//! @brief Initializes the instance
		public UIManager()
		{
			m_asset = LoadResource<NGUIAsset>();
            if (m_asset.RootPrefab == null)
			{
				Log.Error("NGUI Asset does not have a root UI prefab set.");
	#if UNITY_EDITOR
                UnityEditor.Selection.activeObject = m_asset;
	#endif // UNITY_EDITOR
				return;
			}

            if (BaseLayer < 0 || LockLayer < 0)
            {
                Log.Error("Invalid layers in NGUI Asset.");
            }

            m_root = GameObject.Instantiate(m_asset.RootPrefab) as GameObject;
			UnityEngine.Object.DontDestroyOnLoad(m_root);
			m_subHierarchyRoot = FindSubHierarchyRoot();
            m_coroutineSupport = m_root.AddComponent<MonoBehaviour>();

			m_pauseRequests = 0;
		}

        //! @brief Clean the instance
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_root != null)
            {
                GameObject.Destroy(m_root);
            }
        }

		//! @brief Registers Menus and HUDs
		public void RegisterUI(NGUIMenuDesc menuDesc, NGUIHudDesc hudDesc)
		{
            CloseAll();
            UnloadMenus();

            m_navigation.Clear();

			m_menuAsset = menuDesc;
			m_hudAsset = hudDesc;

			m_menus = new UI.Page[(m_menuAsset != null)? m_menuAsset.Count : 0];
			for(uint menuIndex = 0; menuIndex < m_menus.Length; ++menuIndex)
			{
				m_menus[menuIndex] = null;
			}
			m_huds = new GameObject[(m_hudAsset != null)? m_hudAsset.Count : 0];
			for(uint hudIndex = 0; hudIndex < m_huds.Length; ++hudIndex)
			{
				m_huds[hudIndex] = null;
			}
		}

		public void CloseAll()
		{
            CloseMenus();
            CloseHuds();
		}

        public void CloseMenus()
        {
            if (m_menus != null)
            {
                for (uint menuIndex = 0; menuIndex < m_menus.Length; ++menuIndex)
                {
                    if (IsMenuShown(menuIndex))
                    {
                        ShowMenu(menuIndex, false);
                    }
                    if (IsMenuShown(menuIndex))
                    {
                        Aube.Assertion.Check(false, "The menu " + m_menus[menuIndex].name + " (" + menuIndex + ") failed to hide.");
                    }
                }
            }
        }

        public void CloseHuds()
        {
            if (m_huds != null)
            {
                for (uint hudIndex = 0; hudIndex < m_huds.Length; ++hudIndex)
                {
                    if (IsHUDShown(hudIndex))
                    {
                        ShowHUD(hudIndex, false);
                    }
                    if (IsHUDShown(hudIndex))
                    {
                        Aube.Assertion.Check(false, "The hud " + m_huds[hudIndex].name + " (" + hudIndex + ") failed to hide.");
                    }
                }
            }
        }

        public void ClearPauseListeners()
        {
            m_pauseDelegate = null;
        }

        //! @brief Register a delegate to manage the pause
        //! the delegate must return true to entirely manage the pause
        public void RegisterPauseListener(PauseDelegate function)
        {
            m_pauseDelegate = function;
        }
        
        public int BaseLayer
        {
            get { return (m_asset != null)? m_asset.BaseLayer : -1; }
        }

        public int LockLayer
        {
            get { return (m_asset != null)? m_asset.LockLayer : -1; }
        }
                
		protected override void ProcessMessage(int a_messageId)
		{
			if(a_messageId == "OnLoadingScene".GetHashCode())
			{
				CloseAll();                
                UnloadMenus();
                
                if (m_hudAsset != null)
                {
                    m_hudAsset.OnLoadingScene();
                }               

                Aube.UIHelper.UpdateDependencies(Aube.UIHelper.UPDATE_OPT.RESET, null);
                
				// Clean the remainnig objects
				if (Aube.UIHelper.CamObj != null)
				{
					foreach (Transform child in Aube.UIHelper.CamObj.transform)
					{
						GameObject.Destroy(child.gameObject);
					}
				}
			}
		}

	//*************************************************************************
	// Public API Methods : HUD
	//*************************************************************************
		//! @brief Show/Hide HUD
		//!
		//! @param hudIndex		index of the hud to show/hide
		//! @param show			true to show hud, false to hide
        public void ShowHUD(uint hudIndex, bool show)
        {
			if(IsValid == false)
			{
				return;
			}

			Assertion.Check(hudIndex < m_hudAsset.Count, "Invalid HUD index");

			if(show  &&  IsHUDShown(hudIndex) == false)
			{
				GameObject hud = m_hudAsset.GetHUD(hudIndex);
				if(hud == null)
				{
					Log.Warning("HUD " + (hudIndex + 1) + " is not set.");
	#if UNITY_EDITOR
					UnityEditor.Selection.activeObject = m_hudAsset as ScriptableObject;
	#endif // UNITY_EDITOR
				}
				else
				{
					m_huds[hudIndex] = hud;
					m_huds[hudIndex].transform.parent = m_subHierarchyRoot;
					m_huds[hudIndex].transform.localPosition = Vector3.zero;
					m_huds[hudIndex].transform.localRotation = Quaternion.identity;
					m_huds[hudIndex].transform.localScale = Vector3.one;
                    m_huds[hudIndex].SetActive(true);
				}
			}
			else if(show == false  &&  IsHUDShown(hudIndex))
			{
                m_huds[hudIndex].SetActive(false);
                m_hudAsset.OnHudHidden(hudIndex);              
				m_huds[hudIndex] = null;
			}
		}


		//! @brief Get the given HUD
		//!
		//! @param hudIndex	index of the HUD to check
		public T GetHUD<T>(uint hudIndex) where T : Component
		{
			Assertion.Check(IsValid, "UI Manager not valid");
			return (m_huds[hudIndex] != null)? m_huds[hudIndex].GetComponentInChildren<T>() : null;
		}

		//! @brief Checks if the given HUD is shown
		//!
		//! @param hudIndex	index of the HUD to check
		public bool IsHUDShown(uint hudIndex)
		{
			Assertion.Check(IsValid, "UI Manager not valid");

			return m_huds[hudIndex] != null;
		}

	//*************************************************************************
	// Public API Methods : Menu
	//*************************************************************************
        //! @brief number of menus which are registered
        public int MenusCount
        {
            get { return (m_menus != null)? m_menus.Length : 0; }
        }

		//! @brief Show/Hide Menu
		//!
		//! @param a_menuIndex	index of the menu to show/hide
		//! @param show			true to show menu, false to hide
		public void ShowMenu(uint a_menuIndex, bool show)
		{
			if(IsValid == false)
			{
				return;
			}
			
			Assertion.Check(a_menuIndex < m_menuAsset.Count, "Invalid Menu index");
			
			if(show  &&  IsMenuShown(a_menuIndex) == false)
			{
				GameObject menu = m_menuAsset.GetMenu(a_menuIndex);
				if(menu == null)
				{
					Log.Warning("MENU " + (a_menuIndex + 1) + " is not set.");
	#if UNITY_EDITOR
					UnityEditor.Selection.activeObject = m_menuAsset as ScriptableObject;
	#endif // UNITY_EDITOR
				}
				else
				{
                    m_menus[a_menuIndex] = menu.GetComponent<UI.Page>();

					if(m_menus[a_menuIndex] == null)
					{
						Log.Error("The object " + menu.name + " is set as a menu but does not have the component Page.");
					}
					else
					{
                        m_menus[a_menuIndex].transform.parent = m_subHierarchyRoot;
                        m_menus[a_menuIndex].transform.localPosition = Vector3.zero;
                        m_menus[a_menuIndex].transform.localRotation = Quaternion.identity;
                        m_menus[a_menuIndex].transform.localScale = Vector3.one;
                        m_menus[a_menuIndex].gameObject.SetActive(true);                     
					}
				}
			}
			else if(show == false  &&  IsMenuShown(a_menuIndex))
            {
                m_menus[a_menuIndex].gameObject.SetActive(false);

                if (!m_menus[a_menuIndex].gameObject.activeSelf)
                {
                    m_menus[a_menuIndex] = null;
                    m_menuAsset.OnMenuHidden(a_menuIndex);
                }
            }            
		}

        //! @brief Save a menu state (the current visibility of the pages)
        //!
        //! @param a_menuIndex index of the menu (identifier used to restore a page state)
        public void SaveMenuState(uint a_menuIndex)
        {
            m_navigation.PushState(a_menuIndex, m_menus);
        }

        //! @brief Restore a menu state (if a save has been done for the given menu)
        //!
        //! @param a_menuIndex index of the state to restore
        //! @param previousState if false then the menu state is restore, else the previous state is restored
        public bool RestoreMenuState(uint a_menuIndex, bool previousState)
        {
            return (previousState)? m_navigation.PopState(a_menuIndex) : m_navigation.RestoreState(a_menuIndex); 
        }
  
        //! @brief Remove a menu from states
        //!
        //! @param a_menuIndex index of the menu (identifier used to restore a page state)
        public void RemoveMenuFromStates(uint a_menuIndex)
        {
            m_navigation.RemoveMenu(a_menuIndex);
        }
  
        //! @brief Get the last menu displayed
        //!
        //! @param menuIndex index of the menu returned
        public bool GetLastMenuDisplayed(out uint a_menuIndex)
        {
            a_menuIndex = (m_menus != null)? (uint)m_menus.Length : 0;

            if (m_menus == null)
            {
                return false;
            }

            for (uint i = 0; i < m_menus.Length; ++i)
            {
                if (m_menus[i] != null)
                {
                    if (a_menuIndex >= m_menus.Length || m_menus[i].DisplayStamp > m_menus[a_menuIndex].DisplayStamp)
                    {
                        a_menuIndex = i;
                    }
                }
            }
            return (a_menuIndex < m_menus.Length);
        }

		//! @brief Get the given menu
		//!
		//! @param menuIndex index of the Menu to check
        public T GetMenu<T>(uint a_menuIndex) where T : Component
		{
            if (m_menus == null)
            {
                return null;
            }

            if (m_menus[a_menuIndex] is T)
            {
                return m_menus[a_menuIndex] as T;
            }

            GameObject node = (m_menus[a_menuIndex] != null)? m_menus[a_menuIndex].gameObject : m_menuAsset.GetMenu(a_menuIndex);
            T[] components = (node != null)? node.GetComponentsInChildren<T>(true) : null;
            return (components != null && components.Length > 0)? components[0] : null;
		}

        //! @brief Checks if the given menu is shown
        //!
        //! @param a_menuIndex index of the menu to check
        public bool IsMenuShown(uint a_menuIndex)
        {
            return m_menus != null && a_menuIndex < m_menus.Length && m_menus[a_menuIndex] != null && m_menus[a_menuIndex].gameObject.activeSelf;
        }

        //! @brief Load the given menu
        //!
        //! @param menuIndex index of the menu to load
        public IEnumerator LoadMenu(uint a_menuIndex)
        {
            if (m_menuAsset != null && m_coroutineSupport != null)
            {
                yield return m_coroutineSupport.StartCoroutine(m_menuAsset.Load(a_menuIndex));

                UI.Page page = GetMenu<UI.Page>(a_menuIndex);

                if (page != null)
                {
                    yield return m_coroutineSupport.StartCoroutine(page.Load(m_coroutineSupport));
                }
            }
            else
            {
                Log.Error("UI Manager: the menu can't be loaded");
            }
        }

        //! @brief Unload the given menu
        //!
        //! @param menuIndex index of the menu to unload
        public void UnloadMenu(uint a_menuIndex)
        {
            ShowMenu(a_menuIndex, false);

            if (m_menuAsset != null)
            {
                m_menuAsset.Unload(a_menuIndex);
            }
        }

        //! @brief Load all the menus
        public IEnumerator LoadMenus()
        {
            if (m_coroutineSupport != null)
            {
                for (uint i = 0; i < MenusCount; ++i)
                {
                    yield return m_coroutineSupport.StartCoroutine(LoadMenu(i));
                }
            }
            else
            {
                Log.Error("UI Manager: the menus can't be loaded");
            }
        }

        //! @brief Unload the menus
        public void UnloadMenus()
        {
            for (uint i = 0; i < MenusCount; ++i)
            {
                UnloadMenu(i);
            }
        }
#region Internal
		internal void RequestPause(bool a_pause)
		{
			if(a_pause == false  &&  m_pauseRequests == 0)
			{
				Log.Warning("Unpause requested but the game is not in pause.");
				return;
			}

			if(a_pause)
			{
				++m_pauseRequests;

				if(m_pauseRequests == 1)
				{
                    if (m_pauseDelegate == null || !m_pauseDelegate(true))
                    {
                        Time.timeScale = 0;
                    }
				}
			}
			else
			{
				--m_pauseRequests;

				if(m_pauseRequests == 0)
				{
                    if (m_pauseDelegate == null || !m_pauseDelegate(false))
                    {
                        Time.timeScale = 1;
                    }
				}
			}
		}
#endregion

#region Internal
		//! @brief Called by a Menu page root
        internal void CloseMenu(UI.Page a_menu, bool navigationEnabled)
		{
            uint index = FindMenuIndex(a_menu);

            if (!IsMenuShown(index))
            {
                return;
            }
            
            if (!navigationEnabled || !RestoreMenuState(index, true))
            {
                ShowMenu(index, false);
            }
		}
        
        internal void OnPageActivation(UI.Page page, bool activated)
        {
            UpdateModal();

            UI.Page parent = page.ParentPage;
            uint childIndex = (parent == null)? FindMenuIndex(page) : parent.FindChildIndex(page);

            for (int i = 0; i < m_menus.Length; ++i)
            {
                if (m_menus[i] != null && (parent != null || i != childIndex))
                {
                    m_menus[i].OnPageActivation(parent, childIndex, activated);                
                }
            }

            if (parent == null)
            {
                if (activated)
                {
                    if (m_menus[childIndex].HasOption(UI.Page.Option.NAVIGATION_STATE))
                    {
                        SaveMenuState(childIndex);
                    }                
                }  
                else
                {
                    // Reset the navigation if all the menus are hidden
                    if (!m_navigation.Locked)
                    {
                        bool clearBackup = true;

                        for (uint i = 0; i < m_menus.Length && clearBackup; ++i)
                        {
                            clearBackup = !IsMenuShown(i);
                        }

                        if (clearBackup)
                        {
                            m_navigation.Clear();
                        }
                    }
                }
            }
        }
        
        //! @brief Check the pages and disable the input if the page isn't the current modal page
        internal void UpdateModal()
        {
            UI.Page modal = GetModalPage();

            for (int i = 0; i < m_menus.Length; ++i)
            {
                if (m_menus[i] != null)
                {
                    m_menus[i].SetInputEnabled(modal == null || m_menus[i].IsInFrontOf(modal), this);
                }
            }

            if (modal != null)
            {
                modal.SetInputEnabled(true, this);
            }
        }
#endregion

#region Private
#region Properties
		private bool IsValid
		{
			get
			{
				return m_root != null  &&  m_subHierarchyRoot != null;
			}
		}
	#endregion

	#region Methods
		private Transform FindSubHierarchyRoot()
		{
			if(m_root == null)
			{
				return null;
			}

			Transform it = m_root.transform;
			while(it.childCount == 1)
			{
				it = it.GetChild(0);
			}

			if(it.childCount > 1)
			{
				Log.Error("The prefab defined as NGUI Root have multiple children under object " + it.gameObject.name + ".");
	#if UNITY_EDITOR
				UnityEditor.Selection.activeGameObject = it.gameObject;
	#endif // UNITY_EDITOR
				return null;
			}

			return it;
		}

        private uint FindMenuIndex(UI.Page a_menu)
        {
            uint menuIndex = 0;
            while (menuIndex < m_menus.Length && m_menus[menuIndex] != a_menu)
            {
                ++menuIndex;
            }
            return menuIndex;
        }

        private UI.Page GetModalPage()
        {
            UI.Page candidate = null;

            for (int i = 0; i < m_menus.Length; ++i)
            {
                if (m_menus[i] != null && m_menus[i].gameObject.activeSelf)
                {
                    UI.Page modal = m_menus[i].GetModalPageInHierarchy();

                    if (modal != null && (candidate == null || modal.IsInFrontOf(candidate)))
                    {
                        candidate = modal;
                    }
                }
            }
            return candidate;
        }
#endregion
#endregion

#region Aube Clean AssetTools
		//! @brief load a resource or creates it if the asset is not present 
		[System.Obsolete]
		private static T LoadResource<T>() where T : ScriptableObject
		{
			return LoadResource<T>(typeof(T).Name);
		}
		
		//! @brief load a resource or creates it if the asset is not present 
		[System.Obsolete]
		private static T LoadResource<T>(string name) where T : ScriptableObject
		{
			T asset = Resources.Load(name) as T;
			
			if (asset == null) 
			{
				asset = CreateAsset< T >( "Assets/Resources/" + name + ".asset" );
			}
			
			if (asset == null)
			{
				Aube.Log.Warning("Resource '" + name + "' can't be loaded");
			}
			
			return asset;
		}
		
		//! @brief creates an asset
		[System.Obsolete]
		private static T CreateAsset<T>( string sFullPath ) where T : ScriptableObject
		{
			T oAsset = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
			if( oAsset != null )
			{				
				string path = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + "/" + sFullPath;
				string[] folders = sFullPath.Split('/');
				path = path.Substring(0, path.Length - folders[folders.Length - 1].Length);
				
				System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo(path);
				
				if (!folder.Exists)
					folder.Create();
				
				UnityEditor.AssetDatabase.CreateAsset( oAsset, sFullPath );			
				UnityEditor.AssetDatabase.SaveAssets();
				UnityEditor.AssetDatabase.Refresh();
			}
#endif
			
			return oAsset; 
		}
		
#if UNITY_EDITOR
		[System.Obsolete]
		private static T CreateAsset<T>() where T : ScriptableObject
		{
			return CreateAsset<T>(GetUniqueAssetPathNameOrFallback(typeof(T).ToString() + ".asset"));
		}        

		[System.Obsolete]
		private static string GetUniqueAssetPathNameOrFallback(string filename)
		{
			string path;
			try
			{
				System.Type assetdatabase = typeof(UnityEditor.AssetDatabase);
				path = (string)assetdatabase.GetMethod("GetUniquePathNameAtSelectedPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(assetdatabase, new object[] { filename });
			}
			catch
			{
				path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/" + filename);
			}
			return path;
		}
#endif
#endregion
	}
} // namespace Aube

#endif // !AUBE_NO_UI
