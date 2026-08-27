using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Core
{
    public class AnimComponent : MonoBehaviour
    {
        private static readonly int MoveHash = Animator.StringToHash("Move");
        private static readonly int DeadHash = Animator.StringToHash("Dead");
        private static readonly int ResurrectHash = Animator.StringToHash("Resurrect");
        private static readonly int SplitHash = Animator.StringToHash("Split");

        private Animator _anim;
        private Rigidbody _rb;

        [SerializeField] private float _moveThreshold = 0.1f;

        public bool HasAnimator => _anim != null;
        public bool IsInTransition(int layer = 0) => _anim != null && _anim.IsInTransition(layer);
        public int CurrentStateHash(int layer = 0)
            => _anim != null ? _anim.GetCurrentAnimatorStateInfo(layer).shortNameHash : 0;
        public float CurrentNormalizedTime(int layer = 0)
            => _anim != null ? _anim.GetCurrentAnimatorStateInfo(layer).normalizedTime : 1f;


        public void TriggerDead() => _anim?.SetTrigger(DeadHash);
        public void TriggerResurrect() => _anim?.SetTrigger(ResurrectHash);
        public void TriggerSplit() => _anim?.SetTrigger(SplitHash);
        public void PlayTrigger(int hash) => _anim?.SetTrigger(hash); // 노드에서 범용 호출

        public void ResetAnim()
        {
            if (_anim == null) return;
            _anim.Rebind();
            _anim.ResetTrigger(DeadHash);
            _anim.ResetTrigger(ResurrectHash);
            _anim.ResetTrigger(SplitHash);
            _anim.Update(0f);
        }

        private void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            bool moving = _rb.linearVelocity.sqrMagnitude > _moveThreshold * _moveThreshold;
            _anim.SetBool(MoveHash, moving);
        }

        private void OnDisable()
        {
            _anim?.SetBool(MoveHash, false); // 풀 반환 시 Move 초기화
        }
    }
}