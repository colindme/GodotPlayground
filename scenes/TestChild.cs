using Godot;
using System;

public partial class TestChild : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InputEvent += OnInput;
	}
	
	private void OnInput(Node viewport, InputEvent @event, long shapeIdx) 
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
			{
				GD.Print($"{Name} was clicked");
			}
		}
	}
}
