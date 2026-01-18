using Godot;
using System;

[Tool]
public partial class CardLabel : Label
{
	[Export]
	public Card Parent
	{
		get => _parent;
		set
		{
			Callable suitCall = new Callable(this, MethodName.OnSuitChanged);
			Callable valueCall = new Callable(this, MethodName.OnValueChanged);
			if (_parent != null)
			{
				SignalExtensions.TryDisconnect(_parent, SuitedBase.SignalName.OnSuitChanged, suitCall);
				SignalExtensions.TryDisconnect(_parent, Card.SignalName.OnValueChanged, valueCall);
			}

			_parent = value;
			if (_parent != null)
			{
				SignalExtensions.TryConnect(_parent, SuitedBase.SignalName.OnSuitChanged, suitCall);
				SignalExtensions.TryConnect(_parent, Card.SignalName.OnValueChanged, valueCall);
			}
		}
	}

	private Card _parent;

	public override void _Ready()
	{
		if (_parent == null)
		{
			GD.PrintErr($"CardLabel named {Name} does not have a parent during _Ready().");
		}
	}

	private void OnValueChanged(int value)
	{
		if (Enum.IsDefined(typeof(FaceValue), value))
		{ 
			FaceValue f = Enum.Parse<FaceValue>(value.ToString());
			Text = f.ToString().Substring(0, 1);
		}
		else
		{
			Text = value.ToString();
		}
	}

	private void OnSuitChanged(Suit suit)
	{
		Color? color = null;
		// Use the default color if we are in editor
		if (Engine.IsEditorHint())
		{
			SuitsConfig config = GD.Load<SuitsConfig>(EditorConstants.EditorSuitsConfig);
			if (config != null && config.Suits != null && config.Suits.TryGetValue(suit, out Color c))
			{
				color = c;
			}
		}
		else
		{
			color = GlobalSuitsConfig.Instance?.TryGetSuitColor(suit);	
		}

		if (color != null)
		{
			AddThemeColorOverride("font_color", color.Value);
		}
		else
		{
			GD.PrintErr($"Error trying to set suit {suit} | Color was null");
		}
	}
}