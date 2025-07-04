using Data;
using Interface;
using UnityEngine;

namespace Weapon
{
    public abstract class WeaponBehaviour : MonoBehaviour, IWeapon
    {
        [SerializeField] protected WeaponData weaponData;

        public virtual void Attack()
        {
            Debug.Log("Base attack");
        }

        public Sprite GetIcon() => weaponData.icon;
        public string GetName() => weaponData.weaponName;
    }
}
