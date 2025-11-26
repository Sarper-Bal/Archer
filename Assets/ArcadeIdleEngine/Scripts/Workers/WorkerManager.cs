using System;
using System.Collections.Generic;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes_SpecialCase;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Workers
{
    [CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Workers) + "/" + nameof(WorkerManager))]
    public class WorkerManager : ScriptableObject
    {
        [SerializeField] WorkerMover workerMoverPrefab;

        [NonSerialized] readonly List<WorkerMover> _activeEmployees = new List<WorkerMover>();

        public void AssignEmployee(Vector3 loadPoint, Vector3 unloadPoint, float loadTime, float unloadTime, Vector3 instantiatingPoint)
        {
            var employee = Instantiate(workerMoverPrefab, instantiatingPoint, Quaternion.identity);
            employee.Initialize(loadPoint, unloadPoint, loadTime, unloadTime);
            _activeEmployees.Add(employee);
        }

        [Button]
        public void PauseAll()
        {
            foreach (WorkerMover activeEmployee in _activeEmployees)
            {
                activeEmployee.Pause();
            }
        }

        [Button]
        public void ResumeAll()
        {
            foreach (WorkerMover activeEmployee in _activeEmployees)
            {
                activeEmployee.Resume();
            }
        }
    }
}