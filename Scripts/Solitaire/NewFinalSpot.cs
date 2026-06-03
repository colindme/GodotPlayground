using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
	[GlobalClass, Tool]
	public partial class NewFinalSpot : SuitedBase, IDropSpot, IPile
	{
		[Export] public Zone Zone { get; private set; }
        public PileData PileData { get; private set; }
		public Vector2 ChildOffset	{ get; private set; }

        public override void _Ready()
		{
			PileData = new PileData();
		}

        public bool TryDrop(Card droppedCard)
        {
            throw new NotImplementedException();
        }

		private int GetNextNeededValue()
        {
			return PileData.Contents.Count + 1;
        }

        public List<TweenInfo> CreateTweenInfoForMove(IPile source, List<Card> cardList)
        {
            throw new NotImplementedException();
        }

        public List<StateChange> CreateStateChangeForMove(List<Card> cardList)
        {
            throw new NotImplementedException();
        }

        public void UpdateVisuals()
        {
            throw new NotImplementedException();
        }
    }
}