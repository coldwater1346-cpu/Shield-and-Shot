using Shield_Shot.GameplayCore.Field;
using UnityEngine;

public class SettingPos : MonoBehaviour
{
    [SerializeField] private ArenaSpawnPointProvider _arenaSpawnPointProvider;
    [SerializeField, Min(0)] private int _lineCellY = 3; // DeadLine이 놓일 세로 셀
    [SerializeField] private bool _applyOnStart = true;

    private void Start()
    {
        if (_applyOnStart) ApplyLineHeight();
    }

    [ContextMenu("Apply Line Height")]
    public void ApplyLineHeight()
    {
        // 가로(x)는 의미 없으니 현재 x 셀 아무거나 써서 세로 높이만 구함
        Vector2Int cell = new Vector2Int(0, _lineCellY);

        // 1순위: Provider (지형 높이 스냅까지 처리)
        if (_arenaSpawnPointProvider != null &&
            _arenaSpawnPointProvider.TryGetCellWorldPose(cell, out Pose pose))
        {
            Vector3 p = transform.position;
            p.z = pose.position.z;   // 세로(라인 높이)만 셀 기준으로
            transform.position = p;
            return;
        }

        // 폴백: 그리드 직접 사용
        var grid = ElementFieldGrid.Instance;
        if (grid != null && grid.TryCellToWorld(grid.ClampCell(cell), out Vector3 world))
        {
            Vector3 p = transform.position;
            p.z = world.z;
            transform.position = p;
        }
    }
}