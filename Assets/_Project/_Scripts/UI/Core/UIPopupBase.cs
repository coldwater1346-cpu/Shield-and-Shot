using UnityEngine;

namespace Shield_Shot.UI.Core
{
    public class UIPopupBase : MonoBehaviour, IPopup
    {
        public virtual void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}

