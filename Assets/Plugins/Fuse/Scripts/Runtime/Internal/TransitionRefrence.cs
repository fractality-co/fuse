namespace Fuse
{
    /// <summary>
    /// Internal reference to a transition and what state/events it encapsulates.
    /// </summary>
    public class TransitionReference
    {
        private readonly Transition _transition;
        private bool _fired;

        public TransitionReference(Transition transition)
        {
            _transition = transition;
        }

        public bool Transition => _fired;
        public string State => _transition.To;

        public bool ProcessEvent(string type)
        {
            if (type == _transition.Event)
                _fired = true;
            return Transition;
        }
    }
}