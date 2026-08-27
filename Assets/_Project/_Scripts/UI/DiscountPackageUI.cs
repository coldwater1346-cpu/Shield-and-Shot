using UnityEngine;

namespace Shield_Shot.UI
{
    public class DiscountPackageUI : MonoBehaviour
    {
        [SerializeField] private GameObject _bigSalePackagePanel;

        void Start()
        {
            if(PlayerPrefs.GetInt("LastStageStatus", 1) == 0)
            {
                _bigSalePackagePanel.SetActive(true);

                PlayerPrefs.SetInt("LastStageStatus", 1);
            }
            else
            {
                _bigSalePackagePanel.SetActive(false);
            }
        }

        public void TestDiscountPackage()
        {
            Debug.Log("스테이지 실패");

            PlayerPrefs.SetInt("LastStageStatus", 0);

            _bigSalePackagePanel.SetActive(true);
        }
    }
}
