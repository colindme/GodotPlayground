using Godot;
using System;

public partial class Testing : Node2D
{
	[Export] private Area2D area2D;
	[Export] private Sprite2D sprite2D;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		area2D.Connect(Area2D.SignalName.InputEvent, new Callable(this, MethodName.OnArea2DInputEvent));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//GD.Print("Glorp");
	}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("test_z"))
		{
			Visible = !Visible;

			GetViewport().SetInputAsHandled();
		}
    }

	private void OnArea2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
    {
        // Check if the event is a mouse button press
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
            {
                GD.Print("Area2D was left-clicked!");
                // Optional: Mark the event as handled so it doesn't propagate further
                GetViewport().SetInputAsHandled(); 
            }
        }
    }
}
