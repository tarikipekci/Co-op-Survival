using UnityEngine;

namespace Weapon
{
    public class Knife : WeaponBehaviour
    {
        public override void Attack()
        {
            Debug.Log("Knife Slash!");
            if (animator != null)
                animator.SetTrigger(AttackTrigger);
        }
    }
}