using UnityEngine;

namespace Game.Player
{
    [CreateAssetMenu(fileName = "NewBodyData", menuName = "Static Data/Body Data")]
    public class PlayerBodyData : ScriptableObject
    {
        public float StandingHeight = 1.96f;
        public float CrouchHeight = 0.96f;
        public float Radius = 0.32f;
    }
}