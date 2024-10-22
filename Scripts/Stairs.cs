using Godot;
using System;

public partial class Stairs : Node3D
{
	public void BodyEntered(Node body)
	{
		if(body is CharacterController)
		{
			CharacterController player = body as CharacterController;
			player.NextLevel();
		}
	}
}
