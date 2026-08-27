using Shield_Shot.DataManagement.DataParsing;
using UnityEngine;
using BackEnd;

namespace Shield_Shot.DataManagement.Login
{
public class ServerChartLoader
{
    public void LoadAll()
    {
        var itemLoader = ItemDataParsingManager.Instance;

        itemLoader.LoadWeaponTableFromServer("248748");
        itemLoader.LoadShieldTableFromServer("248748");
        itemLoader.LoadEnhanceCostTableFromServer("245979");
        itemLoader.LoadItemPriceTableFromServer("246710");
        itemLoader.LoadItemCombineTableFromServer("246759");
        itemLoader.LoadPropertyRateTableFromServer("246943");

        //MonsterDataParsingManager.Instance.LoadMonsterTableFromServer("246477");

       
        //StageDataParsingManager.Instance.LoadStageWaveTableFromServer("246469","246479", "246480", "246481","246471");
    }
  }
}