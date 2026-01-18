using Godot;

[Tool]
public partial class GlobalSuitsConfig : Node
{
	public static GlobalSuitsConfig Instance { get; private set; }

	[Export]
	private SuitsConfig SuitsConfig;

	public Color? TryGetSuitColor(Suit suit)
	{
		//if (SuitsConfig != null && SuitsConfig.Suits.TryGetValue((int)suit, out Color color))
		//{
		//	return color;
		//}
		
		return null;
	}

    public override void _Ready()
    {
        GD.Print("GlobalSuitsConfig | Ready");
        Instance = this;
    }

    public override void _Process(double delta)
    {
        //GD.Print($"Hello. Is Instance null?: {Instance == null}");
    }

    public override void _EnterTree()
    {
        GD.Print("GlobalSuitsConfig | EnterTree");
    }

	public override void _ExitTree()
	{
		GD.Print("GlobalSuitsConfig | ExitTree");
	}
}
