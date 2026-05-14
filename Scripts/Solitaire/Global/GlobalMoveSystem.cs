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
			List<TweenInfo> animationTweenInfo = move.Destination.CreateTweenInfoForMove(move.Source, move.CardList);
			List<StateChange> animationOnStartStateChange = move.Destination.CreateStateChangeForMove(move.CardList);
			MoveAnimation moveAnimation = new MoveAnimation(animationTweenInfo, animationOnStartStateChange);
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
				foreach (TweenInfo info in tweenInfos)
				{
					if (_longestDuration > info.FullDuration)
					{
						GD.Print($"Creating end delay for {info} | {_longestDuration - info.FullDuration}s");
						info.TweenActions.Add(new ActionDelay(_longestDuration - info.FullDuration));
					}
				}
			}

			public void PlayFromStart()
			{
				Finished = false;
				foreach (StateChange change in _onStartStateChanges)
				{
					if (IsInstanceValid(change.Node))
					{
						change.Node.Set(change.Property, change.EndVariant);
					}
				}

				foreach (TweenInfo info in _tweenInfos)
				{
					CreateTweenFromAction(info);
				}

				_animStartTime = (double)Time.GetTicksMsec() / 1_000;
			}

			// ISSUE: when spamming reverse, the anim speeds up because currentActionStartTime gets so small
			// Calc progress?
			// now - start time
			public void Reverse()
			{
				GD.PrintErr("In Reverse");
				InReverse = !InReverse;
				_completedTweens = 0;
				foreach (StateChange change in _onStartStateChanges)
				{
					if (IsInstanceValid(change.Node))
					{
						change.Node.Set(change.Property, InReverse ? change.StartVariant : change.EndVariant);
					}
				}

				// TODO: Add logs here to debug this
				double timeSinceAnimStart = ((double)Time.GetTicksMsec() / 1_000) - _animStartTime;
				foreach (TweenInfo info in _tweenInfos)
				{
					GD.Print($"currentAction: {info.CurrentAction}");
					if (!IsInstanceValid(info.Node))
					{
						_completedTweens++;
						continue;
					}

					double? timeSinceCurrentTweenActionStart = null;
					if (info.CurrentAction != null)
					{
						double test = ((double)Time.GetTicksMsec() / 1_000) - info.CurrentActionStartTimeSec;
						GD.PrintErr($"Got diff of: {test} | Full Duration {info.CurrentAction.Duration}");
						timeSinceCurrentTweenActionStart = Math.Min(test, info.CurrentAction.Duration);
						GD.Print($"Calculated a time since current action start of: {timeSinceCurrentTweenActionStart} for {info}");
					}

					CreateTweenFromAction(info, info.CurrentAction, timeSinceCurrentTweenActionStart);
				}

				Finished = false;
				CheckAnimationCompletion();
			}

			private void CreateTweenFromAction(TweenInfo info, TweenAction customStartAction = null, double? customStartDuration = null)
			{
				if (!IsInstanceValid(info.Node)) return; 

				KillTween(info);
				Tween tween = info.Node.CreateTween();

				List<TweenAction> actions = info.TweenActions;
				if (InReverse)
				{
					actions.Reverse();
				}
				
				GD.Print("ACTIONS");
				foreach (TweenAction action in actions)
				{
					GD.Print(action);
				}
				GD.Print("TWEEN ACTIONS ORDER");
				foreach (TweenAction action in info.TweenActions)
				{
					GD.Print(action);
				}

				GD.Print($"In CreateTweenFromAction for {info}");
				bool foundCustomStartAction = false;
				foreach (TweenAction action in actions)
				{
					double duration = action.Duration;
					if (customStartAction != null && !foundCustomStartAction)
					{
						if (action == customStartAction)
						{
							GD.Print($"Found custom start action: {customStartAction}");
							foundCustomStartAction = true;
							if (customStartDuration.HasValue)
							{
								duration = customStartDuration.Value;
							}
						}
						else
						{
							GD.Print($"Skipping action {action} because it was before custom start action: {customStartAction}");
							continue;
						}
					}

					GD.Print($"Creating set current action call for {action}");
					tween.TweenCallback(Callable.From(() => info.SetCurrentAction(action)));
					if (action is ActionDelay)
					{
						GD.Print($"Creating action delay for {action}");
						tween.TweenInterval(duration);
					}
					else if (action is ActionActive actionActive)
					{
						GD.Print($"Creating action active for {action}");
						tween.TweenProperty(info.Node, info.Property, InReverse ? actionActive.StateChange.StartVariant : actionActive.StateChange.EndVariant, duration);
					}
					// TODO: Log this
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
				else
				{
					GD.PrintErr($"CreateTween called while tween is NOTNOTNOTNOT running. Name: {info.Node.Name} | Property: {info.Property}");
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

		public Node Node { get; private set; }
		public string Property { get; private set; }
		public double FullDuration { get; private set; }
		public List<TweenAction> TweenActions { get; private set; }
		public TweenAction CurrentAction { get; private set; }
		public double CurrentActionStartTimeSec { get; private set; }
		public Tween ActiveTween { get; set; }

        public override string ToString()
        {
            return $"Node: {Node.Name} | Property: {Property} | Full Duration: {FullDuration}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Node.GetInstanceId(), Property.ToUpper().GetHashCode()); 
        }

		public void SetCurrentAction(TweenAction currentAction)
		{
			CurrentAction = currentAction;
			CurrentActionStartTimeSec = (double)Time.GetTicksMsec() / 1_000;
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