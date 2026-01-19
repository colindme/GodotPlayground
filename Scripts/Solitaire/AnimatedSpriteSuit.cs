using Godot;

namespace Solitaire
{
	[Tool]
	public partial class AnimatedSpriteSuit : AnimatedSprite2D
	{
		[Export]
		private bool ChangeSpriteColor;

		[Export]
		public SuitedBase Parent
		{
			get => _parent;
			set
			{
				Callable c = new Callable(this, MethodName.OnSuitChanged);
				if (_parent != null)
				{
					SignalExtensions.TryDisconnect(_parent, SuitedBase.SignalName.OnSuitChanged, c);
				}

				_parent = value;
				if (_parent != null)
				{
					SignalExtensions.TryConnect(_parent, SuitedBase.SignalName.OnSuitChanged, c);
				}
			}
		}

		private SuitedBase _parent;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			if (_parent == null)
			{
				GD.PrintErr($"AnimatedSpriteSuit named {Name} does not have a parent during _Ready().");
			}
		}

		private void OnSuitChanged(Suit suit)
		{
			Frame = (int)suit;
			if (ChangeSpriteColor)
			{
				Color? color = null;
				// Use the default color if we are in editor
				if (Engine.IsEditorHint())
				{
					SuitsConfig config = GD.Load<SuitsConfig>(EditorConstants.EditorSuitsConfig);
					if (config.Suits.TryGetValue(suit, out Color c))
					{
						color = c;
					}
				}
				else
				{
					color = GlobalSuitsConfig.Instance.TryGetSuitColor(suit);	
				}

				if (color != null)
				{
					SelfModulate = color.Value;
				}
				else
				{
					GD.PrintErr($"Error trying to set suit {suit} | Color was null from TryGetSuitColor");
				}
			}
		}
	}
}