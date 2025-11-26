using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Transformers
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Processors) + "/" + nameof(Transformers) + "/" + nameof(TransformerDefinition))]
	public class TransformerDefinition : ScriptableObject
	{
		[field: SerializeField] public TransformerRuleset Ruleset { get; private set; }
		[field: SerializeField, Range(0.01f, 2f)] public float JumpHeight { get; private set; }
		[field: SerializeField, Range(0.01f, 2f)] public float JumpDuration { get; private set; }
	}
}