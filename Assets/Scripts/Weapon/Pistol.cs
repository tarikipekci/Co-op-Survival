using UnityEngine;

namespace Weapon
{
    public class Pistol : WeaponBehaviour
    {
        public override void Attack()
        {
            Debug.Log("Pistol Fire!");
        }
    }
}