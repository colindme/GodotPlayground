using System.Collections.Generic;
using Godot;

namespace Solitaire
{
    public interface IPile
    {
        public Vector2 ChildOffset	{ get; }
		public Zone Zone { get; }

        PileData PileData { get; }
        LinkedList<Card> Contents { get => PileData.Contents; }  
        Vector2 Position { get; set; }
        Vector2 GlobalPosition { get; set;}

        List<TweenInfo> CreateTweenInfoForMove(IPile source, List<Card> cardList);
        List<StateChange> CreateStateChangeForMove(List<Card> cardList);
        /// <summary>
        /// For a card in a pile, return the count that Pile.ChildOffset should be multiplied by for a card's position in a pile
        /// </summary>
        /// <param name="card">The card to sure for</param>
        /// <returns>The count that Pile.ChildOffset should be multiplied by</returns>
        int GetChildOffsetCountForCard(Card card);
		void UpdateVisuals();
    }

    public enum Zone
	{
		NONE,
		SCRY,
		PLAY,
		FINAL,
        DECK,
	}
}