using Interface;
using Player;
using UnityEngine;

namespace Environment.Pickups
{
    public class HealthPickup : MonoBehaviour, IPickupEffect
    {
        [SerializeField] private int healAmount = 1;

        public void Apply(GameObject player)
        {
            PlayerHealth health = player.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                health.IncreaseCurrentHealth(healAmount);
            }
        }
    }
}