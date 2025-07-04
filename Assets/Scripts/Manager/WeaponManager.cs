using Data;
using Unity.Netcode;
using UnityEngine;
using Weapon;

namespace Manager
{
    public class WeaponManager : NetworkBehaviour
    {
        [SerializeField] public Transform weaponHolderRight;
        [SerializeField] public Transform weaponHolderLeft;
        [SerializeField] private WeaponData[] availableWeapons;

        private WeaponBehaviour currentWeapon;
        private int currentWeaponIndex = -1;

        public override void OnNetworkSpawn()
        {
            if (IsServer && currentWeaponIndex == -1)
            {
                EquipWeaponServerRpc(0);
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
            if (index == currentWeaponIndex) return;
            EquipWeaponServerRpc(index);
        }

        [ServerRpc]
        private void EquipWeaponServerRpc(int index)
        {
            if (index < 0 || index >= availableWeapons.Length) return;
            currentWeaponIndex = index;
            EquipWeaponClientRpc(index);
        }

        [ClientRpc]
        private void EquipWeaponClientRpc(int index)
        {
            if (currentWeapon != null)
                Destroy(currentWeapon.gameObject);

            GameObject weaponInstance = Instantiate(availableWeapons[index].weaponPrefab, weaponHolderRight);
            currentWeapon = weaponInstance.GetComponent<WeaponBehaviour>();
        }

        public WeaponBehaviour GetCurrentWeapon()
        {
            return currentWeapon;
        }
    }
}