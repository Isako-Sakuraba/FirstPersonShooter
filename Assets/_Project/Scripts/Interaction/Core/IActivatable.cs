namespace Game.Interaction
{
    public interface IActivatable
    {
        bool IsActivated { get; }

        bool Toggle();

        void SetActiveState(bool activated);
        bool TrySetActiveState(bool activated);

        void Activate();
        void Deactivate();
    }
}