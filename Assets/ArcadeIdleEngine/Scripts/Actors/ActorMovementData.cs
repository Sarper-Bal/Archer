using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Actors
{
    [CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Actors) + "/" + nameof(ActorMovementData))]
    public class ActorMovementData : ScriptableObject
    {
        public float SideMoveSpeed;
        public float ForwardMoveSpeed;
    }
}