namespace Solitaire
{
    public abstract class TweenAction
    {
        public double Duration { get; set; }
    }

    public class ActionDelay : TweenAction
    {
        public ActionDelay(double duration)
        {
            Duration = duration;
        }

        public override string ToString()
        {
            return $"ActionDelay | Duration: {Duration}";
        }
    }

    public class ActionActive : TweenAction
    {
        public ActionActive(StateChange stateChange, double duration)
        {
            StateChange = stateChange;
            Duration = duration;
        }

        public StateChange StateChange { get; private set; }

        public override string ToString()
        {
            return $"ActionActive | Duration: {Duration} | {StateChange}";
        }
    }
}