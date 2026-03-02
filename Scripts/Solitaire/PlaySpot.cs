using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
	public partial class PlaySpot : Area2D//, IPile, IDropSpot
	{
		/*
		private LinkedList<CardInfo> _pile = new LinkedList<CardInfo>();
		public LinkedList<CardInfo> Pile { get => _pile; set => _pile = value; }

		[Export]
		public Vector2 ChildOffset { get; set; }

		public Zone Zone { get => Zone.PLAY; set {} }

        public bool TryDrop(Card droppedCard)
		{
			bool success = false;

			if (droppedCard.PileParent != null)
			{
				if (_pile.Count == 0)
				{
					if (droppedCard.Value == (int)FaceValue.KING)
					{
						success = true;
					}
				}
				else
				{
					int nextExpectedValue = _pile.First.Value.Value - 1;
					HashSet<Suit> nextAllowedSuits = new HashSet<Suit>();
					if (_pile.First.Value.Suit == Suit.DIAMOND || _pile.First.Value.Suit == Suit.HEART)
					{
						nextAllowedSuits.Add(Suit.SPADE);
						nextAllowedSuits.Add(Suit.CLUB);
					}
					else
					{
						nextAllowedSuits.Add(Suit.HEART);
						nextAllowedSuits.Add(Suit.DIAMOND);
					}

					if (droppedCard.Value == nextExpectedValue && nextAllowedSuits.Contains(droppedCard.Suit))
					{
						success = true;
					}
				}
			}

			if (success)
			{
				GlobalMoveSystem.Move move = new GlobalMoveSystem.Move()
				{
					Source = droppedCard.PileParent,
					Destination = this,
					ReverseOnUndo = true,
					CardList = droppedCard.PileParent.SearchUntilCard(new CardInfo()
					{
						Value = droppedCard.Value,
						Suit = droppedCard.Suit
					})
				};

				GlobalMoveSystem.Instance.ExecuteMove(move);
			}

			return success;
		}

    	public void AddCardVisual(Card card, LinkedListNode<CardInfo> node)
        {
			// What does AddCardVisual for PlaySpot need to do
			card.PileParent = this;
			card.Zone = Zone;
        }

		public void UpdateVisuals()
		{
			throw new NotImplementedException();
		}
		*/

		// Should Pile care about CardVisuals?
		// Why would this be a good idea?
		/*
			IPile.List<Card> cardVisualList
			IPile.ClearPile(Vector2 visualClearPosition, float animationTime)
			RemoveFromPile(Pile.Count) 

		*/
	}
}
