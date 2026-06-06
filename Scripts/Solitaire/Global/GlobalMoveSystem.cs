using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
		private MoveAnimationManager AnimationManager;

		public override void _Ready()
		{
			Instance = this;
			PreviousMovePile = new Stack<Move>();
			RedoMovePile = new Stack<Move>();
			AnimationManager = new MoveAnimationManager();
		}

		public void ExecuteMove(Move move)
		{
			GD.Print($"Move size: {move.CardList.Count}");
			List<TweenInfo> animationTweenInfo = move.Destination.CreateTweenInfoForMove(move.Source, move.CardList);
			List<StateChange> animationOnStartStateChange = move.Destination.CreateStateChangeForMove(move.CardList);
			MoveAnimation moveAnimation = new MoveAnimation(animationTweenInfo, animationOnStartStateChange);
			move.Animation = moveAnimation;

			PileData.Move(move.Source.PileData, move.Destination.PileData, move.CardList);
			AnimationManager.PlayAnimationFromStart(move.Animation);

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
			PileData.Move(moveToUndo.Destination.PileData, moveToUndo.Source.PileData, moveToUndo.CardList, moveToUndo.ReverseOnUndo);
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
			PileData.Move(moveToRedo.Source.PileData, moveToRedo.Destination.PileData, moveToRedo.CardList);
			PreviousMovePile.Push(moveToRedo);

			EmitSignal(SignalName.OnPreviousMovePileCountChange, PreviousMovePile.Count);
			EmitSignal(SignalName.OnRedoMovePileCountChange, RedoMovePile.Count);
		}

		public class Move
		{
			public List<Card> CardList { get; set; }
			public IPile Source { get; set; }
			public IPile Destination { get; set; }
			public bool ReverseOnUndo { get; set; }
			public MoveAnimation Animation { get; set; }
		}

		private class MoveAnimationManager
		{
			private Dictionary<int, MoveAnimation> _activeTweenDictionary;

			public MoveAnimationManager()
			{
				_activeTweenDictionary = new Dictionary<int, MoveAnimation>();
			}

			public void PlayAnimationFromStart(MoveAnimation move)
			{
				// See if any of the new tweens are currently being tweened by another move
				foreach (TweenInfo info in move.TweenInfos)
				{
					int hashCode = info.GetHashCode();
					if (_activeTweenDictionary.TryGetValue(hashCode, out MoveAnimation animation))
					{
						if (animation != null)
						{
							// Kill the current animation (considering it as done!)
							animation.KillTween(info, true);
						}
					}

					_activeTweenDictionary[hashCode] = move;
				}

				move.PlayFromStart();
			}

			public void ReverseAnimation(MoveAnimation move)
			{
				foreach (TweenInfo info in move.TweenInfos)
				{
					int hashCode = info.GetHashCode();
					if (_activeTweenDictionary.TryGetValue(hashCode, out MoveAnimation animation)
						&& animation != null
						&& animation != move)
					{
						animation.KillTween(info, true);
					}
				}
				move.Reverse();
			}

			public void RemoveMoveInfoForAnimation(MoveAnimation move)
			{
				foreach (TweenInfo info in move.TweenInfos)
				{
					int hashCode = info.GetHashCode();
					if (_activeTweenDictionary.TryGetValue(hashCode, out MoveAnimation dictMove))
					{
						if (dictMove == null || dictMove == move)
						{
							_activeTweenDictionary.Remove(hashCode);
						}
					}
				}
			}
		}

		public class MoveAnimation
		{
			public bool Finished { get; private set; }
			public bool InReverse { get; private set; }
			public List<TweenInfo> TweenInfos { get => _tweenInfos; }
			private int _completedTweens = 0;
			private double _longestDuration = 0;
			private double _animStartTime;
			private List<TweenInfo> _tweenInfos;
			private List<StateChange> _onStartStateChanges;

			private MoveAnimation() { }

			public MoveAnimation(List<TweenInfo> tweenInfos, List<StateChange> onStartStateChanges)
			{
				_tweenInfos = tweenInfos;
				_onStartStateChanges = onStartStateChanges;
				foreach (TweenInfo info in tweenInfos)
				{
					if (info.FullDuration > _longestDuration)
					{
						_longestDuration = info.FullDuration;
					}
				}

				// Add a delay at the end of MoveAnimation for any tweens who don't have the longest duration
				// In the future, consider not doing this and instead calculate the end delay at runtime
				foreach (TweenInfo info in tweenInfos)
				{
					if (_longestDuration > info.FullDuration)
					{
						info.TweenActions.Add(new ActionDelay(_longestDuration - info.FullDuration));
					}
				}
			}

			public void PlayFromStart()
			{
				Finished = false;
				if (_onStartStateChanges != null)
				{
					foreach (StateChange change in _onStartStateChanges)
					{
						if (IsInstanceValid(change.Node))
						{
							change.Node.Set(change.Property, change.EndVariant);
						}
					}
				}

				if (_tweenInfos != null)
				{
					foreach (TweenInfo info in _tweenInfos)
					{
						CreateTweenFromAction(info);
					}
				}

				_animStartTime = (double)Time.GetTicksUsec() / 1_000_000;
			}

			public void Reverse()
			{
				InReverse = !InReverse;
				_completedTweens = 0;
				foreach (StateChange change in _onStartStateChanges)
				{
					if (IsInstanceValid(change.Node))
					{
						change.Node.Set(change.Property, InReverse ? change.StartVariant : change.EndVariant);
					}
				}

				double elapsed = ((double)Time.GetTicksUsec() / 1_000_000) - _animStartTime;
				elapsed = Math.Min(elapsed, _longestDuration);
				double reverseElapsed = _longestDuration - elapsed;
				foreach (TweenInfo info in _tweenInfos)
				{
					if (!IsInstanceValid(info.Node))
					{
						_completedTweens++;
						continue;
					}

					int startIndex = InReverse ? info.TweenActions.Count - 1 : 0;
					int endIndex = InReverse ? -1 : info.TweenActions.Count;
					int step = InReverse ? -1 : 1;
					double r = reverseElapsed;
					for (int i = startIndex; i != endIndex; i += step)
					{
						TweenAction action = info.TweenActions[i];
						r -= action.Duration;
						if (r > 0)
						{
							continue;
						}

						double customDuration = Math.Abs(r);
						CreateTweenFromAction(info, action, customDuration);
						break;
					}
				}

				_animStartTime = (double)Time.GetTicksUsec() / 1_000_000 - reverseElapsed;
				Finished = false;
				CheckAnimationCompletion();
			}

			private void CreateTweenFromAction(TweenInfo info, TweenAction customStartAction = null, double? customStartDuration = null)
			{
				if (!IsInstanceValid(info.Node)) return; 

				KillTween(info);
				Tween tween = info.Node.CreateTween();
				int startIndex = InReverse ? info.TweenActions.Count - 1 : 0;
				int endIndex = InReverse ? -1 : info.TweenActions.Count;
				int step = InReverse ? -1 : 1;
				bool foundCustomStartAction = false;
				for (int i = startIndex; i != endIndex; i += step)
				{
					TweenAction action = info.TweenActions[i];
					double duration = action.Duration;
					if (customStartAction != null && !foundCustomStartAction)
					{
						if (action == customStartAction)
						{
							foundCustomStartAction = true;
							if (customStartDuration.HasValue)
							{
								duration = customStartDuration.Value;
							}
						}
						else
						{
							continue;
						}
					}

					if (action is ActionDelay)
					{
						tween.TweenInterval(duration);
					}
					else if (action is ActionActive actionActive)
					{
						tween.TweenProperty(info.Node, info.Property, InReverse ? actionActive.StateChange.StartVariant : actionActive.StateChange.EndVariant, duration);
					}
				}

				tween.TweenCallback(Callable.From(OnTweenFinish));
				info.ActiveTween = tween;
			}

			public void KillTween(TweenInfo info, bool considerTweenAsComplete = false)
			{
				if (info.ActiveTween != null && info.ActiveTween.IsRunning())
				{
					GD.PrintErr($"CreateTween called while tween is currently running. Name: {info.Node.Name} | Property: {info.Property}");
					info.ActiveTween.Kill();
					info.ActiveTween = null;

					if (considerTweenAsComplete)
					{
						_completedTweens++;
						CheckAnimationCompletion();
					}
				}
			}

			private void OnTweenFinish()
			{
				_completedTweens++;
				CheckAnimationCompletion();
			}

			private void CheckAnimationCompletion()
			{
				if (_completedTweens == _tweenInfos.Count)
				{
					OnAnimationFinish();
				}
			}

			private void OnAnimationFinish()
			{
				GD.Print("All tweens successfully finished!");
				Finished = true;
				Instance.AnimationManager.RemoveMoveInfoForAnimation(this);
			}
		}
	}

	public class TweenInfo
    {
        private TweenInfo() {}

        public static TweenInfo CreateTweenInfo(Node node, string property, double duration, double delay, Variant start, Variant end)
        {
			double fullDuration = 0;
			List<TweenAction> tweenActions = new List<TweenAction>();
			if (delay > 0)
			{
				tweenActions.Add(new ActionDelay(delay));
				fullDuration += delay;
			}

			StateChange stateChange = StateChange.CreateStateChange(node, property, start, end);
			tweenActions.Add(new ActionActive(stateChange, duration));
			fullDuration += duration;
            return new TweenInfo()
            {
                Node = node,
				Property = property,
				TweenActions = tweenActions,
                FullDuration = fullDuration
            };
        }

		public static TweenInfo CreateTweenInfo(Node node, string property, List<TweenAction> actions)
		{
			double fullDuration = 0;
			foreach(TweenAction action in actions)
			{
				fullDuration += action.Duration;
			}

			return new TweenInfo()
			{
				Node = node,
				Property = property,
				TweenActions = actions,
				FullDuration = fullDuration
			};
		}

		public Node Node { get; private set; }
		public string Property { get; private set; }
		public double FullDuration { get; private set; }
		public List<TweenAction> TweenActions { get; private set; }
		public Tween ActiveTween { get; set; }

		public void AppendNewAction(TweenAction action)
		{
			FullDuration += action.Duration;
			TweenActions.Add(action);
		}

        public override string ToString()
        {
            return $"Node: {Node.Name} | Property: {Property} | Full Duration: {FullDuration}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Node.GetInstanceId(), StringComparer.OrdinalIgnoreCase.GetHashCode(Property)); 
        }
    }

	public class StateChange
	{
		private StateChange() { }
		public static StateChange CreateStateChange(Node node, string property, Variant start, Variant end)
		{
			return new StateChange()
			{
				Node = node,
				Property = property,
				StartVariant = start,
				EndVariant = end
			};
		}

		public Node Node { get; private set; }
		public string Property { get; private set; }
		public Variant StartVariant { get; private set; }
		public Variant EndVariant { get; private set; }

        public override string ToString()
        {
            return $"StateChange | Node: {Node.Name} | Property: {Property} | StartVariant: {StartVariant} | EndVariant: {EndVariant} |";
        }
	} 
}