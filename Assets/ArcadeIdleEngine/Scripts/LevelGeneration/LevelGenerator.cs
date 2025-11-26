using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.LevelGeneration
{
    [CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(LevelGeneration) + "/" + nameof(LevelGenerator))]
    public class LevelGenerator : ScriptableObject
    {
        [SerializeField] LevelGeneratorDefinition[] _data;
        [SerializeField] Portal _townPortal;

        GameObject _levelEntrance;
        List<GameObject> _instantiatedLevel;

        void OnEnable()
        {
            _instantiatedLevel = new List<GameObject>();
        }
        
        // Each battle arena have tiles, and every tile have an enemy based on difficulty
        public void Generate(int level, int difficulty, int tileLength, Vector3 tileStartingPoint, Vector3 backPortalPoint, GameObject levelEntrance)
        {
            _levelEntrance = levelEntrance;
            levelEntrance.SetActive(false);
            SpawnBackPortal(tileStartingPoint, backPortalPoint);
            SpawnLevel(level, difficulty, tileLength, tileStartingPoint);
        }

        public void DestroyLevel()
        {
            _levelEntrance.SetActive(true);
            foreach (GameObject instantiatedLandPiece in _instantiatedLevel)
            {
                Destroy(instantiatedLandPiece);
            }

            _instantiatedLevel.Clear();
        }

        void SpawnBackPortal(Vector3 tileStartPoint, Vector3 portalSpawnPointRelative)
        {
            Portal p = Instantiate(_townPortal, tileStartPoint + portalSpawnPointRelative, Quaternion.identity);
            p.Initialize(this, false);
            _instantiatedLevel.Add(p.gameObject);
        }

        void SpawnLevel(int level, int difficulty, int tileLength, Vector3 startingPos)
        {
            LevelGeneratorDefinition currentLevelDefinition = _data[level];
            Tile land = currentLevelDefinition.GetTile();
            Tile previousLand = InstantiateLand(land, startingPos);
            for (int i = 0; i < tileLength; i++)
            {
                previousLand = InstantiateLand(land, previousLand.GetEndPoint().position);
                int remainedTile = (tileLength - i);
                bool haveEnoughTile = remainedTile * _data[level].MaxLevel <= difficulty;
                if (haveEnoughTile)
                {
                    // we don't have enought tile so give highest difficulty battle group
                    previousLand.SpawnGroup(currentLevelDefinition.GetGroup(_data[level].MaxLevel));
                    difficulty -= _data[level].MaxLevel;
                }
                else
                {
                    // we have enough tile so give random battle group
                    previousLand.SpawnGroup(currentLevelDefinition.GetRandomGroup(out int rnd));
                    difficulty -= rnd;
                }
            }

            previousLand = SpawnMaxLevelGroup(land, previousLand, currentLevelDefinition.GetLevelEndGroup());
            previousLand = SpawnPortal(land, previousLand);
        }

        Tile SpawnPortal(Tile land, Tile previousLand)
        {
            previousLand = InstantiateLand(land, previousLand.GetEndPoint().position);
            previousLand.SpawnPortal(_townPortal, this);
            return previousLand;
        }

        Tile SpawnMaxLevelGroup(Tile land, Tile previousLand, Group spawnGroup)
        {
            previousLand = InstantiateLand(land, previousLand.GetEndPoint().position);
            previousLand.SpawnGroup(spawnGroup);
            return previousLand;
        }

        Tile InstantiateLand(Tile land, Vector3 position)
        {
            Tile landPiece = Instantiate(land, position, Quaternion.identity);
            _instantiatedLevel.Add(landPiece.gameObject);
            return landPiece;
        }
    }
}