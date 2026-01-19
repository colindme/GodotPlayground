using Godot;
using System;
using System.ComponentModel.DataAnnotations;

namespace Solitaire
{
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

		public bool IsDraggable { get; set; } = true;

		public Action CancelMove;

		private int _value;
		private bool _held;
		private bool _isFlippedOver;
		private Vector2 _heldOffset;
		private Vector2 _childOffset;

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
					/*
					Next up: completing the move.
					Step 1: Detecting the move type to attempt to make
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
					*/
					if (revertMove)
					{
						CancelMove();
					}
					GetViewport().SetInputAsHandled();
				}
			}
		}

		private void OnInput(Node viewport, InputEvent @event, long shapeIdx) 
		{
			if (@event is InputEventMouseButton mouseButtonEvent)
			{
				if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
				{
					GD.Print($"Card Clicked: {Value} | {Suit}");
					if (IsDraggable)
					{
						GD.Print($"Card held: {Value} | {Suit}");
						_held = true;
						_heldOffset = GlobalPosition - mouseButtonEvent.GlobalPosition;
						ZIndex = HeldZIndex;
						GetViewport().SetInputAsHandled();
					}
					else
					{
						GD.Print($"Card Not Draggable: {Value} | {Suit}");
					}
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
}