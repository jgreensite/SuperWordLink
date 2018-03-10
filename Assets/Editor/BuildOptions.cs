using UnityEngine;
using System.Collections;
using UnityEditor;

public class BuildHeadlessServerUnix {
	public static void Perform() {
		var sceneArray = new EditorBuildSettingsScene[1];
		sceneArray[0] = new EditorBuildSettingsScene("Assets/Server.unity", true);
		EditorBuildSettings.scenes = sceneArray;
		BuildPipeline.BuildPlayer (GetScenePaths (), "Builds/OSX/" + GetProjectName () + "Server" + ".app", BuildTarget.StandaloneOSX, BuildOptions.AutoRunPlayer|BuildOptions.EnableHeadlessMode);
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
