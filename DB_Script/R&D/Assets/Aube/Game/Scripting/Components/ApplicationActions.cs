using UnityEngine;
using System.Collections;

namespace Aube
{
	//! @class ApplicationActions
	//!
	//! @brief Component that allows users to trigger actions relative to the application
	[AddComponentMenu("Scripts/Aube/Application/Application Actions")]
	public class ApplicationActions : MonoBehaviour
	{
		public void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif // UNITY_EDITOR
		}
	}
}
