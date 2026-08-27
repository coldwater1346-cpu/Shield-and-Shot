using UnityEngine;

namespace Shield_Shot.UI
{
    public class PurchasePackage : MonoBehaviour
    {
        public void OnClickPurchase()
        {
            Debug.Log("패키기 구매 완료");

            //TODO: 실제 결제 연동

            gameObject.SetActive(false);
        }

        public void OnClickClosed()
        {
            gameObject.SetActive(false);
        }
    }
}
