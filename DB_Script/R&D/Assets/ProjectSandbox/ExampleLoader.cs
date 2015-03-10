using UnityEngine;
using System.Collections;

namespace Aube.Sandbox
{
	[AddComponentMenu("Sandbox/Example Loader")]

	//! @class ExampleLoader
	//!
	//! @brief Behaviour that loads the current example on Start
	public class ExampleLoader : MonoBehaviour
	{
		public static Example currentExample;

#region Unity Callbacks
		private void Start()
		{
			useGUILayout = false;
			m_quitRequested = false;

			Aube.Assertion.Check(currentExample != null, "The current example should not be null.");
			Aube.Assertion.Check(currentExample.type != null, "The current example type should not be null.");

			new GameObject("Example : " + currentExample.path, currentExample.type);
		}

		private void OnGUI()
		{
			GUIContent labelContent = new GUIContent("Current example : " + currentExample.path);
			Vector2 labelSize = GUI.skin.label.CalcSize(labelContent);

			GUIContent buttonQuitContent = new GUIContent("Quit");
			Vector2 buttonQuitSize = GUI.skin.button.CalcSize(buttonQuitContent);

			Rect labelRect = new Rect(Screen.width - buttonQuitSize.x - labelSize.x - 10.0f, 0.0f, labelSize.x, labelSize.y);
			Rect buttonQuitRect = new Rect(Screen.width - buttonQuitSize.x, 0.0f, buttonQuitSize.x, buttonQuitSize.y);

			GUI.Label(labelRect, labelContent);
			if(GUI.Button(buttonQuitRect, buttonQuitContent))
			{
				m_quitRequested = true;
			}
		}

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				m_quitRequested = true;
			}

			if(m_quitRequested)
			{
				m_quitRequested = false;
				LoadingManager.LoadLevel("mainMenu", null, "LoadingScreen");
			}
		}
#endregion

#region Private
		private bool m_quitRequested;
#endregion
	}
}
