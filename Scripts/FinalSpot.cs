using Godot;

[Tool]
public partial class FinalSpot : SuitedBase, IDropSpot
{
	[Export]
    public Vector2 ChildOffset { get; set;}

	private int nextNeededValue = 1;

    public void TryDrop(Card droppedCard)
    {
		// see if the dropped card is dropped alone

        if (droppedCard.Suit == _suit && droppedCard.Value == nextNeededValue)
		{
			
		}
    }
}