using Godot;
using System;

public partial class DeckManager : Node2D
{
	[Export]
	private PackedScene _cardScene;

	private RandomNumberGenerator rng = new RandomNumberGenerator();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept"))
		{
			GD.Print("Spawn");
			Card card = _cardScene.Instantiate<Card>();
			card.GlobalPosition = GetGlobalMousePosition();
			card.Suit = (Suit)(rng.Randi() % 4);
			AddChild(card);
		}
	}

	public bool DoCardsShareColor()
	{
		return false;
	}
}

public enum Zone
{
	Deck,
	Final,
	Board,
	PlayPile
}

// Solitaire
// 52 cards, 2 -> A in all 4 suits
// 4 Areas for stacking the cards
// The Deck
// 7 piles
