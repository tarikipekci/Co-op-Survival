using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Environment
{
    public class XPPickup : NetworkBehaviour
    {
        public int xpValue = 10;
        public float moveSpeed = 5f;
        private Transform target;
        private bool isActive;

        [SerializeField] private float detectionRadius = 4f;

        private Coroutine followRoutine;

        public void Activate(Vector3 spawnPos)
        {
            transform.position = spawnPos;
            isActive = true;

            followRoutine ??= StartCoroutine(CheckForPlayerAndFollow());
        }

        private IEnumerator CheckForPlayerAndFollow()
        {
            while (isActive)
            {
                if (target == null)
                {
                    var players = GameObject.FindGameObjectsWithTag("Player");

                    float closestDist = float.MaxValue;
                    Transform closest = null;

                    foreach (var p in players)
                    {
                        float dist = Vector3.Distance(transform.position, p.transform.position);
                        if (dist < detectionRadius && dist < closestDist)
                        {
                            closestDist = dist;
                            closest = p.transform;
                        }
                    }

                    if (closest != null)
                    {
                        target = closest;
                    }
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                }

                yield return null;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer || !isActive) return;

            if (other.CompareTag("Player"))
            {
                Manager.XPManager.Instance.AddXPServerRpc(xpValue);
                DespawnSelf();
            }
        }

        private void DespawnSelf()
        {
            isActive = false;
            target = null;

            if (followRoutine != null)
            {
                StopCoroutine(followRoutine);
                followRoutine = null;
            }

            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                Manager.NetworkPoolManager.Instance.Despawn(NetworkObject);
            }
        }
    }
}
