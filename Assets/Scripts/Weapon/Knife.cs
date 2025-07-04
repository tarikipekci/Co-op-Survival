using UnityEngine;

namespace Weapon
{
    public class Knife : WeaponBehaviour
    {
        public override void Attack()
        {
            Debug.Log("Knife Slash!");
        }
    }
}