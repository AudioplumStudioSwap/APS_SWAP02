using System;
using System.Reflection;
using UnityEngine;

namespace Aube
{
	public class GameManager : Singleton<GameManager>
	{
	    #region Members
	    protected GameMode m_pGameMode = null;
	    protected GameObject m_gameManagerObject = null;
	    protected int m_iCurrentGameMode;
	    protected int m_iPreviousGameMode;

	#if NETWORK_MGR
	    private NetworkView m_pNetworkView;
	#endif
	    #endregion
		
	    public GameMode GameMode
	    {
	        get
	        {
	            return m_pGameMode;
	        }
	    }
		
		public int CurrentGameMode
	    {
	        get { return m_iCurrentGameMode; }
	    }

	#if NETWORK_MGR
	    public NetworkView NetworkView
	    {
	        get
	        {
	            return m_pNetworkView;
	        }
	    }
	#endif

	    public GameManager()
	    {
	        m_pGameMode = null;
	        m_gameManagerObject = new GameObject("Game Manager");
	#if NETWORK_MGR
	        m_pNetworkView = (NetworkView)m_pGameModeObject.AddComponent(typeof(NetworkView));
	        if (m_pNetworkView)
	        {
	            m_pNetworkView.stateSynchronization = NetworkStateSynchronization.Off;
	            m_pNetworkView.observed = null;
	        }
	#endif

#if AUBE_SHOW_STATS
			GameObject statsDisplay = new GameObject("Stats Display");
			statsDisplay.AddComponent<DebugShowStats>();
			statsDisplay.transform.parent = m_gameManagerObject.transform;
#endif // AUBE_SHOW_STATS

	        UnityEngine.Object.DontDestroyOnLoad(m_gameManagerObject);
	    }

	    public void Init()
	    {

	    }

	    public void Reset()
	    {
	        if (m_pGameMode != null)
	        {
	            m_pGameMode.Dispose();
	            GameObject.Destroy(m_pGameMode.gameObject);
	            m_pGameMode = null;
	        }
	    }

        public void SetGameModeType(System.Type gameMode)
        {
            if (typeof(Aube.GameMode).IsAssignableFrom(gameMode))
            {
                MethodInfo method = typeof(GameManager).GetMethod("SetGameMode", BindingFlags.Instance | BindingFlags.Public);
                MethodInfo genericMethod = (method != null) ? method.MakeGenericMethod(gameMode) : null;
                Assertion.Check(genericMethod != null, "method not found");
                genericMethod.Invoke(this, new object[] {-1});
            }
            else
            {
                Log.Error("Invalid game mode type");
            }
        }

	    public void SetGameMode<T>(int iNewGM) where T : GameMode
	    {
	        Reset();
	        m_iPreviousGameMode = m_iCurrentGameMode;
	        m_iCurrentGameMode = iNewGM;

			GameObject gameMode = new GameObject("Game Mode", typeof(T));
			gameMode.transform.parent = m_gameManagerObject.transform;
			m_pGameMode = gameMode.GetComponent<T>();
	#if NETWORK_MGR
	        NetworkManager networkMgr = (NetworkManager)GameObject.FindObjectOfType(typeof(NetworkManager));

	        if (networkMgr)
	        {
	            m_pGameModeObject.networkView.viewID = networkMgr.GameModeId;
	        }
	#endif
	    }

	    public T GetGameMode<T>() where T : GameMode
	    {
	        return m_pGameMode as T;
	    }

        public void SetTag(string Tag)
        {
            if(m_gameManagerObject)
            {
                m_gameManagerObject.tag = Tag;
            }
        }
	}
} // namespace Aube