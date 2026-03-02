using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
	[GlobalClass]
	public abstract partial class Pile : Node2D
	{
		public LinkedList<Card> Contents { get; } = new LinkedList<Card>();

		[Export] public Vector2 ChildOffset	{ get; protected set; }

		[Export] public Zone Zone { get; protected set; }

		/// <summary>
        /// Attempts to search a pile for a specific Card. This search is conducted from the front of the pile.
        /// </summary>
        /// <param name="card">The exact card info to search a pile for</param>
        /// <returns>A list of cards, starting with the desired card. Null if not found</returns>
        public List<Card> SearchUntilCard(Card card)
        {
            List<Card> cards = new List<Card>();
            bool found = false;
            LinkedListNode<Card> node = Contents.First;
            while (node != null)
            {
                cards.Add(node.Value);
                if (node.Value.Value == card.Value
                    && node.Value.Suit == card.Suit)
                {
                    found = true;
                    break;
                }

                node = node.Next;
            }
            
            if (found)
            {
                cards.Reverse();
                return cards;
            }
            return null;
        }

        public void RemoveFromPile(int count)
        {
            if (count > Contents.Count)
            {
                GD.PrintErr("RemoveFromPile called with count value higher than Pile.Count");
                return;
            }
            for (int i = 0; i < count; i++)
            {
                Contents.RemoveFirst();
            }
        }

        public Card PopFrontCard()
        {
            Card result = null;
            if (Contents.Count != 1)
            {
                result = Contents.First.Value;
                RemoveFromPile(1);
            }

            return result;
        }

        public List<LinkedListNode<Card>> AddToPile(List<Card> cards)
        {
            List<LinkedListNode<Card>> newNodes = new List<LinkedListNode<Card>>();
            for (int i = 0; i < cards.Count; i++)
            {
                newNodes.Add(Contents.AddFirst(cards[i]));
            }

            newNodes.Reverse();
            return newNodes;
        }

        public LinkedListNode<Card> AddToPile(Card card)
        {
            return Contents.AddFirst(card);
        }

        public void ClearPile(Vector2 visualClearPosition, float visualClearTime)
        {
            Contents.Clear();
        }

		public abstract List<TweenInfo> CreateTweenInfoForMove(Pile source, List<Card> cardList);

		public abstract void UpdateVisuals();

		public static void Move(Pile source, Pile destination, List<Card> cardList, bool reverseCards = false)
        {
            if (cardList == null) return;
            source.RemoveFromPile(cardList.Count);

            if (reverseCards)
            {
                cardList.Reverse();
            }

            destination.AddToPile(cardList);
        }
	}	
}