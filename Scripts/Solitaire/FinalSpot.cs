using System.Collections.Generic;
using Godot;

namespace Solitaire
{
    [Tool]
    public partial class FinalSpot : SuitedBase, IDropSpot, IPile
    {
        [Export] public Vector2 ChildOffset { get; set;}   
        public Zone Zone { get => Zone.FINAL; set {} }
        public LinkedList<CardInfo> Pile => _pile;
        private LinkedList<CardInfo> _pile = new LinkedList<CardInfo>();

        bool IDropSpot.TryDrop(Card droppedCard)
        {
            if (droppedCard.PileParent != null && droppedCard.Suit == _suit && droppedCard.Value == GetNextNeededValue())
            {            
                List<CardInfo> cardInfo = droppedCard.PileParent.SearchUntilCard(new CardInfo()
                {
                    Value = droppedCard.Value,
                    Suit = droppedCard.Suit
                });
                if (cardInfo == null || cardInfo.Count != 1)
                {
                    return false;
                }

                GlobalMoveSystem.Move move = new GlobalMoveSystem.Move()
                {
                    Source = droppedCard.PileParent,
                    Destination = this,
                    ReverseOnUndo = false,
                    CardInfoList = cardInfo
                };

                GlobalMoveSystem.Instance.ExecuteMove(move);
                return true;
            }
            return false;
        }

        public void UpdateVisuals()
        {
            GD.Print($"Final Spot Info | Count: {_pile.Count} | Next Value needed: {GetNextNeededValue()}");
            LinkedListNode<CardInfo> node = _pile.First;
            while (node != null)
            {
                GD.Print($"Value: {node.Value.Value}");
                node = node.Next;
            }
            if (_pile.Count == 0)
            {
                
            }
            else
            {
                
            }
        }

        private int GetNextNeededValue()
        {
            if (Pile.Count == 0)
            {
                return 1;
            }
            else
            {
                return Pile.First.Value.Value + 1;
            }
        }
    }
}