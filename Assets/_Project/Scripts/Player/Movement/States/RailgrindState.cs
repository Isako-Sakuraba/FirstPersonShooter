using UnityEngine.Splines;

namespace Game.Player.Movement.States
{
    public class RailgrindState : PlayerMovementStateBase
    {
        public RailgrindState(PlayerContext context) : base(context) { }

        private SplineContainer _railSpline;

        public override void Enter()
        {
            
        }
    }
}