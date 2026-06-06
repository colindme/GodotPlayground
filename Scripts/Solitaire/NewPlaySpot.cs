using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
    // I think I have to solve this one now
    // Rules for the play spot
    // start of game
    // manually add cards + animate
    // 
    // during game
    // how to handle children??
    //
    // end of game
    // just animate to deck
    public partial class NewPlaySpot : Area2D, IDropSpot, IPile
    {
        public PileData PileData { get; private set; }
        [Export] public Vector2 ChildOffset	{ get; set; }
		[Export] public Zone Zone { get; set; }

        public override void _Ready()
        {
            PileData = new PileData(this);
        }

        public List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            throw new NotImplementedException();
        }

        public List<TweenInfo> CreateTweenInfoForMove(IPile source, List<Card> cardList)
        {
            throw new NotImplementedException();
        }

        public bool TryDrop(Card droppedCard, out IPile pile)
        {
            pile = this;
            throw new NotImplementedException();
        }

        public void UpdateVisuals()
        {
            throw new NotImplementedException();
        }

        public List<Card> GetCardsUnderCard(Card card)
        {
            return PileData.SearchUntilCard(card);
        }
    }
}