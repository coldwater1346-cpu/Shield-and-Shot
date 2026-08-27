using UnityEngine;
using Shield_Shot.InputSystem.Data;

namespace Shield_Shot.GameplayCore.Weapon.Core
{
	public interface IWeaponStrategy
	{
        // 무기 데이터를 기반으로 초기화
        void Initialize(/*WeaponData data*/);
        // 입력이 유지되는 동안 매 프레임 호출
        void HandleInputStay(InputContext ctx);
        // 입력이 해제될 때 한 번 호출
        void HandleInputUp(InputContext ctx);
        // 무기 비활성화 시 호출
        void Deactivate();
    }
}