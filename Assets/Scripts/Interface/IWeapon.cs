using UnityEngine;

namespace Interface
{
    public interface IWeapon
    {
        void Attack();
        Sprite GetIcon(); 
        string GetName();
    }
}
