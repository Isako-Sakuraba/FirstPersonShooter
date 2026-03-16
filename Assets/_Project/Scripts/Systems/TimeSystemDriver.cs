using Game.Systems;
using UnityEngine;


namespace Game.Systems.Drivers 
{
    [DefaultExecutionOrder(-200)]
    public class TimeSystemDriver : MonoBehaviour
    {
        private void Update()
        {
            TimeSystem.Tick(Time.unscaledDeltaTime, Time.fixedUnscaledTime);
        }
    }
}
