using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class Card : InteractBase
{
	[Export]
	private Godot.Collections.Array<AnimatedSprite2D> _suitSprites; 

	[Export]
	public Suit Suit 
	{ 
		get => _suit; 
		set 
		{
			_suit = value;
			UpdateSuitSprite();	
		}
	}

	private Suit _suit;
	private bool _held;
	private Vector2 _heldOffset;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		InputEvent += OnInput;
		if (_suitSprites == null || _suitSprites.Count == 0)
		{
			GD.PrintErr("ERROR: SuitSprites on Card must be set");
			throw new ApplicationException("ERROR: SuitSprites on Card must be set");
		}
		// Although a bit unintuitive, update the suit sprite here in _Ready because the _suitSprites array is not ready when first initialized
		UpdateSuitSprite();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_held)
		{
			GlobalPosition = GetGlobalMousePosition() + _heldOffset;
		}
	}
 
    public override void _Input(InputEvent @event)
    {
        if (!_held) return;
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsReleased())
			{
				_held = false;
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
				GetViewport().SetInputAsHandled();
			}
		}
	}

	public void UpdateSuitSprite()
	{
		if (_suitSprites == null) return;

		foreach(AnimatedSprite2D sprite in _suitSprites)
		{
			sprite.Frame = (int)_suit;
		}
	}
}

public enum Suit
{
	HEART = 0,
	DIAMOND,
	SPADE,
	CLUB
}

public enum FaceValue
{
	ACE = 1,
	JACK = 11,
	QUEEN,
	KING
}
