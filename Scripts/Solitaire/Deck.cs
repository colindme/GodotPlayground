using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solitaire
{
	public partial class Deck : Area2D, IPile
	{
		[Export]
		private ScryPile ScryPile;

		[Export]
		private Sprite2D DeckSprite;

		[ExportCategory("Deck Depth Settings")]
		[Export]
		private Sprite2D DeckDepthSprite;

		[Export]
		private float DeckDepthCardScalar = 0.02f;

		private LinkedList<CardInfo> _deck = new LinkedList<CardInfo>();

		private bool _mouseHeld = false;

		LinkedList<CardInfo> IPile.Pile { get => _deck; set => _deck = value; }

		private const int _fullDeckCount = 52;

		public override void _Ready()
		{
			ResetDeck();
			InputEvent += OnInput;
		}

		private void ResetDeck()
		{
			CardInfo[] tempDeck = new CardInfo[_fullDeckCount];
			for (Suit suit = Suit.HEART; suit <= Suit.CLUB; suit++)
			{
				for (int value = (int)FaceValue.ACE; value <= (int)FaceValue.KING; value++)
				{
					int index = ((int)suit * (int)FaceValue.KING) + (value - 1);
					tempDeck[index] = new CardInfo()
					{
						Suit = suit,
						Value = value	
					};
				}
			}

			// TODO: Seed this?
			Random.Shared.Shuffle(tempDeck);
			if (_deck.Count != 0)
			{
				_deck.Clear();
			}

			((IPile)this).AddToPile(tempDeck.ToList());
		}

		public void UpdateVisuals()
		{
			DeckDepthSprite.Scale = new Vector2(DeckDepthSprite.Scale.X, 1 + (DeckDepthCardScalar * _deck.Count));
			DeckSprite.Visible = _deck.Count != 0;
			DeckDepthSprite.Visible = _deck.Count != 0;
		}

		private void OnInput(Node viewport, InputEvent @event, long shapeIdx) {
			if (@event is InputEventMouseButton mouseButtonEvent)
			{
				if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
				{
					if (mouseButtonEvent.Pressed)
					{
						_mouseHeld = true;
					}
					if (mouseButtonEvent.IsReleased() && _mouseHeld)
					{
						_mouseHeld = false;
						if (_deck.Count == 0)
						{
							GlobalMoveSystem.Move move = new GlobalMoveSystem.Move();
							move.MoveType = GlobalMoveSystem.MoveType.SCRY_TO_DECK;
							move.CardInfoList = ScryPile.GetPile();
							move.Source = ScryPile;
							move.Destination = this;
							move.ReverseOnUndo = true;

							GlobalMoveSystem.Instance.ExecuteMove(move);
						}
						else
						{
							GlobalMoveSystem.Move move = new GlobalMoveSystem.Move();
							move.MoveType = GlobalMoveSystem.MoveType.DECK_TO_SCRY;
							move.Source = this;
							move.Destination = ScryPile;
							move.ReverseOnUndo = true;

							int cardsToGrab = Mathf.Min(ScryPile.CardsToScry, _deck.Count);
							List<CardInfo> cards = new List<CardInfo>();
							LinkedListNode<CardInfo> currNode = null;
							for (int i = 0; i < cardsToGrab; i++)
							{
								currNode = currNode == null ? _deck.First : currNode.Next;
								cards.Add(currNode.Value);
							}
							move.CardInfoList = cards;

							GlobalMoveSystem.Instance.ExecuteMove(move);
						}
					}

					GetViewport().SetInputAsHandled();
				}
			}
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButtonEvent && _mouseHeld &&
				mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsReleased())
			{
				Callable.From(DelayedResetMouseHeld).CallDeferred();
			}

			if (@event.IsActionPressed("ui_left"))
			{
				GD.Print("Deck contents:");
				LinkedListNode<CardInfo> node = null;
				for (int i = 0; i < _deck.Count; i++)
				{
					node = node == null ? _deck.First : node.Next;
					GD.Print($"Order: {i  + 1} | CardInfo: {node.Value.Value} of {node.Value.Suit}");
				}
			}
		}

		private void DelayedResetMouseHeld()
		{
			_mouseHeld = false;
			GetViewport().SetInputAsHandled();
		}
	}

	public struct CardInfo
	{
		public Suit Suit { get; set; }
		public int Value { get; set; }
	}
}