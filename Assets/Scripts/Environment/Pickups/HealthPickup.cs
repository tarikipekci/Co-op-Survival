using Interface;
using Player;
using UnityEngine;

namespace Environment.Pickups
{
    public class HealthPickup : MonoBehaviour, IPickupEffect
    {
        [SerializeField] private int healAmount = 1;

        public bool Apply(GameObject player)
        {
            PlayerHealth health = player.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                if (health.CurrentHealth >= health.GetMaxHealth()) return false;

                health.IncreaseCurrentHealth(healAmount);
                return true;
            }

            return false;
        }
    }
}