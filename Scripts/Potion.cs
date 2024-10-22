using Godot;
using System;

public partial class Potion : Node3D
{
	[Export] Node3D model;
	[Export] float rotationSpeedMultiplier = 2f;
	float rotationSpeed = 0.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Random rand = new Random();
		rotationSpeed = (float)rand.NextDouble() * 2.0f - 1.0f;
		rotationSpeed *= rotationSpeedMultiplier;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		model.RotateY(rotationSpeed);
	}
	void BodyEntered(Node body)
	{
		if (body is CharacterController)
		{
			if (!IsQueuedForDeletion())
			{
				Audio.PlaySfx("res://Audio/SFX/potion_pickup.wav", body);
				CharacterController controller = body as CharacterController;
				controller.Health = Math.Min(controller.Health + 100, 600);
				QueueFree();
			}
		}
	}
}
