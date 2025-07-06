using Data;
using Unity.Netcode;
using UnityEngine;
using Weapon;
using System.Collections;

namespace Manager
{
    public class WeaponManager : NetworkBehaviour
    {
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private WeaponData[] availableWeapons;

        private WeaponBehaviour currentWeapon;
        private int currentWeaponIndex = -1;
        private bool isSpawning;

        public override void OnNetworkSpawn()
        {
            if (IsServer && currentWeaponIndex == -1)
            {
                StartCoroutine(EquipInitialWeapon());
            }
        }

        private IEnumerator EquipInitialWeapon()
        {
            yield return new WaitForSeconds(0.5f);
            EquipWeaponServerRpc(0);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                if (currentWeapon != null && currentWeapon.NetworkObject != null &&
                    currentWeapon.NetworkObject.IsSpawned)
                {
                    currentWeapon.NetworkObject.Despawn();
                }

                currentWeapon = null;
                currentWeaponIndex = -1;
                isSpawning = false;
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) TryEquipWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryEquipWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryEquipWeapon(2);

            if (Input.GetMouseButtonDown(0))
            {
                currentWeapon?.Attack();
            }
        }

        private void TryEquipWeapon(int index)
        {
            if (index == currentWeaponIndex || availableWeapons == null || index < 0 ||
                index >= availableWeapons.Length)
                return;

            EquipWeaponServerRpc(index);
        }

        [ServerRpc(RequireOwnership = false)]
        private void EquipWeaponServerRpc(int index)
        {
            if (isSpawning) return;
            if (availableWeapons == null || index < 0 || index >= availableWeapons.Length ||
                availableWeapons[index]?.weaponPrefab == null)
                return;

            if (weaponHolder == null)
            {
                Debug.LogError("WeaponHolder is not assigned!");
                return;
            }

            isSpawning = true;
            StartCoroutine(EquipWeaponCoroutine(index));
        }

        private IEnumerator EquipWeaponCoroutine(int index)
        {
            if (currentWeapon != null && currentWeapon.NetworkObject != null && currentWeapon.NetworkObject.IsSpawned)
            {
                currentWeapon.NetworkObject.Despawn();
                yield return new WaitUntil(() => !currentWeapon.NetworkObject.IsSpawned);
            }

            currentWeapon = null;

            GameObject weaponInstance = Instantiate(
                availableWeapons[index].weaponPrefab,
                weaponHolder.position,
                weaponHolder.rotation
            );

            NetworkObject networkObject = weaponInstance.GetComponent<NetworkObject>();
            WeaponBehaviour weaponBehaviour = weaponInstance.GetComponent<WeaponBehaviour>();

            if (networkObject.IsSceneObject != null &&
                (networkObject == null || weaponBehaviour == null || (bool)networkObject.IsSceneObject))
            {
                Destroy(weaponInstance);
                isSpawning = false;
                yield break;
            }

            if (!networkObject.IsSpawned)
            {
                networkObject.Spawn(true);
                yield return null;
            }

            weaponInstance.transform.SetParent(transform);
            currentWeapon = weaponBehaviour;
            currentWeaponIndex = index;

            EquipWeaponClientRpc(networkObject.NetworkObjectId);
            isSpawning = false;
        }

        [ClientRpc]
        private void EquipWeaponClientRpc(ulong weaponNetworkId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(weaponNetworkId,
                    out NetworkObject netObj))
            {
                WeaponBehaviour weapon = netObj.GetComponent<WeaponBehaviour>();
                if (weapon != null)
                {
                    currentWeapon = weapon;
                    netObj.transform.position = weaponHolder.position;
                    netObj.transform.SetParent(transform);
                }
            }
        }

        public WeaponBehaviour GetCurrentWeapon()
        {
            return currentWeapon;
        }
    }
}