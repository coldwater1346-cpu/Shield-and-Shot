using Shield_Shot.GameplayCore.Monster.Core;
using System.Collections;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Attack
{
    public interface IAttackBehavior
    {
        IEnumerator AttackRoutine(MonsterBase monster);
    }
}
