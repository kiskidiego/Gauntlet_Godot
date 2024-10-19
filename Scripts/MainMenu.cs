using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] Control mainMenu;
	[Export] Control optionsMenu;
	[Export] Slider sfxSlider;
	[Export] Slider musicSlider;
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		musicSlider.Value = Audio.musicVolume;
		sfxSlider.Value = Audio.sfxVolume;
		Seed.seed = ulong.MaxValue;
		Audio.PlayMusic("res://Audio/Music/menu_theme.mp3", GetTree());
	}
	public void QuitButtonPressed()
	{
		PlaySound();
		GetTree().Quit();
	}
	public void PlayButtonPressed()
	{
		PlaySound();
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/Tutorial.tscn");
	}
	public void OptionsButtonPressed()
	{
		PlaySound();
		mainMenu.Visible = false;
		optionsMenu.Visible = true;
	}
	public void BackButtonPressed()
	{
		PlaySound();
		mainMenu.Visible = true;
		optionsMenu.Visible = false;
	}
	public void MusicVolumeChanged(float value)
	{
		Audio.musicVolume = value;
	}
	public void SFXVolumeChanged(float value)
	{
		Audio.sfxVolume = value;
	}
	public void PlaySound(bool b = false)
	{
		Audio.PlaySfx("res://Audio/SFX/menu_select.wav", GetTree());
	}
}
