using UnityEngine;


public class ItemPriceData
{
    public int ItemEnhanceLevel;
    public int c;
    public int uc;
    public int rare;
    public int sr;
    public int ssr;
    public int ur;

   
    public int GetPriceByGrade(ItemGradeType gradeType)
    {
        switch (gradeType)
        {
            case ItemGradeType.C: return c;
            case ItemGradeType.UC: return uc;
            case ItemGradeType.Rare: return rare;
            case ItemGradeType.SR: return sr;
            case ItemGradeType.SSR: return ssr;
            case ItemGradeType.UR: return ur;
            default:
                UnityEngine.Debug.LogWarning($"[ItemPriceData] 없는 등급 : {gradeType}");
                return 0;
        }
    }
}