using UnityEngine;
using UnityEditor;

namespace Aube
{
	//! @class PlayScripts
	//!
	//! @brief Scripts that can be used to start and stop a playing application
	internal static class PlayScript
	{
		[MenuItem("Aube/Play Game _%g")]
		internal static void PlayGame()
		{
			PlayScene(null);
		}

		[MenuItem("Aube/Play Current Scene _%t")]
		public static void PlayCurrentScene()
		{
			PlayScene(EditorApplication.currentScene);
		}

		[MenuItem("Aube/Play Game _%g", true)]
		[MenuItem("Aube/Play Current Scene _%t", true)]
		internal static bool IsApplicationNotPlaying() { return EditorApplication.isPlaying == false  &&  EditorApplication.isPlayingOrWillChangePlaymode == false; }

		[MenuItem("Aube/Stop and reload current scene _%#q")]
		internal static void StopAndReloadCurrentScene()
		{
			if(EditorApplication.isPlaying == false)
			{
				return;
			}

			EditorApplication.isPlaying = false;

			if(EditorApplication.OpenScene(AubePreferences.currentScene) == false)
			{
				Debug.LogError("Editor failed to open '" + AubePreferences.currentScene + "'.");
				return;
			}
		}

		[MenuItem("Aube/Stop and reload current scene _%#q", true)]
		internal static bool IsApplicationPlaying() { return EditorApplication.isPlaying; }

#region Private
		private static void PlayScene(string a_scene)
		{
			if(EditorApplication.isPlaying)
			{
				return;
			}
			
			if(EditorBuildSettings.scenes.Length == 0)
			{
				Debug.LogError("There is no scene set in the BuildSettings.");
				return;
			}						

			string rootScene = EditorBuildSettings.scenes[0].path;

			AubePreferences.currentScene = EditorApplication.currentScene;
			AubePreferences.sceneToLaunch = (a_scene == rootScene)? null : a_scene;

			if(EditorApplication.currentScene != rootScene)
			{
				if(SaveCurrentScene() == false)
				{
					return;
				}
				
				if(EditorApplication.OpenScene(rootScene) == false)
				{
					Debug.LogError("Editor failed to open '" + rootScene + "'.");
					return;
				}
			}
			
			EditorApplication.isPlaying = true;
		}

		private static bool SaveCurrentScene()
		{
			// if the scene has no name, it means it has not been saved once before, so we discard it.
			if(AubePreferences.alwaysSaveCurrentScene  &&  string.IsNullOrEmpty(EditorApplication.currentScene))
			{
				return true;
			}

			int dialogResult = AubePreferences.alwaysSaveCurrentScene? 0 :
					EditorUtility.DisplayDialogComplex("Unsaved Scene",
					                                   "Current edited scene was not saved. All unsaved changes will be lost. Do you want to save?",
					                                   "Yes",
					                                   "No",
					                                   "Always Save");
				
			if(dialogResult == 2)
			{
				AubePreferences.alwaysSaveCurrentScene = true;
			}

			if((dialogResult == 0  ||  dialogResult == 2))
			{
				if(string.IsNullOrEmpty(EditorApplication.currentScene))
				{
					return EditorApplication.SaveCurrentSceneIfUserWantsTo();
				}
				else if(EditorApplication.SaveScene() == false)
				{
					Debug.LogError("Editor failed to save '" + EditorApplication.currentScene + "'.");
					return false;
				}
			}

			return true;
		}
#endregion
	}
} // namespace Aube
