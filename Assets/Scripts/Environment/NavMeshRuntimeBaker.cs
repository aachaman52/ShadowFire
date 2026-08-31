using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace ShadowFire.Environment
{
    [RequireComponent(typeof(NavMeshSurface))]
    public class NavMeshRuntimeBaker : MonoBehaviour
    {
        private NavMeshSurface _surface;

        private void Awake()
        {
            _surface = GetComponent<NavMeshSurface>();
            if (_surface == null)
            {
                _surface = gameObject.AddComponent<NavMeshSurface>();
            }
            _surface.collectObjects = CollectObjects.All;
            _surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        }

        public void BakeNavMesh()
        {
            if (_surface == null)
            {
                _surface = GetComponent<NavMeshSurface>();
                if (_surface == null) _surface = gameObject.AddComponent<NavMeshSurface>();
            }

            _surface.BuildNavMesh();
            Debug.Log("[ShadowFire] NavMesh generated successfully for Arena.");
        }
    }
}
