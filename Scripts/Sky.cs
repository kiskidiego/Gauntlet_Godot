using Godot;
using System;

public partial class Sky : WorldEnvironment
{
	[Export] float rotationSpeed = 0.1f;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Environment.SkyRotation += new Vector3(rotationSpeed * (float)delta, 0, 0);
	}
}
