using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
    public partial class NewPlaySpot : Area2D, IDropSpot, IPile
    {
        public PileData PileData { get; private set; }
        [Export] public Vector2 ChildOffset	{ get; set; }
		[Export] public Zone Zone { get; set; }
        [Export] public double CardAnimTime { get; set;}

        public override void _Ready()
        {
            PileData = new PileData(this);
        }

        public List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            return null;
        }

        public List<TweenInfo> CreateTweenInfoForMove(IPile source, List<Card> cardList)
        {
            List<TweenInfo> result = new List<TweenInfo>();

            // This can happen from SCRY, PLAY, FINAL
            // POSITION
            // Z INDEX
            if (cardList == null || cardList.Count == 0)
            {
                GD.PrintErr("PlaySpot.CreateTweenInfoForMove was passed an empty or null cardList");
                return result;
            }

            int offset = cardList[0].PileParent.GetChildOffsetCountForCard(cardList[0]);
            int targetOffset = PileData.Contents.Count;
            for (int i = 0; i < cardList.Count; i++)
            {
                Card card = cardList[i];

                result.Add(TweenInfo.CreateTweenInfo(card, "position",  
                    start: card.PileParent.GlobalPosition + (offset * card.PileParent.ChildOffset), 
                    end: GlobalPosition + (targetOffset * ChildOffset), 
                    duration: CardAnimTime, 
                    delay: 0));
                result.Add(TweenInfo.CreateTweenInfo(card, "z_index",
                    [
                        new ActionActive(StateChange.CreateStateChange(card, "z_index", start: offset, end: SolitaireGlobals.MoveZIndex + offset), duration: 0),
                        new ActionDelay(duration: CardAnimTime),
                        new ActionActive(StateChange.CreateStateChange(card, "z_index", start: SolitaireGlobals.MoveZIndex + offset, end: targetOffset), duration: 0)
                    ]));
                    
                // Rather than recalc ChildOffsetCountForCard with each card, just assume that the next one would be 1 off
                offset++;
                targetOffset++;
            }

            return result;
        }

        public bool TryDrop(Card droppedCard, out IPile pile)
        {
            bool result = false;
            if (PileData.Contents.Count == 0)
            {
                result = droppedCard.Value == (int)FaceValue.KING;
            }
            else
            {
                // check if dropped card is the right value and acceptable suit
                result = droppedCard.Value == PileData.Contents.First.Value.Value - 1
                    && (int)droppedCard.Suit / 2 != (int)PileData.Contents.First.Value.Suit / 2;
            }

            pile = this;
            return result;
        }

        public void UpdateVisuals()
        {
            throw new NotImplementedException();
        }

        public int GetChildOffsetCountForCard(Card card)
        {
            return Math.Abs(PileData.GetIndexForCard(card) - (PileData.Contents.Count - 1));
        }
    }
}