using Godot;
using System;

public interface IDropSpot
{
	public Vector2 ChildOffset { get; }

	public void TryDrop(Card droppedCard);
}
