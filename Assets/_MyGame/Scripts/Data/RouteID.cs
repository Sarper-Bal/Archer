using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    // Proje içinde sağ tık -> Create -> MyGame -> Route ID diyerek oluşturacaksın.
    [CreateAssetMenu(fileName = "NewRouteID", menuName = "MyGame/Route ID")]
    public class RouteID : ScriptableObject
    {
        [TextArea] public string Description;
    }
}