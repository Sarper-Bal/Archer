using ArcadeBridge.ArcadeIdleEngine.Actors;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.LevelGeneration
{
	public class LevelGenerationTrigger : MonoBehaviour
	{
		[SerializeField] LevelGenerator _levelGenerator;
		[SerializeField] Transform _levelStartingPoint;
		[SerializeField] Vector3 _backPortalSpawnPoint;
		[SerializeField] int _level;
		[SerializeField] int _difficulty;
		[SerializeField] int _tileLength;

		void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out ArcadeIdleMover _))
			{
				SpawnLevel();
			}
		}

		void SpawnLevel()
		{
			_levelGenerator.Generate(_level, _difficulty, _tileLength, _levelStartingPoint.position, _backPortalSpawnPoint, gameObject);
		}
	}
}
