using UnityEngine;

namespace Game.Data.Movement
{
    [CreateAssetMenu(fileName = "NewMovementData", menuName = "Static Data/Movement Data")]
    public class MovementConfig : ScriptableObject
    {
        public GroundConfig Ground;
        public AirConfig Air;
        public JumpConfig Jump;
        public SlideConfig Slide;
        public WallrunConfig Wallrun;
        public EnvironmentConfig Environment;
    }
}
