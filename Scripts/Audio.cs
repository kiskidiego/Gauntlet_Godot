using Godot;
using System;

public class Audio
{
	public static AudioBusLayout audioBusLayout = ResourceLoader.Load<AudioBusLayout>("res://Audio/AudioBusLayout.tres");
	static AudioStreamPlayer musicPlayer;
	public static float musicVolume 
	{ 
		get 
		{ 
			return AudioServer.Singleton.GetBusVolumeDb(AudioServer.Singleton.GetBusIndex("Music")); 
		} 
		set 
		{ 
			AudioServer.Singleton.SetBusVolumeDb(AudioServer.Singleton.GetBusIndex("Music"), value);
			GD.Print("Music volume set to " + musicVolume);
		} 
	}
	public static float sfxVolume
	{
		get
		{
			return AudioServer.Singleton.GetBusVolumeDb(AudioServer.Singleton.GetBusIndex("SFX"));
		}
		set
		{
			AudioServer.Singleton.SetBusVolumeDb(AudioServer.Singleton.GetBusIndex("SFX"), value);
			GD.Print("SFX volume set to " + sfxVolume);
		}
	}
	public static AudioStreamPlayer3D PlaySfx(string path, Node node)
	{
		AudioStreamPlayer3D audioStreamPlayer = new AudioStreamPlayer3D();
		audioStreamPlayer.Stream = ResourceLoader.Load<AudioStream>(path);
		audioStreamPlayer.Bus = "SFX";
		node.AddChild(audioStreamPlayer);
		audioStreamPlayer.Play();
		audioStreamPlayer.Finished += () => audioStreamPlayer.QueueFree();
		return audioStreamPlayer;
	}
	public static AudioStreamPlayer PlaySfx(string path, SceneTree tree)
	{
		AudioStreamPlayer audioStreamPlayer = new AudioStreamPlayer();
		audioStreamPlayer.Stream = ResourceLoader.Load<AudioStream>(path);
		audioStreamPlayer.Bus = "SFX";
		tree.CurrentScene.AddChild(audioStreamPlayer);
		audioStreamPlayer.Play();
		audioStreamPlayer.Finished += () => audioStreamPlayer.QueueFree();
		return audioStreamPlayer;
	}
	public static void PlayMusic(string path, SceneTree tree)
	{
		if(musicPlayer == null)
		{
			musicPlayer = new AudioStreamPlayer();
			musicPlayer.Bus = "Music";
			musicPlayer.ProcessMode = Node.ProcessModeEnum.Always;
			tree.Root.CallDeferred(Node.MethodName.AddChild, musicPlayer);
		}
		musicPlayer.Stream = ResourceLoader.Load<AudioStream>(path);
		musicPlayer.CallDeferred(AudioStreamPlayer.MethodName.Play);
	}
}
