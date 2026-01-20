using Godot;
using Godot.Collections;
using System;

namespace Solitaire
{
	[Tool]
	public partial class Card : SuitedBase, IDropSpot
	{
		[Signal] public delegate void OnValueChangedEventHandler(int value);
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

		[Export] private int HeldZIndex = 10;
		[ExportCategory("Drop Spot Settings")]
		[Export] public Vector2 ChildOffset { get => _childOffset; set => _childOffset = value; }
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
        public Zone Zone { get; set; } = Zone.NONE;
		public IPile PileParent { get; set; }
		public Action CancelMove;
		private int _value;
		private bool _held;
		private bool _isFlippedOver;
		private Vector2 _heldOffset;
		private Vector2 _childOffset;
		private GlobalMoveSystem.Move _tempMove;

		public override void _Ready()
		{
			InputEvent += OnInput;
			_tempMove = null;
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
								if (dropSpot.TryDrop(this))
								{
									revertMove = false;
									break;
								}
							}
						}
					}

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
					if (IsDraggable)
					{
						_held = true;
						_heldOffset = GlobalPosition - mouseButtonEvent.GlobalPosition;
						ZIndex = HeldZIndex;
						GetViewport().SetInputAsHandled();
					}
				}
			}
		}

		public bool TryDrop(Card droppedCard)
		{
			bool success = false;
            switch (Zone)
            {
                case Zone.NONE:
                case Zone.SCRY:
					GD.Print($"Tried to drop on card with zone {Zone} | Returning false");
                    break;
                case Zone.PLAY:
					// 
                    break;
                case Zone.FINAL:
					// Implement final first
                    break;					
            }

            return success;
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