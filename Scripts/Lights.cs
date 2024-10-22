using Godot;
using System;

public partial class Lights : Node3D
{
	[Export] float rotationSpeed = -0.0005f;
	// Called when the node enters the scene tree for the first time.


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		RotateZ(rotationSpeed);
	}
}
