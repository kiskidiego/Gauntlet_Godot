using Godot;
using System;

public partial class Door : Node3D
{
	public bool open = false;
	public void Open()
	{
		open = true;
		AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.Play("GateOpen");
	}
	public void AnimationFinished(StringName animName)
	{
		QueueFree();
	}
}
