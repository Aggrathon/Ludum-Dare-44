using System.Diagnostics;
using System.IO;
using UnityEditor;

/*
  This scripts adds a menu option for exporting the game to multiple platforms.
  The platforms are Windows, Linux, Mac and WebGL. The script is useful for Ludum Dare or other gamejams.
  This script is dependent on having 7-zip installed for zipping the results and is written for Windows.
  IMPORTANT: The file must be placed within a folder called 'Editor' (somewhere in the Unity Assets folder).
  GIST available here: https://gist.github.com/Aggrathon/f854e9ab73569883647ba637edee4c1d
*/
public class BuildPipelineScript : EditorWindow
{
	[MenuItem("File/Build For All Platforms")]
	static void DoIt()
	{

		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
		string name = PlayerSettings.productName + " " + PlayerSettings.companyName;
		name = name.Replace(" ", "_");
		name = name.Replace("'", "");
		BuildPlatform(path, name, BuildTarget.StandaloneWindows64);
		BuildPlatform(path, name, BuildTarget.StandaloneLinux64);
		BuildPlatform(path, name, BuildTarget.StandaloneOSX);
		BuildPlatform(path, name, BuildTarget.WebGL);
		Process.Start(path);
	}

	[MenuItem("File/Build WebGL")]
	static void DoIt2()
	{
		string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
		string name = PlayerSettings.productName + " " + PlayerSettings.companyName;
		name = name.Replace(" ", "_");
		name = name.Replace("'", "");
		BuildPlatform(path, name, BuildTarget.WebGL);
		Process.Start(path);
	}

	static void BuildPlatform(string path, string name, BuildTarget platform)
	{
		switch (platform)
		{
			case BuildTarget.StandaloneOSX:
				string folder = Path.Combine(path, "mac");
				BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, Path.Combine(folder, name), platform, BuildOptions.None);
				Compress(folder, name + "_MAC", path);
				break;
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				folder = Path.Combine(path, "win");
				BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, Path.Combine(folder, name + ".exe"), platform, BuildOptions.None);
				Compress(folder, name + "_WIN", path);
				break;
			case BuildTarget.WebGL:
				folder = Path.Combine(path, "web");
                PlayerSettings.WebGL.wasmStreaming = true;
                PlayerSettings.WebGL.threadsSupport = true;
                PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
                PlayerSettings.WebGL.threadsSupport = true;
                PlayerSettings.WebGL.memorySize = 512;
				BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, folder, platform, BuildOptions.None);
				break;
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
				folder = Path.Combine(path, "lin");
				BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, Path.Combine(folder, name), platform, BuildOptions.None);
				Compress(folder, name + "_LIN", path);
				break;
		}
	}

	static void Compress(string folder, string name, string targetFolder)
	{
		ProcessStartInfo inf = new ProcessStartInfo("C:\\Program Files\\7-Zip\\7z.exe", "a \"" + Path.Combine(targetFolder, name + ".zip") + "\" \"" + Path.Combine(folder, "*") + "\"");
		inf.CreateNoWindow = true;
		var p = Process.Start(inf);
		p.WaitForExit();
	}
}