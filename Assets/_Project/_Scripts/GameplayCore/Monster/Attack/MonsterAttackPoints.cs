using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Attack
{
    public class MonsterAttackPoints : MonoBehaviour
    {
        [SerializeField] private Transform _front;
        [SerializeField] private Transform _left;
        [SerializeField] private Transform _right;

        public Transform Front => _front;
        public Transform Left => _left;
        public Transform Right => _right;

        public enum AttackOrigin 
        { 
            FRONT,
            LEFT,
            RIGHT,
            SIDES,
            ALL 
        }

        public Transform[] GetPoints(AttackOrigin origin) => origin switch
        {
            AttackOrigin.FRONT => new[] { _front },
            AttackOrigin.LEFT => new[] { _left },
            AttackOrigin.RIGHT => new[] { _right },
            AttackOrigin.SIDES => new[] { _left, _right },
            AttackOrigin.ALL => new[] { _front, _left, _right },
            _ => new[] { _front }
        };
    }


}