using Shield_Shot.Core;
using Shield_Shot.GameplayCore.Weapon.Projectile;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    /// 플레이어 부활: 광고 시청 → HP 리셋 + 무적 + 타임스케일 복구.
    public class PlayerReviveController : MonoBehaviour
    {
        [SerializeField] private float _reviveInvincibleTime = 3f;

        // 부활 광고 버튼 OnClick에 연결
        public void OnClickReviveAd() => AdManager.Instance.ShowReviveAd(Revive);

        public void Revive()
        {
            if (LocalPlayerStatusContext.TryGet(out PlayerStatus ps))
            {
                ps.ResetHP();
                ps.InvincibleTimer = _reviveInvincibleTime;
            }
            Time.timeScale = 1f;
        }
    }
}