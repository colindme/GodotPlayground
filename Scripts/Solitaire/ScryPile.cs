using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire
{
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

        public Zone Zone { get => Zone.SCRY; set {} }
		private LinkedList<CardInfo> _pile = new LinkedList<CardInfo>();
        public LinkedList<CardInfo> Pile { get => _pile; set => _pile = value; }
        [ExportCategory("Card Visuals Settings")]
		[Export] private PackedScene CardScene;
		[Export] private Node2D CardParent;
		[Export] public Vector2 ChildOffset { get; set; }
		[Export] private int MinCardsToShow;
		[Export] private int BaseZIndex;
		private int _cardsToScry;
		private int _cardsToShow;

        List<Card> _visualCards = new List<Card>();

		public List<CardInfo> GetPile()
		{
			return _pile.ToList();
		}

		public void UpdateVisuals()
		{
			// TODO: Handle undo / dragging off cards	
			int cardsToShow = Mathf.Min(_cardsToShow, _pile.Count);
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
				_visualCards[i].IsDraggable = false;
			}

			// Make sure the others are invisible
			for (int i = cardsToShow; i < _visualCards.Count; i++)
			{
				if (_visualCards[i].GetParent() != null)
				{
					CardParent.RemoveChild(_visualCards[i]);
				}
				_visualCards[i].IsDraggable = false;
			}

			if (_pile.Count > 0)
			{
				_visualCards[cardsToShow - 1].IsDraggable = true;
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
					c.CancelMove = UpdateVisuals;
					c.Zone = Zone.SCRY;
					c.PileParent = this;
					_visualCards.Add(c);
				}
			}
			else if (_visualCards.Count > _cardsToShow)
			{
				int diff = _visualCards.Count - _cardsToShow;
				GD.Print($"Detected too many visual cards. Current: {_visualCards.Count} | Target: {_cardsToShow} | Difference: {diff}");
				for (int i = 0; i < diff; i++)
				{
					_visualCards[_cardsToShow].QueueFree();
					_visualCards.RemoveAt(_cardsToShow);
				}
			}

			UpdateVisuals();
		}
    }
}
