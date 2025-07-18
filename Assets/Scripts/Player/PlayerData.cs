using Unity.Netcode;

namespace Player
{
    public class PlayerData : NetworkBehaviour
    {
        public NetworkVariable<int> Level = new NetworkVariable<int>(1);
        public NetworkVariable<int> Experience = new NetworkVariable<int>();
        public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(3);
        public NetworkVariable<float> MoveSpeed = new NetworkVariable<float>(2);
    }
}
