using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Transformers
{
    [CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Processors) + "/" + nameof(Transformers) + "/" + nameof(TransformerRuleset))]
    public class TransformerRuleset : ScriptableObject
    {
        public ItemDefinitionCountPair[] Inputs;
        public ItemDefinitionCountPair[] Outputs;
    }
}