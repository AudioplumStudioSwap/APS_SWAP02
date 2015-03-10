using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Aube
{
	[InitializeOnLoad]
	public class AssertionDialog : EditorWindow
	{
		static AssertionDialog()
		{
			Assertion.displayDialogCallback = Show;
		}

#region Unity Callbacks
		private void OnGUI()
		{
			GUI.skin = AubeEditor.skin;

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = m_currentDataIndex > 0;
				if(GUILayout.Button("<<"))
				{
					m_currentDataIndex = 0;
				}
				if(GUILayout.Button("<"))
				{
					m_currentDataIndex = Mathf.Max(m_currentDataIndex - 1, 0);
				}
				GUI.enabled = m_currentDataIndex < m_data.Count - 1;
				if(GUILayout.Button(">"))
				{
					m_currentDataIndex = Mathf.Min(m_currentDataIndex + 1, m_data.Count - 1);
				}
				if(GUILayout.Button(">>"))
				{
					m_currentDataIndex = m_data.Count - 1;
				}
				GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Label("Message :");
			EditorGUILayout.HelpBox(currentMessage, MessageType.Error);

			GUILayout.Space(10.0f);

			GUILayout.Label("Stack Trace :");
			m_scroll = EditorGUILayout.BeginScrollView(m_scroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);
			{
				System.Diagnostics.StackFrame[] frames = currentStackTrace.GetFrames();
				for(int frameIndex = 2; frameIndex < frames.Length; ++frameIndex)
				{
					EditorGUILayout.BeginHorizontal();

					string advancedInformation = frames[frameIndex].GetFileName() + " (" + frames[frameIndex].GetFileLineNumber() + ")\n";
					advancedInformation += frames[frameIndex].GetMethod();

					string styleName = (frameIndex % 2) == 0? "Assertion Stack Pair" : "Assertion Stack Impair";
					GUILayout.Label(advancedInformation, styleName, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

					if(GUILayout.Button("Open", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false)))
					{
						OpenStackFrame(frameIndex);
					}

					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndScrollView();

			GUILayout.Space(10.0f);

			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Quit"))
				{
					m_closeAction = CloseAction.Quit;
					Close();
				}
				if(GUILayout.Button("Continue"))
				{
					m_closeAction = CloseAction.Continue;
					Close();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void Update()
		{
			if(EditorApplication.isPlaying == false  &&  EditorApplication.isPaused == false)
			{
				m_closeAction = CloseAction.None;
				Close();
			}
		}

		private void OnDestroy()
		{
			switch(m_closeAction)
			{
				case CloseAction.Continue: 	EditorApplication.isPaused = false;		break;
				case CloseAction.Quit:		EditorApplication.isPlaying = false;	break;
			}
		}
#endregion

#region Private
		private static void Show(string a_message, System.Diagnostics.StackTrace a_stackTrace)
		{
			EditorApplication.isPaused = true;

			AssertionDialog dialog = GetWindow<AssertionDialog>(true, "Assertion", true);
			dialog.ShowUtility();

			dialog.Push(a_message, a_stackTrace);
		}

		private void Push(string a_message, System.Diagnostics.StackTrace a_stackTrace)
		{
			if(m_data == null)
			{
				m_data = new List<AssertionData>();
				m_currentDataIndex = 0;
			}

			m_data.Add(new AssertionData{ message = a_message, stackTrace = a_stackTrace });
		}

		private void OpenStackFrame(int a_frameIndex)
		{
			System.Diagnostics.StackFrame frame = currentStackTrace.GetFrame(a_frameIndex);
			string assetRelativePath = frame.GetFileName();
			assetRelativePath = assetRelativePath.Replace('\\', '/');
			if(assetRelativePath.StartsWith(UnityEngine.Application.dataPath))
			{
				assetRelativePath = assetRelativePath.Substring(UnityEngine.Application.dataPath.Length - "Assets".Length);
				UnityEngine.Object assetFile = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetRelativePath);
				UnityEditor.AssetDatabase.OpenAsset(assetFile, frame.GetFileLineNumber());
			}
		}

	#region Properties
		private string currentMessage
		{
			get{ return m_data[m_currentDataIndex].message; }
		}

		private System.Diagnostics.StackTrace currentStackTrace
		{
			get{ return m_data[m_currentDataIndex].stackTrace; }
		}
	#endregion

	#region Attributes
		enum CloseAction
		{
			None,
			Continue,
			Quit,
		}
		private CloseAction m_closeAction = CloseAction.Quit;

		private struct AssertionData
		{
			public string message;
			public System.Diagnostics.StackTrace stackTrace;
		}
		private List<AssertionData> m_data;
		private int m_currentDataIndex;

		private Vector2 m_scroll = Vector2.zero;
	#endregion
#endregion
	}
}
