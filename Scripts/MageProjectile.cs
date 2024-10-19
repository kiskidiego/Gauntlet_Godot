using Godot;
using System;

public partial class MageProjectile : Area3D
{
	[Export] float Speed = 1.0f;
	[Export] public int damage = 35;
	Vector3 movement = new Vector3(0, 0, 0);
	public override void _Ready()
	{
		movement += Basis.Z * -Speed;
		Reparent(GetTree().CurrentScene);
	}
	public override void _PhysicsProcess(double delta)
	{
		Transform = Transform.Translated(movement * (float)delta);
	}
	public void BodyHasEntered(Node body)
	{
		//GD.Print("Body has entered");
		if (body is Enemy)
		{
			(body as Enemy).TakeDamage(damage);
		}
		else if (body is Chest)
        {
			(body as Chest).TakeDamage(damage);
        }
		else if(body is Spawner)
		{
			(body as Spawner).TakeDamage(damage);
		}
        QueueFree();
	}
}
