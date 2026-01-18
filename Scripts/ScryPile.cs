using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ScryPile : Marker2D, IPile
{
	[Export]
	public int CardsToScry = 1;

	// First most card in the pile is the most recent, back is oldest
	private LinkedList<CardInfo> _pile = new LinkedList<CardInfo>();

    LinkedList<CardInfo> IPile.Pile { get => _pile; set => _pile = value; }

    public List<CardInfo> GetPile()
	{
		return _pile.ToList();
	}

    public void UpdateVisuals()
    {
        GD.Print("ScryPile | UpdateVisuals called");	
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
		{
			LinkedListNode<CardInfo> node = null;
			GD.Print("ScryPile contents:");
			for (int i = 0; i < _pile.Count; i++)
			{
				node = node == null ? _pile.First : node.Next;
				GD.Print($"Order: {i  + 1} | CardInfo: {node.Value.Value} of {node.Value.Suit}");
			}
		}
    }

}