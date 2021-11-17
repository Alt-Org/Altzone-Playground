using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/Shield Model")]
    public class ShieldModel : ScriptableObject
    {
        [Min(1)]
        public float MovementSpeed;
        [Min(1)]
        public float HitResistance;
        [Min(1)]
        public float AttackSpeed;
        [Min(1)]
        public float ActivationDistance;
        [TextArea(10, 20)]
        public string CharacterPersonality;
    }
}