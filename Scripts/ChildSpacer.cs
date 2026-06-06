using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class ChildSpacer : Node2D
{
	[Export] public Vector2 InitialOffset { get; private set; }
	[Export] public Godot.Collections.Array<Node2D> ExceptionNodes { get; private set; }
	[Export] public Vector2 ChildOffset 
	{ 
		get => _childOffset;
		private set
		{
			_childOffset = value;
			SetChildPositions();
		}
	}

	private Vector2 _childOffset;

	public override void _Ready()
	{
		SignalExtensions.TryConnect(this, SignalName.ChildOrderChanged, Callable.From(SetChildPositions));
	}

	private void SetChildPositions()
	{
		Callable c = Callable.From((Node2D node, int i) => node.Position = InitialOffset + (ChildOffset * i));

		int childOffsetCount = 0;
		foreach(Node2D node in GetChildren())
		{
			if (ExceptionNodes != null && ExceptionNodes.Contains(node))
			{
				continue;
			}

			if (c.Target != null)
			{
				c.CallDeferred(node, childOffsetCount);
			}
			
			childOffsetCount++;
		}
	}
}
