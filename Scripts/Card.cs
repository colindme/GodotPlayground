using Godot;
using System;
using System.ComponentModel.DataAnnotations;

[Tool]
public partial class Card : SuitedBase, IDropSpot
{
	[Signal]
	public delegate void OnValueChangedEventHandler(int value);

	[Export]
	private Sprite2D FlippedOverSprite;

	[Export]
	public bool IsFlippedOver
	{
		get => _isFlippedOver;
		set
		{
			_isFlippedOver = value;
			if (FlippedOverSprite != null)
			{
				FlippedOverSprite.Visible = _isFlippedOver;
			}
		}
	}

	[Export]
	private int HeldZIndex = 10;

	[ExportCategory("Drop Spot Settings")]
	[Export]
    public Vector2 ChildOffset { get => _childOffset; set => _childOffset = value; }

	[Export(PropertyHint.Range, "1,13,")]
	public int Value { 
		get => _value;
		set
		{
			_value = value;
			// TODO: Consider making the deferment an editor only functionality
			Callable.From(OnValueChangedInternal).CallDeferred();
		}
	}

	private int _value;
	private bool _held;
	private bool _isFlippedOver;
	private Vector2 _heldOffset;
	private Vector2 _childOffset;

	double test = 0;

	public override void _Ready()
    {
		InputEvent += OnInput;
    }

	public override void _Process(double delta)
	{
		// Update held location
		if (_held)
		{
			GlobalPosition = GetGlobalMousePosition() + _heldOffset;
		}

		if(!Engine.IsEditorHint())
		{
			//test += delta;
			if (test > 1)
			{
				test = 0;
				Suit = (Suit)((int)(_suit + 1) % 4);
				Value = Math.Max((_value + 1) % 14, 1);
			}
		}
	}
 
	// Drop input is handled here since OnInput is only called if the mouse is hovering over the collider (which sometimes isn't the case if the collider is lagging 1 frame behind)
    public override void _Input(InputEvent @event)
    {
        if (!_held) return;
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsReleased())
			{
				_held = false;
				Godot.Collections.Array<Area2D> overlapAreas = GetOverlappingAreas();
				bool revertMove = true;
				if (overlapAreas.Count != 0)
				{
					foreach (Area2D area in overlapAreas)
					{
						if (area is IDropSpot dropSpot)
						{
							dropSpot.TryDrop(this);
						}
					}
				}
				if (revertMove)
				{
					
				}
				GetViewport().SetInputAsHandled();
			}
		}
    }

	private void OnInput(Node viewport, InputEvent @event, long shapeIdx) {
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
			{
				_held = true;
				_heldOffset = GlobalPosition - mouseButtonEvent.GlobalPosition;
				ZIndex = HeldZIndex;
				GetViewport().SetInputAsHandled();
			}
		}
	}

    public void TryDrop(Card droppedCard)
    {
        droppedCard.Reparent(this);
		droppedCard.Position = _childOffset;
		// Reset the ZIndex from _heldZIndex
		droppedCard.ZIndex = 1;
		GD.Print(droppedCard.Position);
    }

	private void OnValueChangedInternal()
	{
		EmitSignal(SignalName.OnValueChanged, _value);
	}
}

public enum FaceValue
{
	ACE = 1,
	JACK = 11,
	QUEEN,
	KING
}
