using Shield_Shot.GameplayCore.Monster.BT.Core;
using Shield_Shot.GameplayCore.Monster.Core;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.BT
{
    public class BtRunner : MonoBehaviour
    {
        private BtNode<BtContext> _root;
        private BtContext _ctx;

        public void Initialize(MonsterBase monster, BtNode<BtContext> root)
        {
            _root = root;
            _root?.OnReset();
            _ctx ??= new BtContext(monster);   // 스폰마다 재생성하지 않음
        }


        private void Update()
        {
            _root?.Execute(_ctx);
        }

        private void OnDisable()
        {
            // OnReset은 Initialize(재활성화 시)에서 처리.
            // 여기서 호출하면 DieAction 실행 중 SetActive(false)가 동기적으로
            // 트리를 리셋해 DeadAnimNode가 루프에 빠지는 문제 발생.
        }
    }
}