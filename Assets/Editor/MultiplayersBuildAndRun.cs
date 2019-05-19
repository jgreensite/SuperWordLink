using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Script;

namespace Editor
{

//Originally inspired by https://www.appsfresh.com/blog/multiplayer/

	public static class MultiplayersBuildAndRun
	{

		//Store old list of scenes
//	static int oldSceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;     
//	static EditorBuildSettingsScene[] oldScenes = new EditorBuildSettingsScene[oldSceneCount];

		static BuildPlayerOptions newBuildPlayerOptions = new BuildPlayerOptions();

		//Client Builds
		[MenuItem("File/Run Multiplayer/Windows/1 Player")]
		static void PerformWin64ClientBuild1()
		{
			PerformBuild(1, CS.WINBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Windows/2 Players")]
		static void PerformWin64ClientBuild2()
		{
			PerformBuild(2, CS.WINBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Windows/3 Players")]
		static void PerformWin64ClientBuild3()
		{
			PerformBuild(3, CS.WINBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Windows/4 Players")]
		static void PerformWin64ClientBuild4()
		{
			PerformBuild(4, CS.WINBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Mac OSX/1 Player")]
		static void PerformOSXClientBuild1()
		{
			PerformBuild(1, CS.OSXBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Mac OSX/2 Players")]
		static void PerformOSXClientBuild2()
		{
			PerformBuild(2, CS.OSXBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Mac OSX/3 Players")]
		static void PerformOSXClientBuild3()
		{
			PerformBuild(3, CS.OSXBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Mac OSX/4 Players")]
		static void PerformOSXClientBuild4()
		{
			PerformBuild(4, CS.OSXBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/iOS/1 Player")]
		static void PerformiOSClientBuild1()
		{
			PerformBuild(1, CS.IOSBUILDPLATFORM, CS.CLIENTSCENECOLLECTION);
		}

		//Server Builds	
		[MenuItem("File/Run Multiplayer/Windows Server")]
		static void PerformWin64ServerBuild1()
		{
			PerformBuild(1, CS.WINBUILDPLATFORM, CS.SERVERSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Mac OSX Server")]
		static void PerformOSXServerBuild1()
		{
			PerformBuild(1, CS.OSXBUILDPLATFORM, CS.SERVERSCENECOLLECTION);
		}

		[MenuItem("File/Run Multiplayer/Unix Server")]
		static void PerformUnixServerBuild1()
		{
			PerformBuild(1, CS.UNXBUILDPLATFORM, CS.SERVERSCENECOLLECTION);
		}

		static void PerformBuild(int playerCount, string buildPlatform, string buildScenes)
		{
			for (int i = 1; i <= playerCount; i++)
			{
				switch (buildPlatform)
				{
					case CS.OSXBUILDPLATFORM:
						newBuildPlayerOptions.target = BuildTarget.StandaloneOSX;
						newBuildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
						newBuildPlayerOptions.locationPathName =
							"Builds/OSX/" + GetProjectName() + buildScenes + i.ToString() + ".app";
						break;

					case CS.WINBUILDPLATFORM:
						newBuildPlayerOptions.target = BuildTarget.StandaloneWindows;
						newBuildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
						newBuildPlayerOptions.locationPathName =
							"Builds/Win64/" + GetProjectName() + buildScenes + i.ToString() + ".exe";

						break;

					case CS.UNXBUILDPLATFORM:
						newBuildPlayerOptions.target = BuildTarget.StandaloneLinuxUniversal;
						newBuildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
						newBuildPlayerOptions.locationPathName =
							"Builds/Linux/" + GetProjectName() + buildScenes + i.ToString();
						break;

					case CS.IOSBUILDPLATFORM:
						newBuildPlayerOptions.target = BuildTarget.iOS;
						newBuildPlayerOptions.targetGroup = BuildTargetGroup.iOS;
						newBuildPlayerOptions.locationPathName =
							"Builds/iOS/" + GetProjectName() + buildScenes + i.ToString();
						break;

					case CS.ANDBUILDPLATFORM:
						newBuildPlayerOptions.target = BuildTarget.Android;
						newBuildPlayerOptions.targetGroup = BuildTargetGroup.Android;
						newBuildPlayerOptions.locationPathName =
							"Builds/Android/" + GetProjectName() + buildScenes + i.ToString() + ".apk";
						break;

				}

				newBuildPlayerOptions.scenes = ChooseScenes(buildScenes);
				if (buildScenes == CS.SERVERSCENECOLLECTION)
				{
					newBuildPlayerOptions.options = BuildOptions.EnableHeadlessMode;
				}

				//newBuildPlayerOptions.options = BuildOptions.AutoRunPlayer;
				BuildPipeline.BuildPlayer(newBuildPlayerOptions);
			}
		}

		static string GetProjectName()
		{
			string[] s = Application.dataPath.Split('/');
			return s[s.Length - 2];
		}

		static string[] GetScenePaths()
		{
			string[] scenes = new string[EditorBuildSettings.scenes.Length];

			for (int i = 0; i < scenes.Length; i++)
			{
				scenes[i] = EditorBuildSettings.scenes[i].path;
			}

			return scenes;
		}

//	static void SwitchScenes(string sceneCollection)
//	{
//		//Now switch for the server scene
//		EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[1];
//		switch(sceneCollection)
//		{
//		case (CS.SERVERSCENECOLLECTION):
//			newScenes[0] = new EditorBuildSettingsScene("Assets/Scene/Server.unity", true);
//			break;
//		case (CS.CLIENTSCENECOLLECTION):
//			newScenes[0] = new EditorBuildSettingsScene("Assets/Scene/Menu.unity", true);
//			newScenes[1] = new EditorBuildSettingsScene("Assets/Scene/Main.unity", true);
//			break;
//		}
//	}

		static string[] ChooseScenes(string sceneCollection)
		{
			string[] newScenes;

			//Now switch for the server scene;
			if (sceneCollection == CS.SERVERSCENECOLLECTION)
			{
				newScenes = new string[1];
			}
			else
			{
				newScenes = new string[2];
			}

			switch (sceneCollection)
			{
				case (CS.SERVERSCENECOLLECTION):
					newScenes[0] = "Assets/Scene/Server.unity";
					break;
				case (CS.CLIENTSCENECOLLECTION):
					newScenes[0] = "Assets/Scene/Menu.unity";
					newScenes[1] = "Assets/Scene/Main.unity";
					break;
			}

			return (newScenes);
		}
	}
}