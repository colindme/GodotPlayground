using Godot;

[Tool]
public partial class GlobalSuitsConfig : Node
{
	public static GlobalSuitsConfig Instance { get; private set; }

	[Export]
	private SuitsConfig SuitsConfig;

	public Color? TryGetSuitColor(Suit suit)
	{
		if (SuitsConfig != null && SuitsConfig.Suits.TryGetValue(suit, out Color color))
		{
			return color;
		}
		
		return null;
	}

    public override void _Ready()
    {
        Instance = this;
    }
}
