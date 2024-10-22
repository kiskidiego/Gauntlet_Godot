using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class MazeGenerator : Node3D
{
	[Export] public Array<PackedScene> entities;
	[Export] PackedScene wall;
	[Export] PackedScene floor;
	[Export] PackedScene pillar;
	[Export] PackedScene stairs;
	[Export] PackedScene door;
	[Export] PackedScene key;
	public ulong seed = ulong.MaxValue;
	[Export] public int mazeSize = 20;
	[Export] float doorProbability = 0.05f;
	float keyProbability = 0.00001f;
	Signal mapReadyForBaking;
	Signal mapBaked;
	Node3D player;
	NavigationNode[,] navigationGraph;
	class NavigationNode
	{
		public Vector3 Position;
		public List<NavigationNode> Neighbors = new List<NavigationNode>();
		public NavigationNode(Vector3 position)
		{
			Position = position;
		}
	}
	class Wall
	{
		public GraphNode node1;
		public GraphNode node2;
		public Node3D wall;
		public Wall(GraphNode node1, GraphNode node2, Node3D wall)
		{
			this.node1 = node1;
			this.node2 = node2;
			this.wall = wall;
		}
	}
	class GraphNode
	{
		public Vector3 Position;
		public bool inMaze;
		public List<GraphNode> Neighbors = new List<GraphNode>();
		public Wall northWall;
		public Wall southWall;
		public Wall eastWall;
		public Wall westWall;
		public Node3D floor;
		public GraphNode cameFrom;
		public GraphNode(Vector3 position)
		{
			Position = position;
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		Audio.PlayMusic("res://Audio/Music/main_theme.mp3", GetTree());
		player = GetTree().GetFirstNodeInGroup("player") as Node3D;
		Generate();
	}
	public void Generate()
	{
		seed = Seed.seed;
		if (seed == ulong.MaxValue)
		{
			seed = (ulong)DateTime.Now.Ticks;
			Seed.seed = seed;
		}
		GraphNode[,] graph = CreateGraph(mazeSize, 1);
		GenerateAllWalls(graph, wall, pillar);
		AddFloors(graph, floor);
		GenerateMaze(graph, seed);
		RestructureGraph(graph);
		SetPathTree(graph[0, 0], graph);
		////GD.Print("Path set");
		bool[,] spawned = new bool[mazeSize, mazeSize];
		GeneraterDoorsAndKeys(graph, doorProbability, keyProbability, door, key, seed, spawned);
		navigationGraph = SetUpNavigationGraph(graph);
		SpawnEntities(graph, entities, seed, spawned);
		(GetTree().GetFirstNodeInGroup("player") as CharacterBody3D).CollisionLayer = 2;
	}
	GraphNode[,] CreateGraph(int nodeSize, float realSize)
	{
		GraphNode[,] graph = new GraphNode[nodeSize, nodeSize];
		for (int i = 0; i < nodeSize; i++)
		{
			for (int j = 0; j < nodeSize; j++)
			{
				graph[i, j] = new GraphNode(new Vector3(i * realSize, 0, j * realSize));
			}
		}
		for (int i = 0; i < nodeSize; i++)
		{
			for (int j = 0; j < nodeSize; j++)
			{
				if (i > 0)
				{
					graph[i, j].Neighbors.Add(graph[i - 1, j]);
				}
				if (i < nodeSize - 1)
				{
					graph[i, j].Neighbors.Add(graph[i + 1, j]);
				}
				if (j > 0)
				{
					graph[i, j].Neighbors.Add(graph[i, j - 1]);
				}
				if (j < nodeSize - 1)
				{
					graph[i, j].Neighbors.Add(graph[i, j + 1]);
				}
			}
		}
		return graph;
	}
	void AddFloors(GraphNode[,] graph, PackedScene floor)
	{
		foreach (GraphNode node in graph)
		{
			if (node.floor != null)
			{
				continue;
			}
			Node3D floorInstance = floor.Instantiate() as Node3D;
			floorInstance.Position = node.Position;
			AddChild(floorInstance);
			node.floor = floorInstance;
			//floorInstance.Owner = GetParent();
		}
		GraphNode stairsNode = graph[graph.GetLength(0) - 1, graph.GetLength(1) - 1];
		stairsNode.floor.QueueFree();
		Node3D stairsInstance = stairs.Instantiate() as Node3D;
		stairsInstance.Position = stairsNode.Position;
		AddChild(stairsInstance);
		stairsNode.floor = stairsInstance;
	}

	void GenerateAllWalls(GraphNode[,] graph, PackedScene wall, PackedScene pillar)
	{
		foreach (GraphNode node in graph)
		{
			foreach (GraphNode neighbor in node.Neighbors)
			{
				if (node.Position.X < neighbor.Position.X)
				{
					if (node.eastWall != null)
					{
						continue;
					}

					node.eastWall = new Wall(node, neighbor, wall.Instantiate() as Node3D);
					node.eastWall.wall.Position = (node.Position + neighbor.Position) / 2f;
					node.eastWall.wall.RotationDegrees = new Vector3(0, 90, 0);
					neighbor.westWall = node.eastWall;
					AddChild(node.eastWall.wall);
					//node.eastWall.Owner = GetParent();
				}
				else if (node.Position.Z < neighbor.Position.Z)
				{
					if (node.northWall != null)
					{
						continue;
					}

					node.northWall = new Wall(node, neighbor, wall.Instantiate() as Node3D);
					node.northWall.wall.Position = (node.Position + neighbor.Position) / 2f;
					node.northWall.wall.RotationDegrees = new Vector3(0, 0, 0);
					neighbor.southWall = node.northWall;
					AddChild(node.northWall.wall);
					//node.northWall.Owner = GetParent();
				}
				else if (node.Position.X > neighbor.Position.X)
				{
					if (node.westWall != null)
					{
						continue;
					}

					node.westWall = new Wall(node, neighbor, wall.Instantiate() as Node3D);
					node.westWall.wall.Position = (node.Position + neighbor.Position) / 2f;
					node.westWall.wall.RotationDegrees = new Vector3(0, 90, 0);
					neighbor.eastWall = node.westWall;
					AddChild(node.westWall.wall);
					//node.westWall.Owner = GetParent();
				}
				else if (node.Position.Z > neighbor.Position.Z)
				{
					if (node.southWall != null)
					{
						continue;
					}

					node.southWall = new Wall(node, neighbor, wall.Instantiate() as Node3D);
					node.southWall.wall.Position = (node.Position + neighbor.Position) / 2f;
					node.southWall.wall.RotationDegrees = new Vector3(0, 0, 0);
					neighbor.northWall = node.southWall;
					AddChild(node.southWall.wall);
					//node.southWall.Owner = GetParent();
				}
			}
		}
		for(int i = 0; i < graph.GetLength(0); i++)
		{
			graph[i, 0].southWall = new Wall(graph[i, 0], null, wall.Instantiate() as Node3D);
			graph[i, 0].southWall.wall.Position = graph[i, 0].Position + new Vector3(0, 0, -0.5f);
			graph[i, 0].southWall.wall.RotationDegrees = new Vector3(0, 0, 0);
			AddChild(graph[i, 0].southWall.wall);
			graph[i, graph.GetLength(1) - 1].northWall = new Wall(graph[i, graph.GetLength(1) - 1], null, wall.Instantiate() as Node3D);
			graph[i, graph.GetLength(1) - 1].northWall.wall.Position = graph[i, graph.GetLength(1) - 1].Position + new Vector3(0, 0, 0.5f);
			graph[i, graph.GetLength(1) - 1].northWall.wall.RotationDegrees = new Vector3(0, 0, 0);
			AddChild(graph[i, graph.GetLength(1) - 1].northWall.wall);
		}
		for (int i = 0; i < graph.GetLength(1); i++)
		{
			if (graph[0, i].westWall != null)
			{
				////GD.Print("West wall already exists");
			}
			graph[0, i].westWall = new Wall(graph[0, i], null, wall.Instantiate() as Node3D);
			graph[0, i].westWall.wall.Position = graph[0, i].Position + new Vector3(-0.5f, 0, 0);
			graph[0, i].westWall.wall.RotationDegrees = new Vector3(0, 90, 0);
			AddChild(graph[0, i].westWall.wall);
			graph[graph.GetLength(0) - 1, i].eastWall = new Wall(graph[graph.GetLength(0) - 1, i], null, wall.Instantiate() as Node3D);
			graph[graph.GetLength(0) - 1, i].eastWall.wall.Position = graph[graph.GetLength(0) - 1, i].Position + new Vector3(0.5f, 0, 0);
			graph[graph.GetLength(0) - 1, i].eastWall.wall.RotationDegrees = new Vector3(0, 90, 0);
			AddChild(graph[graph.GetLength(0) - 1, i].eastWall.wall);
		}

		graph[graph.GetLength(1) - 1, graph.GetLength(1) - 1].northWall.wall.QueueFree();
		graph[graph.GetLength(1) - 1, graph.GetLength(1) - 1].northWall = null;
		
		for(int i = 0; i <= graph.GetLength(0); i++)
		{
			for (int j = 0; j <= graph.GetLength(1); j++)
			{
				Node3D pillarInstance = pillar.Instantiate() as Node3D;
				pillarInstance.Position = new Vector3(i - 0.5f, 0, j - 0.5f);
				AddChild(pillarInstance);
			}
		}
	}
	void GenerateMaze(GraphNode[,] graph, ulong seed = ulong.MaxValue)
	{
		foreach (GraphNode node in graph)
		{
			node.inMaze = false;
		}
		RandomNumberGenerator random = new RandomNumberGenerator();
		random.Seed = seed;
		List<GraphNode> frontier = new List<GraphNode>();
		GraphNode start = graph[random.RandiRange(0, graph.GetLength(0) - 1), random.RandiRange(0, graph.GetLength(1) - 1)];
		start.inMaze = true;
		foreach (GraphNode neighbor in start.Neighbors)
		{
			frontier.Add(neighbor);
		}
		while (frontier.Count > 0)
		{
			// Randomly select a node from the frontier and add it to the maze
			GraphNode current = frontier[random.RandiRange(0, frontier.Count - 1)];
			frontier.Remove(current);
			if (current.inMaze)
			{
				continue;
			}
			current.inMaze = true;
			bool connected = false;

			// Randomize the order of the neighbors
			int[] ints = { 0, 1, 2, 3 };
			for (int i = 0; i < ints.Length; i++)
			{
				int j = random.RandiRange(i, ints.Length - 1);
				int temp = ints[i];
				ints[i] = ints[j];
				ints[j] = temp;
			}

			// Neighbor examination loop
			for (int i = 0; i < 4; i++)
			{
				if (ints[i] >= current.Neighbors.Count)
				{
					continue;
				}
				if (current.Neighbors[ints[i]] == null)
				{
					continue;
				}

				// If the neighbor is not in the maze, add it to the frontier
				if (!current.Neighbors[ints[i]].inMaze)
				{
					frontier.Add(current.Neighbors[ints[i]]);
				}

				// If the neighbor is in the maze, and the current node is not connected to it, connect them
				else if (!connected)
				{
					connected = true;
					// Create a path between current and current.Neighbors[ints[i]]
					if (current.Position.X < current.Neighbors[ints[i]].Position.X && current.eastWall != null)
					{
						current.eastWall.wall.QueueFree();
						current.Neighbors[ints[i]].westWall = null;
						current.eastWall = null;
					}
					else if (current.Position.X > current.Neighbors[ints[i]].Position.X && current.westWall != null)
					{
						current.westWall.wall.QueueFree();
						current.Neighbors[ints[i]].eastWall = null;
						current.westWall = null;
					}
					else if (current.Position.Z < current.Neighbors[ints[i]].Position.Z && current.northWall != null)
					{
						current.northWall.wall.QueueFree();
						current.Neighbors[ints[i]].southWall = null;
						current.northWall = null;
					}
					else if (current.Position.Z > current.Neighbors[ints[i]].Position.Z && current.southWall != null)
					{
						current.southWall.wall.QueueFree();
						current.Neighbors[ints[i]].northWall = null;
						current.southWall = null;
					}
				}
			}
		}
	}
	void RestructureGraph(GraphNode[,] graph)
	{
		foreach (GraphNode node in graph)
		{
			if (node.northWall != null && node.northWall.node1 != null && node.northWall.node2 != null)
			{
				node.northWall.node1.Neighbors.Remove(node.northWall.node2);
				node.northWall.node2.Neighbors.Remove(node.northWall.node1);
			}
			if (node.southWall != null && node.southWall.node1 != null && node.southWall.node2 != null)
			{
				node.southWall.node1.Neighbors.Remove(node.southWall.node2);
				node.southWall.node2.Neighbors.Remove(node.southWall.node1);
			}
			if (node.eastWall != null && node.eastWall.node1 != null && node.eastWall.node2 != null)
			{	
				node.eastWall.node1.Neighbors.Remove(node.eastWall.node2);
				node.eastWall.node2.Neighbors.Remove(node.eastWall.node1);
			}
			if (node.westWall != null && node.westWall.node1 != null && node.westWall.node2 != null)
			{
				node.westWall.node1.Neighbors.Remove(node.westWall.node2);
				node.westWall.node2.Neighbors.Remove(node.westWall.node1);
			}
		}
	}
	void SetPathTree(GraphNode start, GraphNode[,] graph)
	{
		//////GD.Print(start.Neighbors.Count);
		List<GraphNode> openSet = new List<GraphNode>();
		openSet.Add(start);
		while (openSet.Count > 0)
		{
			GraphNode current = openSet[0];
			openSet.RemoveAt(0);
			foreach (GraphNode neighbor in current.Neighbors)
			{
				////GD.Print("Neighbor");
				if (neighbor.cameFrom != null)
				{
					continue;
				}
				neighbor.cameFrom = current;
				if (!openSet.Contains(neighbor))
				{
					openSet.Add(neighbor);
					////GD.Print(openSet.Count);
				}
			}
		}
		graph[0, 0].cameFrom = graph[0, 0];
	}
	void GeneraterDoorsAndKeys(GraphNode[,] graph, float doorProbability, float keyProbability, PackedScene door, PackedScene key, ulong seed, bool[,] spawned)
	{
		GraphNode currentNode = graph[graph.GetLength(0) - 1, graph.GetLength(1) - 1];
		RandomNumberGenerator random = new RandomNumberGenerator();
		random.Seed = seed;
		while (currentNode.Position.X > graph.GetLength(0)/4 && currentNode.Position.Z > graph.GetLength(1)/4)
		{
			random.Seed++;
			if (random.RandfRange(0, 1) > doorProbability)
			{
				currentNode = currentNode.cameFrom;
				continue;
			}
			if (currentNode.Position.X < currentNode.cameFrom.Position.X)
			{
				currentNode.eastWall = new Wall(currentNode, currentNode.cameFrom, door.Instantiate() as Node3D);
				currentNode.eastWall.wall.Position = (currentNode.Position + currentNode.cameFrom.Position) / 2f;
				currentNode.eastWall.wall.RotationDegrees = new Vector3(0, 90, 0);
				currentNode.cameFrom.westWall = currentNode.eastWall;
				AddChild(currentNode.eastWall.wall);
			}
			else if (currentNode.Position.X > currentNode.cameFrom.Position.X)
			{
				currentNode.westWall = new Wall(currentNode, currentNode.cameFrom, door.Instantiate() as Node3D);
				currentNode.westWall.wall.Position = (currentNode.Position + currentNode.cameFrom.Position) / 2f;
				currentNode.westWall.wall.RotationDegrees = new Vector3(0, 90, 0);
				currentNode.cameFrom.eastWall = currentNode.westWall;
				AddChild(currentNode.westWall.wall);
			}
			else if (currentNode.Position.Z < currentNode.cameFrom.Position.Z)
			{
				currentNode.northWall = new Wall(currentNode, currentNode.cameFrom, door.Instantiate() as Node3D);
				currentNode.northWall.wall.Position = (currentNode.Position + currentNode.cameFrom.Position) / 2f;
				currentNode.northWall.wall.RotationDegrees = new Vector3(0, 0, 0);
				currentNode.cameFrom.southWall = currentNode.northWall;
				AddChild(currentNode.northWall.wall);
			}
			else if (currentNode.Position.Z > currentNode.cameFrom.Position.Z)
			{
				currentNode.southWall = new Wall(currentNode, currentNode.cameFrom, door.Instantiate() as Node3D);
				currentNode.southWall.wall.Position = (currentNode.Position + currentNode.cameFrom.Position) / 2f;
				currentNode.southWall.wall.RotationDegrees = new Vector3(0, 0, 0);
				currentNode.cameFrom.northWall = currentNode.southWall;
				AddChild(currentNode.southWall.wall);
			}
			GraphNode pastNode = currentNode;
			GraphNode keyNode = currentNode.cameFrom;
			int steps = 0;
			while (true)
			{
				random.Seed++;
				GraphNode neighbour = keyNode.Neighbors[random.RandiRange(0, keyNode.Neighbors.Count - 1)];
				if (neighbour != pastNode)
				{
					steps++;
					pastNode = keyNode;
					keyNode = neighbour;
				}
				if (random.RandfRange(0, 1) < keyProbability || keyNode.Neighbors.Count == 1)
				{
					if(steps < graph.GetLength(0) / 4 || spawned[(int)keyNode.Position[0], (int)keyNode.Position[1]])
					{
						steps = 0;
						pastNode = currentNode;
						keyNode = currentNode.cameFrom;
						continue;
					}
					Node3D keySpawn = key.Instantiate() as Node3D;
					keySpawn.Position = keyNode.Position;
					spawned[(int)keyNode.Position[0], (int)keyNode.Position[1]] = true;
					AddChild(keySpawn);
					break;
				}
			}
			currentNode = currentNode.cameFrom;
		}
	}
	NavigationNode[,] SetUpNavigationGraph(GraphNode[,] graph)
	{
		NavigationNode[,] navigationGraph = new NavigationNode[graph.GetLength(0), graph.GetLength(1)];
		foreach(GraphNode node in graph)
		{
			navigationGraph[(int)node.Position.X, (int)node.Position.Z] = new NavigationNode(node.Position);
		}
		foreach (GraphNode node in graph)
		{
			foreach (GraphNode neighbor in node.Neighbors)
			{
				if (neighbor == null)
				{
					continue;
				}
				navigationGraph[(int)node.Position.X, (int)node.Position.Z].Neighbors.Add(navigationGraph[(int)neighbor.Position.X, (int)neighbor.Position.Z]);
			}
		}
		return navigationGraph;
	}
	void SpawnEntities(GraphNode[,] graph, Array<PackedScene> entities, ulong seed, bool[,] spawned)
	{
		RandomNumberGenerator random = new RandomNumberGenerator();
		random.Seed = seed;
		foreach (PackedScene entity in entities)
		{
			while (true)
			{
				int x = random.RandiRange(0, graph.GetLength(0) - 1);
				int z = random.RandiRange(0, graph.GetLength(1) - 1);
				if (spawned[x, z] || (x < graph.GetLength(0) / 5 && z < graph.GetLength(1) / 5) || (x == graph.GetLength(0) - 1 && z == graph.GetLength(1) - 1))
				{
					continue;
				}
				Node3D spawnedEntity = entity.Instantiate() as Node3D;
				spawnedEntity.Position = graph[x, z].Position;
				AddChild(spawnedEntity);
				spawned[x, z] = true;
				break;
			}
		}
	}
	public Vector3 GetNextPathPosition(Vector3 currentPosition, Vector3 targetPosition)
	{
		NavigationNode current = navigationGraph[Mathf.RoundToInt(currentPosition.X), Mathf.RoundToInt(currentPosition.Z)];
		NavigationNode target = navigationGraph[Mathf.RoundToInt(targetPosition.X), Mathf.RoundToInt(targetPosition.Z)];
		foreach(NavigationNode neighbor in current.Neighbors)
		{
			if (neighbor == null)
			{
				continue;
			}
			if (neighbor == target)
			{
				return neighbor.Position;
			}
		}
		NavigationNode next = GetNextNodeInPath(current, target);
		////GD.Print(next.Position);
		return next.Position;
	}
	NavigationNode GetNextNodeInPath(NavigationNode current, NavigationNode target, bool[,] checkedNodes)
	{
		if(current == null)
		{
			return null;
		}
		if(current == target)
		{
			return current;
		}
		foreach(NavigationNode neighbor in current.Neighbors)
		{
			if (neighbor == null)
			{
				continue;
			}
			if (checkedNodes[(int)neighbor.Position.X, (int)neighbor.Position.Z])
			{
				continue;
			}
			checkedNodes[(int)neighbor.Position.X, (int)neighbor.Position.Z] = true;
			NavigationNode maybe = GetNextNodeInPath(neighbor, target, checkedNodes);
			if (maybe != null)
			{
				return current;
			}
		}
		return null;
	}
	NavigationNode GetNextNodeInPath(NavigationNode current, NavigationNode target)
	{
		bool[,] checkedNodes = new bool[navigationGraph.GetLength(0), navigationGraph.GetLength(1)];
		checkedNodes[(int)current.Position.X, (int)current.Position.Z] = true;
		foreach (NavigationNode neighbor in current.Neighbors)
		{
			if (neighbor == null)
			{
				continue;
			}
			if (neighbor == target)
			{
				return current;
			}
			NavigationNode maybe = GetNextNodeInPath(neighbor, target, checkedNodes);
			if (maybe != null)
			{
				return maybe;
			}
		}
		return current;
	}
}
