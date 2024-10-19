using Godot;
using System;

public partial class Spikes : Area3D
{
	[Export] int damage = 50;
	[Export] float variance = .1f;
	[Export] Timer spikeTimer;
	bool extended = true;
	AnimationPlayer animationPlayer;
	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("Spikes2/AnimationPlayer");
		spikeTimer.WaitTime = (float)GD.RandRange(spikeTimer.WaitTime - variance, spikeTimer.WaitTime + variance);
		spikeTimer.Start();
	}
	public void TimerTimeout()
	{
		extended = !extended;
		if (extended)
		{
			animationPlayer.Play("SpikesExtend");
			CollisionMask = 2;
		}
		else
		{
			animationPlayer.Play("SpikesRetract");
			CollisionMask = 0;
		}
	}
	public void OnBodyEntered(Node body)
	{
		if (body is CharacterController)
		{
			CharacterController player = body as CharacterController;
			player.TakeDamage(damage);
		}
	}
}
