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
        Vector2 Position { get; set;}

        List<TweenInfo> CreateTweenInfoForMove(IPile source, List<Card> cardList);
        List<StateChange> CreateStateChangeForMove(List<Card> cardList);
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