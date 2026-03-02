using Godot;
using System;
using System.Collections.Generic;

namespace Solitaire
{
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
			List<TweenInfo> animationTweenInfo = move.Destination.CreateTweenInfoForMove(move.Source, move.CardList);
			MoveAnimation moveAnimation = new MoveAnimation(animationTweenInfo);
			move.Animation = moveAnimation;

			Pile.Move(move.Source, move.Destination, move.CardList);

			PreviousMovePile.Push(move);
			// New move has been executed, clear the redo stack
			RedoMovePile.Clear();
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
			Pile.Move(moveToUndo.Destination, moveToUndo.Source, moveToUndo.CardList, moveToUndo.ReverseOnUndo);
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
			Pile.Move(moveToRedo.Source, moveToRedo.Destination, moveToRedo.CardList);
			PreviousMovePile.Push(moveToRedo);

			EmitSignal(SignalName.OnPreviousMovePileCountChange, PreviousMovePile.Count);
			EmitSignal(SignalName.OnRedoMovePileCountChange, RedoMovePile.Count);
		}

		public class Move
		{
			public List<Card> CardList { get; set; }
			public Pile Source { get; set; }
			public Pile Destination { get; set; }
			public bool ReverseOnUndo { get; set; }
			public MoveAnimation Animation { get; set; }
		}

		// TODO: What happens if one MoveAnimation interrupts another MoveAnimation?
		public class MoveAnimation
		{
			public bool Finished { get; private set; }
			public bool InReverse { get; private set; }
			private int _completedTweens = 0;
			private double _longestDuration = 0;
			private Dictionary<int, TweenInfo> _tweenDictionary;

			private MoveAnimation() { }

			public MoveAnimation(List<TweenInfo> tweenInfo)
			{
				_tweenDictionary = new Dictionary<int, TweenInfo>();
				foreach (TweenInfo info in tweenInfo)
				{
					CreateTween(info);
					if (info.Duration > _longestDuration)
					{
						_longestDuration = info.Duration;
					}
				}
			}

			public void Reverse()
			{
				// For now, only support reversing a finished anim
				if (!Finished) return;
				Finished = false;

				InReverse = !InReverse;
				_completedTweens = 0;
				foreach (TweenInfo info in _tweenDictionary.Values)
				{
					double delay = _longestDuration - info.Duration;
					if (delay != 0)
					{
						// TODO Add a cancellable timer (because it will be interruptable in the future!)
						SceneTreeTimer timer = info.Node.GetTree().CreateTimer(delay, false);
						GD.Print($"Delaying for {delay} for {info.Node.Name} | Property: {info.Property}");
						timer.Timeout += () => CreateTween(info);
						timer.Timeout += () => GD.Print($"Timer finished for {info.Node.Name} | Property: {info.Property}");
					}
					else
					{
						GD.Print($"Not delaying for {info.Node.Name} | Property: {info.Property}");
						CreateTween(info);
					}
				}
			}

			private void OnTweenFinish(int infoHashCode)
			{
				TweenInfo info = _tweenDictionary[infoHashCode];
				_completedTweens++;
				ulong timeDiff = Time.GetTicksMsec() - info.StartTimeTicks;
				GD.Print($"OnTweenFinish | CompletedTweens: {_completedTweens} | Time Diff: {timeDiff}");
				
				if (_completedTweens == _tweenDictionary.Keys.Count)
				{
					OnAnimationFinish();
				}
			}

			private void CreateTween(TweenInfo info)
			{
				Tween tween = info.Node.CreateTween();
				tween.TweenProperty(info.Node, info.Property, InReverse ? info.StartVariant : info.EndVariant, info.Duration);
				tween.TweenCallback(Callable.From(() => OnTweenFinish(info.GetHashCode())));
				info.ActiveTween = tween;
				if (_tweenDictionary.TryGetValue(info.GetHashCode(), out TweenInfo storedInfo)
					&& storedInfo.ActiveTween != null
					&& storedInfo.ActiveTween.IsRunning())
				{
					GD.PrintErr($"CreateTween called while tween is currently running! This should not happen. Name: {info.Node.Name} | Property: {info.Property}");
				}

				_tweenDictionary[info.GetHashCode()] = info;
			}

			private void OnAnimationFinish()
			{
				GD.Print("All tweens successfully finished!");
				Finished = true;
			}

		}
	}

	public class TweenInfo
    {
        private TweenInfo() {}

        public static TweenInfo CreateTweenInfo(Node node, string property, double duration, Variant start, Variant end)
        {
            return new TweenInfo()
            {
                Node = node,
                Property = property,
                Duration = duration,
                StartVariant = start,
                EndVariant = end,
                InReverse = false,
                StartTimeTicks = Time.GetTicksMsec()
            };
        }

        public Node Node { get; set; }
        public string Property { get; set; }
        public double Duration { get; set; }
        public Variant StartVariant { get; set; }
        public Variant EndVariant { get; set; }
        public bool InReverse { get; set; }
        public ulong StartTimeTicks { get; set; }
		public Tween ActiveTween { get; set; }

		// TODO: Delay?
		// TODO: Timer?

        public override string ToString()
        {
            return $"Node: {Node.Name} | Property: {Property} | Duration: {Duration}";
        }

        public override int GetHashCode()
        {
            return (int)Node.GetInstanceId() ^ Property.GetHashCode(); 
        }
    }
}