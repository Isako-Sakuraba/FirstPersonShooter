using UnityEngine;

namespace Game.Data.Movement
{
    [CreateAssetMenu(fileName = "NewBodyData", menuName = "Static Data/Body Data")]
    public class BodyConfig : ScriptableObject
    {
        public float StandingHeight = 1.96f;
        public float CrouchHeight = 0.96f;
        public float Radius = 0.32f;
    }
}