using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire 
{
	[Tool]
	public partial class CardMiddle : Node2D
	{
		[Export]
		private Card Parent
		{
			get => _parent;
			set
			{
				Callable c = new Callable(this, MethodName.OnValueChanged);
				if (_parent != null)
				{
					SignalExtensions.TryDisconnect(_parent, Card.SignalName.OnValueChanged, c);
				}

				_parent = value;
				if (_parent != null)
				{
					SignalExtensions.TryConnect(_parent, Card.SignalName.OnValueChanged, c);
				}
			}
		}

		[Export] private Godot.Collections.Array<AnimatedSpriteSuit> PipList;
		[Export] private SpriteFrames SpriteFramesDefault;
		[Export] private Godot.Collections.Dictionary<int, CardLayout> CardLayouts;
		// When there is a single sprite, what should we scale it by?
		[Export] private float SingleSpriteScale = 3;
		// When there is multiple sprites (e.g. 2-10), what should we scale it by?
		[Export] private float MultiSpriteScale = 1.5f;
		[Export] private bool FlipBottomPips;
		[Export] private StringName AnimationNameDefault = "suit";
		private Card _parent;
		private const int _maxPips = 10;

		private void OnValueChanged(int value)
		{
			if (CardLayouts != null)
			{
				if (CardLayouts.TryGetValue(value, out CardLayout layout))
				{
					SpriteFrames sf = layout.SpriteFramesOverride ?? SpriteFramesDefault;
					float scale = layout.PipPositions.Count == 1 ? SingleSpriteScale : MultiSpriteScale;
					Vector2 scaleVector = new Vector2(scale, scale);
					for (int i = 0; i < layout.PipPositions.Count; i++)
					{
						PipList[i].Visible = true;
						PipList[i].SpriteFrames = sf;
						PipList[i].Position = layout.PipPositions[i];
						if (FlipBottomPips && layout.PipPositions[i].Y > 0)
						{
							PipList[i].RotationDegrees = 180;
						}
						else
						{
							PipList[i].Rotation = 0;
						}
						PipList[i].Scale = scaleVector;
						PipList[i].Animation = AnimationNameDefault;
					}
					for (int i = layout.PipPositions.Count ; i < PipList.Count; i++)
					{
						PipList[i].Visible = false;
					}
				}
				else
				{
					GD.PrintErr("CardLayout not present in the dictionary");
				}
			}
		}
	}
}