using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ScryPile : Marker2D, IPile
{
	[Export]
	public int CardsToScry
	{
		get => _cardsToScry;
		set
		{
			GD.Print($"Cards to Scry: {value}");
			_cardsToScry = value;
			_cardsToShow = Mathf.Max(_cardsToScry, MinCardsToShow);
			Callable.From(InitializeCards).CallDeferred();
		}
	}

	[ExportCategory("Card Visuals Settings")]
	[Export] 
	private PackedScene CardScene;

	[Export]
	private Node2D CardParent;

	[Export]
	private Vector2 ChildOffset;

	[Export]
	private int MinCardsToShow;

	[Export]
	private int BaseZIndex;

	private int _cardsToScry;
	private int _cardsToShow;

	// First most card in the pile is the most recent, back is oldest
	private LinkedList<CardInfo> _pile = new LinkedList<CardInfo>();

    LinkedList<CardInfo> IPile.Pile { get => _pile; set => _pile = value; }
	List<Card> _visualCards = new List<Card>();

    public List<CardInfo> GetPile()
	{
		return _pile.ToList();
	}

    public void UpdateVisuals()
    {
		// TODO: Handle undo / dragging off cards
        GD.Print("ScryPile | UpdateVisuals called");	
		// How to properly show this
		int cardsToShow = Mathf.Min(_cardsToShow, _pile.Count);
		GD.Print($"ScryPile | CardsToShow: {cardsToShow}");
		IEnumerator<CardInfo> cardInfoEnumerator = _pile.Take(cardsToShow).Reverse().GetEnumerator();
		
		for (int i = 0; i < cardsToShow; i++)
		{
			if (_visualCards[i].GetParent() == null)
			{
				CardParent.AddChild(_visualCards[i]);
			}

			_visualCards[i].ZIndex = BaseZIndex + i;
			_visualCards[i].Position = ChildOffset * i;
			cardInfoEnumerator.MoveNext();
			_visualCards[i].Suit = cardInfoEnumerator.Current.Suit;
			_visualCards[i].Value = cardInfoEnumerator.Current.Value;
			_visualCards[i].Scale = Vector2.One;
		}
		// Make sure the others are invisible
		for (int i = cardsToShow; i < _visualCards.Count; i++)
		{
			if (_visualCards[i].GetParent() != null)
			{
				CardParent.RemoveChild(_visualCards[i]);
			}
		}
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
		{
			LinkedListNode<CardInfo> node = null;
			GD.Print("ScryPile contents:");
			for (int i = 0; i < _pile.Count; i++)
			{
				node = node == null ? _pile.First : node.Next;
				GD.Print($"Order: {i  + 1} | CardInfo: {node.Value.Value} of {node.Value.Suit}");
			}
		}
    }

	private void InitializeCards()
	{
		if (_visualCards.Count < _cardsToShow)
		{
			int diff = _cardsToShow - _visualCards.Count;
			GD.Print($"Detected too few visual cards. Current: {_visualCards.Count} | Target: {_cardsToShow} | Difference: {diff}");
			for (int i = 0; i < diff; i++)
			{
				Card c = CardScene.Instantiate<Card>();
				_visualCards.Add(c);
				GD.Print("Added new card");
			}
		}
		else if (_visualCards.Count > _cardsToShow)
		{
			int diff = _visualCards.Count - _cardsToShow;
			GD.Print($"Detected too many visual cards. Current: {_visualCards.Count} | Target: {_cardsToShow} | Difference: {diff}");
			for (int i = 0; i < diff; i++)
			{
				_visualCards[_cardsToShow].QueueFree();
				_visualCards.RemoveAt(_cardsToScry);
				GD.Print("Removed card");
			}
		}

		UpdateVisuals();
	}
}