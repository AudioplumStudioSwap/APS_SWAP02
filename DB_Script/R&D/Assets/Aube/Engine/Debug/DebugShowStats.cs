using UnityEngine;
using System.Collections;

namespace Aube
{
	[AddComponentMenu("")]

	//!	@class	DebugShowStats
	//!
	//!	@brief	Component that displays statistics of the game (Fps, ...)
	internal class DebugShowStats : MonoBehaviour
	{
#region Unity Callbacks
		private void Awake()
		{
			m_isFoldoutOpened = true;			// true because some platforms does not have a mouse or a pointing device
			m_updateInterval = 0.5f;			// do not update each frame because the value will change to wildly to be seen

			// update variables
			m_timeElapsedSinceLastUpdate = 0.0f;
			m_frameCountOnLastUpdate = 0;

			// statistics variables
			m_fps = 0;
		}

		private void OnGUI()
		{
			GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
			{
				if(GUILayout.Button("Statistics"))
				{
					m_isFoldoutOpened = !m_isFoldoutOpened;
				}

				if(m_isFoldoutOpened)
				{
					// FPS
					GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
					{
						GUILayout.Label("FPS", GUILayout.Width(50.0f));
						GUILayout.Label(m_fps.ToString());
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndHorizontal();
		}

		private void Update()
		{
			m_timeElapsedSinceLastUpdate += Time.unscaledDeltaTime;
			if(m_timeElapsedSinceLastUpdate >= m_updateInterval)
			{
				// Do the update of all statistics variables
				m_fps = Mathf.RoundToInt((Time.frameCount - m_frameCountOnLastUpdate) / m_timeElapsedSinceLastUpdate);

				// reset update variables
				m_timeElapsedSinceLastUpdate = 0.0f;
				m_frameCountOnLastUpdate = Time.frameCount;
			}
		}
#endregion
		
#region Private
	#region Attributes
		private bool m_isFoldoutOpened;
		private float m_updateInterval;

		//! update variables
		private float m_timeElapsedSinceLastUpdate;
		private int m_frameCountOnLastUpdate;

		//! statistics variables
		private int m_fps;
	#endregion
#endregion
	}
}