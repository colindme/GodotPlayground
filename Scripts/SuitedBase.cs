using Godot;

public abstract partial class SuitedBase : Area2D
{
	[Signal]
	public delegate void OnSuitChangedEventHandler(Suit suit);

	[Export]
	public Suit Suit 
	{ 
		get => _suit; 
		set 
		{
			_suit = value;
			// TODO: Consider making the deferment an editor only functionality
			Callable.From(OnSuitChangedInternal).CallDeferred();
		}
	}
	protected Suit _suit;

	private void OnSuitChangedInternal()
	{
		EmitSignal(SignalName.OnSuitChanged, Variant.From(_suit));
	}
}

public enum Suit
{
	HEART = 0,
	DIAMOND,
	SPADE,
	CLUB
}