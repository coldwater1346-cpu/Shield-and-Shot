using UnityEngine;
using Shield_Shot.GameplayCore.Weapon.Projectile;

namespace Shield_Shot.GameplayCore.Weapon.Modifier
{
    /// <summary>
    /// 반사 직후 주변의 가장 가까운 적을 찾아 방향을 유도 보정하는 부가 효과입니다.
    /// </summary>
    public class HomingReflectEffect : IReflectEffect
    {
        private readonly float searchRadius;
        private readonly float homingStrength; // 0 ~ 1 (1에 가까울수록 완벽하게 적을 조준, 낮을수록 난반사 궤적 유지)
        private readonly LayerMask enemyLayer;

        public HomingReflectEffect(float radius = 10f, float strength = 0.6f, int enemyLayerValue = 1 << 7)
        {
            this.searchRadius = radius;
            this.homingStrength = Mathf.Clamp01(strength);
            this.enemyLayer = enemyLayerValue;
        }

        public IReflectEffect Clone()
        {
            return new HomingReflectEffect(searchRadius, homingStrength, enemyLayer.value);
        }

        public void Execute(ProjectileBase projectile, Vector3 reflectDirection, Collider wallCollider)
        {
            // 1. 지정된 반경 내의 모든 적 충돌체 검색
            Collider[] targets = Physics.OverlapSphere(projectile.transform.position, searchRadius, enemyLayer);
            if (targets.Length == 0) return; // 주변에 적이 없다면 난반사/정반사 방향 그대로 날아감

            // 2. 검색된 적들 중 가장 가까운 타겟 선별
            Collider closestEnemy = null;
            float minDistance = float.MaxValue;
            Vector3 currentPos = projectile.transform.position;

            foreach (var target in targets)
            {
                float dist = (target.transform.position - currentPos).sqrMagnitude;
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestEnemy = target;
                }
            }

            // 3. 타겟이 존재하면 방향 보정 연산 수행
            if (closestEnemy != null)
            {
                // 적을 향하는 조준 벡터 생성 (탑다운 프로젝트 특성에 맞춰 Y축은 격리)
                Vector3 toEnemy = closestEnemy.transform.position - currentPos;
                toEnemy.y = 0f;
                Vector3 homingDir = toEnemy.normalized;

                // [핵심] 기존 반사 방향(reflectDirection)과 유도 방향(homingDir)을 강도에 따라 블렌딩
                // 이렇게 하면 난반사의 무작위성(삐딱하게 튕김)과 유도 성능이 연출적으로 절묘하게 섞입니다.
                Vector3 finalDir = Vector3.Lerp(reflectDirection, homingDir, homingStrength).normalized;

                // 최종 계산된 보정 방향을 투사체에 주입
                //projectile.SetDirection(finalDir);

                Debug.Log($"[HomingReflect] 유도 보정 발동 -> 타겟: {closestEnemy.name} | 방향: {finalDir}");
            }
        }
    }
}