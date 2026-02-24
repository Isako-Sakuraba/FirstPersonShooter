namespace Game.Interaction
{
    public interface IFocusable
    {
        void OnFocusEnter(in InteractionContext context);
        void OnFocusExit(in InteractionContext context);
    }
}