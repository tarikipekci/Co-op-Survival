using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Weapons/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        public Sprite icon;
        public GameObject weaponPrefab;
    }
}
