using Godot;
using Godot.Collections;

public partial class CharacterController : CharacterBody3D
{
	[Export] float speed = 5.0f;
	[Export] float rotationSpeed = 7.5f;
	[Export] Timer damageTimer;
	[Export] Timer blinkTimer;
	[Export] Timer attackTimer;
	[Export] PackedScene bullet;
	[Export] Array<PackedScene> entities;
	[Export] TextResize healthLabel;
	[Export] TextResize scoreLabel;
	[Export] TextResize keyLabel;
	[Export] TextResize levelLabel;
	[Export] PackedScene pauseMenu;
	NodePath modelPath = "Mage";
	Node3D model = null;
	NodePath animationPlayerPath = "Mage/AnimationPlayer";
	AnimationPlayer animationPlayer;
	Basis rotationBasis = Basis.Identity;
	bool whichAttack = false;
	public int Keys { get { return keys; } set { keys = value; keyLabel.Text = value.ToString(); } }
	int keys;
	public int Health { get { return health; } set { health = value; healthLabel.Text = value.ToString(); } }
	int health = 600;
	bool canShoot = true;
	public int Score { get { return score; } set { score = value; scoreLabel.Text = value.ToString(); } }
	int score;
	int level = 1;
	public override void _Ready()
	{
		model = GetNode<Node3D>(modelPath);
		animationPlayer = GetNode<AnimationPlayer>(animationPlayerPath);
	}
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("pause"))
		{
			Control pause = pauseMenu.Instantiate() as Control;
			GetTree().CurrentScene.AddChild(pause);
		}
		if(!canShoot)
		{
			return;
		}
		if (Input.IsActionPressed("shoot_forward"))
		{
			PlayAttackAnimation();
			model.RotationDegrees = new Vector3(0, 0, 0);
			rotationBasis = model.Transform.Basis;
			Shoot();
			//GD.Print("Score: " + Score);
		}
		if (Input.IsActionPressed("shoot_left"))
		{
			PlayAttackAnimation();
			model.RotationDegrees = new Vector3(0, 90, 0);
			rotationBasis = model.Transform.Basis;
			Shoot();
		}
		if (Input.IsActionPressed("shoot_right"))
		{
			PlayAttackAnimation();
			model.RotationDegrees = new Vector3(0, -90, 0);
			rotationBasis = model.Transform.Basis;
			Shoot();
		}
		if (Input.IsActionPressed("shoot_backward"))
		{
			PlayAttackAnimation();
			model.RotationDegrees = new Vector3(0, 180, 0);
			rotationBasis = model.Transform.Basis;
			Shoot();
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		if(Velocity.Length() > 0.1f)
		{
			animationPlayer.Play("PlayerWalk");
		}
		else
		{
			if (animationPlayer.AssignedAnimation == "PlayerWalk")
				animationPlayer.Play("PlayerIdle");
		}
		model.Basis = Basis.Identity;
		Vector3 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		Vector3 direction = (model.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;

			Basis target = model.Transform.LookingAt(model.Transform.Origin + direction, Vector3.Up).Basis;
			rotationBasis = rotationBasis.Slerp(target, (float)delta * rotationSpeed);
			rotationBasis = rotationBasis.Orthonormalized(); // Keep the basis orthonormalized to avoid scaling issues.
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}
		model.Basis = rotationBasis;
		Velocity = velocity;
		MoveAndSlide();

		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			KinematicCollision3D collision = GetSlideCollision(i);
			if (collision.GetCollider() is Door && !(collision.GetCollider() as Door).open && Keys > 0)
			{
				Audio.PlaySfx("res://Audio/SFX/door_open.wav", this);
				(collision.GetCollider() as Door).Open();
				Keys--;
			}
		}
	}
	public void AnimationFinished(StringName anim)
	{
		animationPlayer.Play("PlayerIdle");
	}
	void PlayAttackAnimation()
	{
		Audio.PlaySfx("res://Audio/SFX/player_attack.wav", this);
		if (whichAttack)
		{
			animationPlayer.Play("PlayerAttack1");
		}
		else
		{
			animationPlayer.Play("PlayerAttack2");
		}
		whichAttack = !whichAttack;
	}
	public void TakeDamage(int damage)
	{
		Audio.PlaySfx("res://Audio/SFX/hit.wav", this);
		//GD.Print("Taking damage");
		//GD.Print(Health);
		Health -= damage;
		CollisionLayer = 16;
		//GD.Print(Health);
		if (Health > 0)
		{
			model.Visible = false;
			blinkTimer.Start();
			damageTimer.Start();
		}
		else
		{
			GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/MainMenu.tscn");
		}
	}
	public void DamageTimerTimeout()
	{
		blinkTimer.Stop();
		CollisionLayer = 2;
		model.Visible = true;
	}
	public void Blink()
	{
		model.Visible = !model.Visible;
	}
	public void Shoot()
	{
		canShoot = false;
		// Create a new bullet instance and add it to the scene.
		Node3D bulletInstance = bullet.Instantiate() as Node3D;
		bulletInstance.Transform = Transform;
		bulletInstance.RotationDegrees = model.RotationDegrees;
		bulletInstance.Position += new Vector3(0, .2f, 0);
		GetTree().GetFirstNodeInGroup("maze").AddChild(bulletInstance);
		attackTimer.Start();
	}
	public void CanShoot()
	{
		canShoot = true;
	}
	public void NextLevel()
	{
		CollisionLayer = 0;
		GlobalPosition = new Vector3(0, 0, 0);
		Score += 300;
		MazeGenerator maze = GetTree().GetFirstNodeInGroup("maze") as MazeGenerator;
		Array<Node> stuff = maze.GetChildren();
		foreach(Node thing in stuff)
		{
			thing.QueueFree();
		}
		//GD.Print(maze.mazeSize * maze.mazeSize); // 25
		//GD.Print(maze.entities.Count);
		
		maze.seed++;
		maze.mazeSize += 2;
		RandomNumberGenerator random = new RandomNumberGenerator();
		random.Seed = maze.seed * (ulong)(level + 1);

		for(int i = 0; i < 5; i++)
		{
			maze.entities.Add(entities[random.RandiRange(0, entities.Count - 1)]);
			random.Seed++;
			random.Seed++;
		}

		//GD.Print(maze.mazeSize * maze.mazeSize); // 25
		//GD.Print(maze.entities.Count);

		maze.Generate();
		level++;
		levelLabel.Text = level.ToString();
	}
}
