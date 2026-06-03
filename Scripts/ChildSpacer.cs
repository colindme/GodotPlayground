using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ChildSpacer : Node2D
{
	[Export] public Vector2 InitialOffset { get; private set; }
	[Export] public Vector2 ChildOffset { get; private set; }
	[Export] public Godot.Collections.Array<Node2D> ExceptionNodes { get; private set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SignalExtensions.TryConnect(this, SignalName.ChildOrderChanged, Callable.From(OnChildOrderChanged));
	}

	private void OnChildOrderChanged()
	{
		GD.Print("Child order changed");
		Callable c = Callable.From((Node2D node, int i) => node.Position = InitialOffset + (ChildOffset * i));

		int childOffsetCount = 0;
		foreach(Node2D node in GetChildren())
		{
			GD.Print(node.Name);
			if (ExceptionNodes != null && ExceptionNodes.Contains(node))
			{
				continue;
			}

			c.CallDeferred(node, childOffsetCount);
			childOffsetCount++;
		}
	}
}
