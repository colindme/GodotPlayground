using Godot;
using System;

public partial class Deck : Node2D
{
	[ExportCategory("Deck Depth Settings")]
	[Export]
	private Sprite2D _deckDepthSprite;

	[Export]
	private float _deckDepthCardScalar = 0.005f;

	private int cardCount = 52;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_deckDepthSprite.Scale = new Vector2(_deckDepthSprite.Scale.X, 1 + (_deckDepthCardScalar * cardCount));
	}
}

public struct CardInfo
{
	public Suit Suit { get; set; }
	public int CardValue { get; set; }
}