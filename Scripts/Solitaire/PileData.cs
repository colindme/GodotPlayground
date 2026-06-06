using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
	public partial class PileData
	{
		public LinkedList<Card> Contents { get; } = new LinkedList<Card>();

        private IPile _pileOwner; 

        public PileData(IPile pileOwner)
        {
            _pileOwner = pileOwner;
        }

		/// <summary>
        /// Attempts to search a PileData for a specific Card. This search is conducted from the front of the pile.
        /// </summary>
        /// <param name="card">The exact card info to search a PileData for</param>
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

        // Returns -1 if card was not found in the pile
        public int GetCardCountUnderCard(Card card)
        {
            List<Card> cards = SearchUntilCard(card);
            return cards != null ? cards.Count - 1 : -1;
        }

        public int GetIndexForCard(Card card)
        {
            int index = -1;
            LinkedListNode<Card> node = Contents.First;
            while (node != null)
            {
                index++;
                if (node.Value == card)
                {
                    break;
                }

                node = node.Next;
            }

            return index;
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
                cards[i].PileParent = _pileOwner;
                newNodes.Add(Contents.AddFirst(cards[i]));
            }

            newNodes.Reverse();
            return newNodes;
        }

        public LinkedListNode<Card> AddToPile(Card card)
        {
            card.PileParent = _pileOwner;
            return Contents.AddFirst(card);
        }

		public static void Move(PileData source, PileData destination, List<Card> cardList, bool reverseCards = false)
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