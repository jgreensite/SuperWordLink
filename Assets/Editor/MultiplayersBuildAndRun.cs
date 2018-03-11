using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//https://www.appsfresh.com/blog/multiplayer/

public static class MultiplayersBuildAndRun {

	[MenuItem("File/Run Multiplayer/Windows/2 Players")]
	static void PerformWin64Build2 (){
		PerformWin64Build (2);
	}

	[MenuItem("File/Run Multiplayer/Windows/3 Players")]
	static void PerformWin64Build3 (){
		PerformWin64Build (3);
	}

	[MenuItem("File/Run Multiplayer/Windows/4 Players")]
	static void PerformWin64Build4 (){
		PerformWin64Build (4);
	}

	static void PerformWin64Build (int playerCount)
	{
		//EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
		for (int i = 1; i <= playerCount; i++) {
			BuildPipeline.BuildPlayer (GetScenePaths (), "Builds/Win64/" + GetProjectName () + i.ToString() + ".exe", BuildTarget.StandaloneWindows, BuildOptions.AutoRunPlayer);
		}
	}

	[MenuItem("File/Run Multiplayer/Mac OSX/2 Players")]
	static void PerformOSXBuild2 (){
		PerformOSXBuild (2);
	}

	[MenuItem("File/Run Multiplayer/Mac OSX/3 Players")]
	static void PerformOSXBuild3 (){
		PerformOSXBuild (3);
	}

	[MenuItem("File/Run Multiplayer/Mac OSX/4 Players")]
	static void PerformOSXBuild4 (){
		PerformOSXBuild (4);
	}

	[MenuItem("File/Run Multiplayer/Mac OSX Server")]
	static void PerformOSXServerBuild1 (){
		PerformOSXServerBuild ();
	}

	[MenuItem("File/Run Multiplayer/Unix Server")]
	static void PerformUnixServerBuild1 (){
		PerformUnixServerBuild ();
	}

	static void PerformOSXBuild (int playerCount)
	{
		//EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSX);
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
		for (int i = 1; i <= playerCount; i++) {
			BuildPipeline.BuildPlayer (GetScenePaths (), "Builds/OSX/" + GetProjectName () + i.ToString() + ".app", BuildTarget.StandaloneOSX, BuildOptions.AutoRunPlayer);
		}

	}

	public static void PerformOSXServerBuild() {

		//Store old list of scenes
		int oldSceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;     
		EditorBuildSettingsScene[] oldScenes = new EditorBuildSettingsScene[oldSceneCount];
		oldScenes = EditorBuildSettings.scenes;

		//Now switch for the server scene
		EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[1];
		newScenes[0] = new EditorBuildSettingsScene("Assets/Scene/Server.unity", true);
		EditorBuildSettings.scenes = newScenes;
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
		BuildPipeline.BuildPlayer (GetScenePaths (), "Builds/OSX/" + GetProjectName () + "Server" + ".app",BuildTarget.StandaloneOSX,
			BuildOptions.AutoRunPlayer|BuildOptions.EnableHeadlessMode|BuildOptions.AllowDebugging);

		//Now switch the list of scenes back
		EditorBuildSettings.scenes = oldScenes;
	}

	public static void PerformUnixServerBuild() {

		//Store old list of scenes
		int oldSceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;     
		EditorBuildSettingsScene[] oldScenes = new EditorBuildSettingsScene[oldSceneCount];
		oldScenes = EditorBuildSettings.scenes;

		//Now switch for the server scene
		EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[1];
		newScenes[0] = new EditorBuildSettingsScene("Assets/Scene/Server.unity", true);
		EditorBuildSettings.scenes = newScenes;
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinuxUniversal);
		BuildPipeline.BuildPlayer (GetScenePaths (), "Builds/Unix/" + GetProjectName () + "Server",BuildTarget.StandaloneLinuxUniversal,
			BuildOptions.AutoRunPlayer|BuildOptions.EnableHeadlessMode|BuildOptions.AllowDebugging);

		//Now switch the list of scenes back
		EditorBuildSettings.scenes = oldScenes;
	}


	static string GetProjectName()
	{
		string[] s = Application.dataPath.Split('/');
		return s[s.Length - 2];
	}

	static string[] GetScenePaths()
	{
		string[] scenes = new string[EditorBuildSettings.scenes.Length];

		for(int i = 0; i < scenes.Length; i++)
		{
			scenes[i] = EditorBuildSettings.scenes[i].path;
		}

		return scenes;
	}

}