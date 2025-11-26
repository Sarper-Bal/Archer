using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.LevelGeneration
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] Vector2 _spawnRange;
        [SerializeField] Transform _centerPoint;
        [SerializeField] Transform _endPoint;
        [SerializeField] float _heightOffsetForGroup;

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(_centerPoint.position, new Vector3(_spawnRange.x, 1f, _spawnRange.y));
        }

        public Transform GetEndPoint()
        {
            return _endPoint;
        }

        public void SpawnGroup(Group group)
        {
            float x = Random.Range(-_spawnRange.x, _spawnRange.x);
            float y = Random.Range(-_spawnRange.y, _spawnRange.y);
            Group instantiated = Instantiate(group, _centerPoint.position + new Vector3(x, _heightOffsetForGroup, y), Quaternion.Euler(0f, 180f, 0f));
            instantiated.transform.SetParent(transform);
        }

        public void SpawnPortal(Portal portal, LevelGenerator blg)
        {
            Portal p = Instantiate(portal, _centerPoint.position + Vector3.up * _heightOffsetForGroup, Quaternion.identity);
            p.transform.SetParent(transform);
            p.Initialize(blg, true);
        }
    }
}