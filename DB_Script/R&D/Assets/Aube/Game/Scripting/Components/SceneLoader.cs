using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class SceneLoader
	//!
	//! @brief Component that allows you to load a scene set by users
	[AddComponentMenu("Scripts/Aube/Loading/Scene Loader")]
	public class SceneLoader : MonoBehaviour
	{
		[SerializeField]
		private string m_sceneName = null;

		[SerializeField]
		private string m_loadingScreen = "LoadingScreen";

		public void LoadScene()
		{
            Aube.LoadingManager.LoadLevel(m_sceneName, null, m_loadingScreen);
		}
	}
}