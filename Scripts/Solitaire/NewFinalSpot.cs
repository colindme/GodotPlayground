using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
	[GlobalClass, Tool]
	public partial class NewFinalSpot : SuitedBase, IDropSpot, IPile
	{
        [Export] public double CardAnimTime { get; private set;}
		[Export] public Zone Zone { get; private set; }
        public PileData PileData { get; private set; }
		public Vector2 ChildOffset	{ get; private set; }

        public override void _Ready()
		{
			PileData = new PileData(this);
		}

        public bool TryDrop(Card droppedCard, out IPile pile)
        {
            pile = this;
            return droppedCard != null
                && droppedCard.Suit == Suit
                && droppedCard.Value == GetNextNeededValue()
                && droppedCard.PileParent.PileData.GetCardCountUnderCard(droppedCard) == 0;
        }

		private int GetNextNeededValue()
        {
			return PileData.Contents.Count + 1;
        }

        public List<TweenInfo> CreateTweenInfoForMove(IPile source, List<Card> cardList)
        {
            if (cardList.Count > 1)
            {
                GD.PrintErr("More than 1 card passed in to CreateTweenInfoForMove");
            }

            List<TweenInfo> result = new List<TweenInfo>();
            for (int i = 0; i < cardList.Count; i++)
            {
                Card card = cardList[i];
                int offset = Math.Abs(card.PileParent.PileData.GetIndexForCard(card) - (card.PileParent.PileData.Contents.Count - 1));
                
                result.Add(TweenInfo.CreateTweenInfo(card, "position", CardAnimTime, 0, card.PileParent.Position + card.PileParent.ChildOffset * offset, GlobalPosition));

                List<TweenAction> zIndexActions =
                [
                    new ActionActive(StateChange.CreateStateChange(card, "z_index", offset, SolitaireGlobals.MoveZIndex), 0),
                    new ActionDelay(CardAnimTime),
                    new ActionActive(StateChange.CreateStateChange(card, "z_index", SolitaireGlobals.MoveZIndex, 1), 0),
                ];

                result.Add(TweenInfo.CreateTweenInfo(card, "z_index", zIndexActions));
            }

            return result;
        }

        public List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            List<StateChange> result = new List<StateChange>();
            LinkedListNode<Card> node = PileData.Contents.First;
            if (node != null)
            {
                result.Add(StateChange.CreateStateChange(node.Value, "z_index", 1, 0));
                node = node.Next;
                if (node != null)
                {
                    result.Add(StateChange.CreateStateChange(node.Value, "visibile", true, false));
                }
            }

            return result;
        }

        public void UpdateVisuals()
        {
            throw new NotImplementedException();
        }
    }
}