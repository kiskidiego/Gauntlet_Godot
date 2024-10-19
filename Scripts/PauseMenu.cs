using Godot;
using System;

public partial class PauseMenu : Control
{
	[Export] Control pauseMenu;
	[Export] Control optionsMenu;
	[Export] Label seedLabel;
	[Export] Slider sfxSlider;
	[Export] Slider musicSlider;
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		musicSlider.Value = Audio.musicVolume;
		sfxSlider.Value = Audio.sfxVolume;
		PlaySound();
		GetTree().Paused = true;
		GD.Print(Seed.seed);
		seedLabel.Text = "Seed: " + Convert.ToString((long)Seed.seed, 16);
	}
	public override void _ExitTree()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
	}
	public void OnResume()
	{
		PlaySound();
		QueueFree();
	}
	public void OptionsButtonPressed()
	{
		PlaySound();
		pauseMenu.Visible = false;
		optionsMenu.Visible = true;
	}
	public void MusicVolumeChanged(float value)
	{
		Audio.musicVolume = value;
	}
	public void SFXVolumeChanged(float value)
	{
		Audio.sfxVolume = value;
	}
	public void BackButtonPressed()
	{
		PlaySound();
		pauseMenu.Visible = true;
		optionsMenu.Visible = false;
	}
	public void OnQuitToMain()
	{
		PlaySound();
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/MainMenu.tscn");
	}
	public void PlaySound(bool b = false)
	{
		Audio.PlaySfx("res://Audio/SFX/menu_select.wav", GetTree());
	}
}
