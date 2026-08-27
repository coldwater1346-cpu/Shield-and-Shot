using UnityEngine;

namespace Shield_Shot.GameplayCore.Weapon.Modifier
{
    public class DiffuseReflection : IReflectionBehavior
    {
        private readonly float maxSpreadAngle;

        public DiffuseReflection(float maxSpreadAngle = 30f)
        {
            this.maxSpreadAngle = maxSpreadAngle;
        }

        public IReflectionBehavior Clone() => new DiffuseReflection(maxSpreadAngle);

        public Vector3 CalculateDirection(Vector3 incomingDirection, Vector3 surfaceNormal)
        {
            // 1. 기준이 되는 기본 정반사 방향을 먼저 구합니다.
            Vector3 regularReflectDir = Vector3.Reflect(incomingDirection, surfaceNormal);
            regularReflectDir.y = 0f; // 평면 정렬
            regularReflectDir.Normalize();

            // 2. 기획자가 지정한 최대 각도 범위 내에서 무작위 노이즈 각도를 뽑습니다.
            // 예: maxSpreadAngle이 30이면 -15도 ~ +15도 사이의 랜덤 스프레드 발생
            float randomOffset = Random.Range(-maxSpreadAngle * 0.5f, maxSpreadAngle * 0.5f);

            // 3. [핵심] 탑다운 평면이므로 'Y축(Vector3.up)'을 기준으로 회전 쿼터니언을 만듭니다.
            Quaternion randomRotation = Quaternion.Euler(0f, randomOffset, 0f);

            // 4. 정반사 방향에 랜덤 회전값을 곱해 최종 난반사 방향을 계산합니다.
            Vector3 diffuseDir = randomRotation * regularReflectDir;

            // 디버그 로그로 실제 각도가 틀어지는지 눈으로 확인
            Debug.Log($"[Diffuse] 정반사각: {regularReflectDir} -> 난반사 오프셋: {randomOffset}° -> 최종각: {diffuseDir}");

            return diffuseDir.normalized;
        }
    }
}