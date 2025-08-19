using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerData : NetworkBehaviour
    {
        public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(3);
        public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(2f);
        public NetworkVariable<int> Damage = new NetworkVariable<int>(1);
        public NetworkVariable<float> AttackRate = new NetworkVariable<float>(1f);
    }
}
