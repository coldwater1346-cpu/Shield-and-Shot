using System.Collections.Generic;
using Shield_Shot.GameplayCore.Monster.Wave;
using UnityEngine;

namespace Shield_Shot.GameplayCore.Monster.Stage
{
    [CreateAssetMenu(fileName = "StageSO", menuName = "Monster/Stage")]
    public class StageSO : ScriptableObject
    {
        [SerializeField] private List<WaveSO> _waves;
        public IReadOnlyList<WaveSO> Waves => _waves;

        public void Initialize(List<WaveSO> waves)
        {
            _waves = waves;
        }
    }
}