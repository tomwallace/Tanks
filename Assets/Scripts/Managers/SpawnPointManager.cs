using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class SpawnPointManager : MonoBehaviour
    {
        public Transform[] SpawnPoints;

        public Transform GetRandomSpawnPoint()
        {
            return SpawnPoints[Random.Range(0, SpawnPoints.Length - 1)];
        }
    }
}