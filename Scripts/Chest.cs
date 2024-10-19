using Godot;
using System;

public partial class Chest : StaticBody3D
{
	[Export] public int health = 50;
	[Export] public int pointValue = 100;
	[Export] Timer blinkTimer;
	CharacterController player;
	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("player") as CharacterController;
	}
	public void BlinkTimerTimeout()
	{
		Visible = true;
	}
	public void TakeDamage(int damage)
	{
		health -= damage;
		Visible = false;
		blinkTimer.Start();
		if (health <= 0)
		{
			AudioStreamPlayer3D audio = Audio.PlaySfx("res://Audio/SFX/destroyed.wav", this);
			audio.Reparent(GetTree().CurrentScene);
			player.Score += pointValue;
			QueueFree();
		}
	}
}
