using UnityEngine;
using System.Collections.Generic;


public class RuntimeStageData
{
    public List<WaveSO> Waves { get; private set; }

    public RuntimeStageData(List<WaveSO> waves)
    {
        Waves = waves;
    }
}
