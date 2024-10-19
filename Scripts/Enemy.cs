using Godot;
using System;

public partial class Enemy : CharacterBody3D
{
	[Export] float Speed = 5.0f;
	[Export] float rotationSpeed = 7.5f;
	[Export] Timer blinkTimer;
	[Export] int pointValue = 75;
	NodePath modelPath = "Ghost";
	Node3D model;
	NodePath animationPlayerPath = "Ghost/AnimationPlayer";
	AnimationPlayer animationPlayer;
	public int health = 100;
	public int damage = 50;
	public CharacterController player;
	Basis rotationBasis = Basis.Identity;
	MazeGenerator maze;
	Vector3I previousPosition = new Vector3I(-1, -1, -1);
	double time = 0;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _Ready()
	{
		model = GetNode<Node3D>(modelPath);
		animationPlayer = GetNode<AnimationPlayer>(animationPlayerPath);
		maze = GetTree().GetFirstNodeInGroup("maze") as MazeGenerator;
		player = GetTree().GetFirstNodeInGroup("player") as CharacterController;
		Audio.PlaySfx("res://Audio/SFX/ghost_spawn.wav", this);
	}
	public override void _PhysicsProcess(double delta)
	{
		if(Velocity.Length() > 0.1f)
		{
			animationPlayer.Play("GhostWalkAction");
		}
		else
		{
			animationPlayer.Play("GhostIdleAction");
		}
		if (player == null || player.GlobalPosition == new Vector3(0, 0, 0) || player.CollisionLayer == 16)
		{
			Velocity = Vector3.Zero;
			return;
		}
		model.Basis = Basis.Identity;
		time += delta;
		MoveOnPath();
		if (Velocity != Vector3.Zero)
		{
			Basis target = Basis.LookingAt(Velocity, Vector3.Up);
			rotationBasis = rotationBasis.Slerp(target, (float)delta * rotationSpeed);
			rotationBasis = rotationBasis.Orthonormalized(); // Keep the basis orthonormalized to avoid scaling issues.
		}
		model.Basis = rotationBasis;
		if (MoveAndSlide())
		{
			for(int i = 0; i < GetSlideCollisionCount(); i++)
			{
				KinematicCollision3D collision = GetSlideCollision(i);
				if (collision.GetCollider() == player)
				{
					Audio.PlaySfx("res://Audio/SFX/ghost_attack.wav", this);
					player.TakeDamage(damage);
				}
			}
		}
	}
	void MoveOnPath()
	{
		Vector3 velocity = Velocity;
		////GD.Print("Position: " + Position);
		////GD.Print("Previous Position: " + previousPosition);
        if (Position.DistanceSquaredTo(previousPosition) > 1 || time >= .5)
        {
			time = 0;
			previousPosition = new Vector3I(Mathf.RoundToInt(Position.X), 0, Mathf.RoundToInt(Position.Z));
            velocity = GetVelocity();
        }
		Velocity = velocity;
	}
	Vector3 GetVelocity()
	{
		Vector3 nextLocation = maze.GetNextPathPosition(Position, player.Position);
		Vector3 direction = nextLocation - Position;
		direction = direction.Normalized() * Speed;
		////GD.Print("Direction: " + direction);
		return direction;
	}
	public void TakeDamage(int damage)
	{
		health -= damage;
		blinkTimer.Start();
		model.Visible = false;
		if (health <= 0)
		{
			AudioStreamPlayer3D audio = Audio.PlaySfx("res://Audio/SFX/destroyed.wav", this);
			audio.Reparent(GetTree().CurrentScene);
			player.Score += pointValue;
			QueueFree();
		}
	}

	public void Blink()
	{
		model.Visible = true;
	}
}
