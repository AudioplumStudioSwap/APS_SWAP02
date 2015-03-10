using UnityEngine;
using System.Collections;

//TODO : Check if atlas stays in memory
namespace Aube
{
	public class LoadingManager : MonoBehaviour
    {
    #region Attributes
    #region Private
        [SerializeField]
        protected float m_minDuration = 1.0f;
        [SerializeField]
        protected float m_deferredDuration = 0.0f;

        private const float FRAME_TIME = 0.03f;

        private static LoadingManager m_instance = null;
        private static string m_loadingScreen = "";        
        protected static string m_levelToLoad = "";
        protected static System.Type m_gameModeToLoad = null;

        protected bool m_loadingInProgress = false;
        protected float m_elapsedTime = 0.0f;
        private float m_lastUpdateTime = 0.0f;
        protected AsyncOperation m_asyncStatus;
        protected bool m_changeScene = true;
    #endregion
    #endregion

    #region Methods
    #region Public
        public static string LoadingEvent
        {
            get { return "OnLoadingScene"; }
        }

        public static string LevelToLoad
        {
            get { return m_levelToLoad; }
        }

        public static bool NeedUpdate
        {
            get
            {
                if (m_instance != null)
                {
                    return Time.realtimeSinceStartup - m_instance.m_lastUpdateTime > FRAME_TIME;
                }
                return false;
            }
        }

        public static void LoadLevel(string levelName, System.Type gameMode, string loadingScreen)
        {
            ISingleton.ProcessMessageAll(LoadingEvent);
            Time.timeScale = 1.0f;
            Aube.GameManager.Instance.Reset();

            m_loadingScreen = loadingScreen;
            m_gameModeToLoad = gameMode;
            m_levelToLoad = levelName;

            Application.LoadLevel(m_loadingScreen);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    #endregion
    #region Protected
        protected virtual void Awake()
        {
            m_instance = this;
            DontDestroyOnLoad(this);
        }

        protected virtual void Start()
        {
            m_loadingInProgress = true;
            m_elapsedTime = 0.0f;
            StartCoroutine(Load(m_minDuration));
        }

        protected void Update()
        {
            m_lastUpdateTime = Time.realtimeSinceStartup;
            m_elapsedTime += Time.deltaTime;

            // Wait a few more frames
            if (m_instance != this || (!m_loadingInProgress && m_elapsedTime >= m_deferredDuration))
            {
                // Auto destroy
                m_instance = (m_instance != this)? m_instance : null;
                DestroyObject(gameObject);
            }
        }
    #endregion
    #region Private
        private IEnumerator Load(float minDuration)
        {
            Log.Info("Loading : " + m_levelToLoad + "(scene) ; " + ((m_gameModeToLoad != null)? m_gameModeToLoad.Name : "null") + "(game mode)");

            if (m_gameModeToLoad != null)
            {
                Aube.GameManager.Instance.SetGameModeType(m_gameModeToLoad);

                while (!Aube.GameManager.Instance.GameMode.Loaded)
                {
                    yield return null;
                }
            }

            m_asyncStatus = Application.LoadLevelAsync(m_levelToLoad);
            m_asyncStatus.allowSceneActivation = false;

            while (m_minDuration > m_elapsedTime)
            {
                yield return null;
            }

            while (m_asyncStatus.progress < 0.9f)
            {
                yield return null;
            }

            while (!m_changeScene)
            {
                yield return null;
            }

            m_asyncStatus.allowSceneActivation = true;
            yield return m_asyncStatus;
            
            if (Aube.GameManager.Instance.GameMode)
            {
                Aube.GameManager.Instance.GameMode.enabled = true;
            }

            m_elapsedTime = 0.0f;
            m_loadingInProgress = false;
        }
    #endregion
    #endregion
	}
} // namespace Aube
