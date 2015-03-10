using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace Aube
{
	public class EntryPoint : MonoBehaviour
	{
        [SerializeField]
        private string m_startScene;// = "MenuRoot";
	#if INAPP_MGR
	    private List<InAppProductData> m_pInAppProduct = new List<InAppProductData>();
	#endif

        public string StartScene
        {
            get { return m_startScene; }
            set { m_startScene = value; }
        }

	    public virtual void Awake()
	    {
	#if INAPP_MGR
	        InAppManager.Instance.OnProductDataReceived += InAppProductDataReceived;
	#endif
	        //AutoChooseQualityLevel();
	        DontDestroyOnLoad(this);
	    }

	    private void DoDestroy()
	    {
	#if INAPP_MGR
	        InAppManager.Instance.OnProductDataReceived -= InAppProductDataReceived;
	#endif
	    }

	    public virtual void Start()
	    {
	#if INAPP_MGR
	        UnityEngine.Object[] vInApp = Resources.LoadAll("InApp", typeof(InAppCarac));

	        string[] _Product = new string[vInApp.Length];
	        for (int i = 0; i < vInApp.Length; ++i)
	        {
	            if (vInApp[i] is InAppCarac)
	            {
	                InAppCarac vCarac = (InAppCarac)vInApp[i];
	                _Product[i] = vCarac.ProdutId;
	            }
	        }
	        InAppManager.Instance.CollectStoreInfo(_Product);
	#endif
			Log.Init();
	        GameManager.Instance.Init();
    #if DEBUG_MGR
	        DebugManager.Instance.Start();
    #endif
	        InitGameConfigurator();


#if UNITY_EDITOR
			string sceneToLaunch = AubePreferences.sceneToLaunch;
			if(string.IsNullOrEmpty(sceneToLaunch) == false)
			{
				string[] folders = AubePreferences.sceneToLaunch.Split('/');
				string scene = folders[folders.Length - 1];
				string sceneWithoutExtension = scene.Substring(0, scene.Length - ".unity".Length);
				
				StartScene = sceneWithoutExtension;

				AubePreferences.sceneToLaunch = null;
			}
#endif // UNITY_EDITOR
			LoadStartScene();
	    }

	    public virtual void Update()
	    {
	#if DEBUG_MGR
			DebugManager.Instance.Update();
	#endif
	    }

		protected virtual void InitGameConfigurator()
		{
			
		}       

        protected virtual void LoadStartScene()
        {
            LoadingManager.LoadLevel(StartScene, null, "LoadingScreen");
        }
        
		private void OnLevelWasLoaded(int levelId)
		{
			string levelName;
			levelName = Application.loadedLevelName;
			OnLevelChanged(levelName);
		}

		protected virtual void OnLevelChanged(string newLevel) { }
		
	#if INAPP_MGR
	    public void InAppProductDataReceived(List<InAppProductData> pList)
	    {
	        m_pInAppProduct = pList;
	    }

	    public InAppProductData GetInAppData(string sProductID)
	    {
	        if (m_pInAppProduct.Count == 0)
	        {
	            return null;
	        }

	        foreach (InAppProductData pData in m_pInAppProduct)
	        {
	            if (pData.ProductID == sProductID)
	            {
	                return pData;
	            }
	        }
	        return null;
	    }
	#endif
	}
} // namespace Aube