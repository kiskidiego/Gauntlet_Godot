using Godot;
using System;

public partial class Tutorial : Control
{
	public void StartGame()
	{
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/GameScene.tscn");
	}
}
