using Godot;
using System;
using System.Collections.Generic;

public partial class GlobalMoveSystem : Node
{
	public static GlobalMoveSystem Instance { get; private set; }

	[Signal]
	public delegate void OnPreviousMovePileCountChangeEventHandler(int count);

	[Signal]
	public delegate void OnRedoMovePileCountChangeEventHandler(int count); 
	
	private Stack<Move> PreviousMovePile;
	private Stack<Move> RedoMovePile;

    public override void _Ready()
    {
        Instance = this;
		PreviousMovePile = new Stack<Move>();
		RedoMovePile = new Stack<Move>();
    }


	public void ExecuteMove(Move move)
	{
		PreviousMovePile.Push(move);
		// New move has been executed, clear the redo stack
		RedoMovePile.Clear();

		IPile.Move(move.Source, move.Destination, move.CardInfoList);

		EmitSignal(SignalName.OnPreviousMovePileCountChange, PreviousMovePile.Count);
		EmitSignal(SignalName.OnRedoMovePileCountChange, RedoMovePile.Count);
	}

	public void UndoLastMove()
	{
		if (PreviousMovePile.Count != 0)
		{
			GD.PrintErr("UndoLastMove called while LastMoves stack was empty. This should never happen!");
			return;
		}
		Move moveToUndo = PreviousMovePile.Pop();
		IPile.Move(moveToUndo.Destination, moveToUndo.Source, moveToUndo.CardInfoList, moveToUndo.ReverseOnUndo);
		RedoMovePile.Push(moveToUndo);

		EmitSignal(SignalName.OnPreviousMovePileCountChange, PreviousMovePile.Count);
		EmitSignal(SignalName.OnRedoMovePileCountChange, RedoMovePile.Count);
	}

	public void RedoMove()
	{
		if (RedoMovePile.Count == 0)
		{
			GD.PrintErr("RedoMove called while RedoMovePile stack was empty. This should never happen!");
			return;
		}
		Move moveToRedo = RedoMovePile.Pop();
		IPile.Move(moveToRedo.Source, moveToRedo.Destination, moveToRedo.CardInfoList);
		PreviousMovePile.Push(moveToRedo);

		EmitSignal(SignalName.OnPreviousMovePileCountChange, PreviousMovePile.Count);
		EmitSignal(SignalName.OnRedoMovePileCountChange, RedoMovePile.Count);
	}

	public class Move
	{
		public MoveType MoveType;
		public List<CardInfo> CardInfoList;
		public IPile Source;
		public IPile Destination;
		public bool ReverseOnUndo;
	}

	// Mostly for debug purposes
	public enum MoveType
	{
		DECK_TO_SCRY,
		SCRY_TO_DECK,
		SCRY_TO_PLAY,
		PLAY_TO_PLAY,
		PLAY_TO_FINAL,
		FINAL_TO_PLAY,
		SCRY_TO_FINAL
	}
}
