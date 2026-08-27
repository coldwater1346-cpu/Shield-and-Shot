using Shield_Shot.GameplayCore.Monster.BT.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT.Traits
{
    public abstract class BtTraitSO : ScriptableObject
    {
        public abstract BtNode<BtContext> CreateNode();
    }
}