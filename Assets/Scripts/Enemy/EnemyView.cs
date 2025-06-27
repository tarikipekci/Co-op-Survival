using UnityEngine;

namespace Enemy
{
    public class EnemyView : MonoBehaviour
    {
        public void PlayHitEffect()
        {
        }

        public void Die()
        {
            Destroy(gameObject);
        }
    }
}
