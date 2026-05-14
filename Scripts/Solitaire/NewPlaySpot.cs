using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
    public partial class NewPlaySpot : Pile, IDropSpot
    {
        public override List<TweenInfo> CreateTweenInfoForMove(Pile source, List<Card> cardList)
        {
            throw new NotImplementedException();
        }

        public override List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            throw new NotImplementedException();
        }

        public bool TryDrop(Card droppedCard)
        {
            
            throw new NotImplementedException();
        }

        public override void UpdateVisuals()
        {
            throw new NotImplementedException();
        }

    }
}