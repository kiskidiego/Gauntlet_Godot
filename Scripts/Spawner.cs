using Godot;
using System;

public partial class Spawner : Node3D
{
	[Export] float variance = 2.5f;
	[Export] int pointValue = 150;
	[Export] int health = 150;
	[Export] PackedScene enemy;
	[Export] Timer spawnTimer;
	[Export] Timer blinkTimer;
	public override void _Ready()
	{
		spawnTimer.WaitTime = (float)GD.RandRange(spawnTimer.WaitTime - variance, spawnTimer.WaitTime + variance);
		spawnTimer.Start();
	}
	public void TimerTimeout()
	{
		Node3D newEnemy = enemy.Instantiate() as Node3D;
		newEnemy.Transform = Transform;
		GetTree().GetFirstNodeInGroup("maze").AddChild(newEnemy);
	}
	public void BlinkTimerTimeout()
	{
		Visible = true;
	}
	public void TakeDamage(int damage)
	{
		//GD.Print("Taking damage");
		health -= damage;
		Visible = false;
		blinkTimer.Start();
		if (health <= 0)
		{
			AudioStreamPlayer3D audio = Audio.PlaySfx("res://Audio/SFX/destroyed.wav", this);
			audio.Reparent(GetTree().CurrentScene);
			CharacterController player = GetTree().GetFirstNodeInGroup("player") as CharacterController;
			player.Score += pointValue;
			QueueFree();
		}
	}
}
