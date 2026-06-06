using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace Solitaire
{
	[Tool]
	public partial class Card : SuitedBase, IDropSpot
	{
		[Signal] public delegate void OnValueChangedEventHandler(int value);
		[Export] public double CardReturnAnimTime;
		[Export] private Sprite2D FlippedOverSprite;
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

		[ExportCategory("Drop Spot Settings")]
		[Export(PropertyHint.Range, "1,13,")]
		public int Value { 
			get => _value;
			set
			{
				_value = value;
				Callable.From(OnValueChangedInternal).CallDeferred();
			}
		}

		public bool IsDraggable { get; set; } = true;
        public Zone Zone { get; set; } = Zone.NONE;
		public IPile PileParent { get; set; }
		private int _value;
		private bool _held;
		private bool _isFlippedOver;
		private Vector2 _heldOffset;
		private Vector2 _childOffset;
		private Vector2 _heldStartPosition;
		private int _heldStartZIndex;

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
					bool revertMove = true;
					Array<Dictionary> objectsUnderClick = GetWorld2D().DirectSpaceState.IntersectPoint(
						new PhysicsPointQueryParameters2D()
						{
							Position = GetViewport().GetMousePosition(),
							CollideWithAreas = true,
							Exclude = [GetRid()]
						}
					);

					foreach(Dictionary obj in objectsUnderClick)
					{
						if (obj.TryGetValue("collider", out Variant vCollider))
						{
							if (vCollider.AsGodotObject() is Area2D area && 
								area is IDropSpot dropSpot)
							{
								if (dropSpot.TryDrop(this, out IPile destinationPile))
								{
									revertMove = false;
									GlobalMoveSystem.Move move = new GlobalMoveSystem.Move()
									{
										Source = PileParent,
										Destination = destinationPile,
										ReverseOnUndo = false,
										// TODO: What about cards under this if we are moving play piles around...
										CardList = new List<Card>() { this }	
									};
									GlobalMoveSystem.Instance.ExecuteMove(move);
									break;
								}
							}
						}
					}

					if (revertMove)
					{
						List<TweenInfo> revertTweenInfo =
                        [
                            TweenInfo.CreateTweenInfo(this, "z_index", 0, CardReturnAnimTime, SolitaireGlobals.MoveZIndex, _heldStartZIndex),
                            TweenInfo.CreateTweenInfo(this, "position", CardReturnAnimTime, 0, Position, _heldStartPosition),
                        ];

						GlobalMoveSystem.MoveAnimation animation = new GlobalMoveSystem.MoveAnimation(revertTweenInfo, null);
						animation.PlayFromStart();
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
					if (IsDraggable)
					{
						// TODO: What about cards under this if we are moving play piles around...
						_held = true;
						_heldStartPosition = Position;
						_heldStartZIndex = ZIndex;
						_heldOffset = GlobalPosition - mouseButtonEvent.GlobalPosition;
						ZIndex = SolitaireGlobals.MoveZIndex;
						GetViewport().SetInputAsHandled();
					}
				}
			}
		}

		public bool TryDrop(Card droppedCard, out IPile pile)
		{
			bool success = false;
			pile = null;
			if (PileParent != null && PileParent is IDropSpot dropSpot)
			{
				success = dropSpot.TryDrop(droppedCard, out pile);
			}

            return success;
        }

        private void OnValueChangedInternal()
		{
			EmitSignal(SignalName.OnValueChanged, _value);
		}

        public override string ToString()
        {
            return $"Value: {Value} | Suit: {Suit}";
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