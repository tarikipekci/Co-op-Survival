using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Player
{
    public class PlayerFlashlightController : NetworkBehaviour
    {
        [SerializeField] private GameObject flashlightPrefab;
        public GameObject currentFlashlight;
        private Light2D playerLight;

        public override void OnNetworkSpawn()
        {
            playerLight = gameObject.GetComponent<Light2D>();
            if (IsServer)
            {
                var dayNightManager = FindAnyObjectByType<Manager.DayNightManager>();
                if (dayNightManager != null && dayNightManager.IsNight())
                {
                    SetPlayerLightStateServerRpc(true);
                }
                else
                {
                    SetPlayerLightStateServerRpc(false);
                }
            }
        }

        private GameObject FindFlashlightOnClient()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.name.Contains(flashlightPrefab.name))
                    return child.gameObject;
            }

            return null;
        }

        public void SpawnFlashlight()
        {
            if (!IsServer)
                return;

            if (currentFlashlight != null)
            {
                var netObj = currentFlashlight.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsSpawned)
                {
                    Debug.LogWarning("Flashlight already spawned, skipping spawn.");
                    return;
                }
            }

            currentFlashlight = Instantiate(flashlightPrefab, transform.position, Quaternion.identity, transform);
            var networkObject = currentFlashlight.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError("Flashlight prefab does not have NetworkObject component!");
                Destroy(currentFlashlight);
                currentFlashlight = null;
                return;
            }

            networkObject.Spawn(true);
            currentFlashlight.transform.SetParent(gameObject.transform);
            SetPlayerLightStateClientRpc(true);

            Debug.Log("Flashlight spawned for player " + OwnerClientId);
        }

        public void RemoveFlashlight()
        {
            if (!IsServer)
                return;

            if (currentFlashlight == null)
                return;

            var netObj = currentFlashlight.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn();
            }

            Destroy(currentFlashlight);
            currentFlashlight = null;
            SetPlayerLightStateClientRpc(false);

            Debug.Log("Flashlight removed for player " + OwnerClientId);
        }

        public void UpdateLookDirection(Vector2 lookDir)
        {
            if (!IsOwner) return;

            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
            UpdateLookDirectionServerRpc(angle);
        }

        [ServerRpc]
        private void UpdateLookDirectionServerRpc(float angle)
        {
            UpdateLookDirectionClientRpc(angle);
        }

        [ClientRpc]
        private void UpdateLookDirectionClientRpc(float angle)
        {
            if (currentFlashlight == null)
            {
                currentFlashlight = FindFlashlightOnClient();
                if (currentFlashlight == null)
                {
                    return;
                }
            }

            currentFlashlight.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SetPlayerLightStateServerRpc(bool state)
        {
            SetPlayerLightStateClientRpc(state);
        }

        [ClientRpc]
        private void SetPlayerLightStateClientRpc(bool state)
        {
            if (playerLight != null)
                playerLight.enabled = state;
        }
    }
}