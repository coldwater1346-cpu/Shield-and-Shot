using UnityEngine;
using UnityEngine.UI;
public class SignoffPanelController : MonoBehaviour
{
    [SerializeField] private Button _signoffBtn;

    [SerializeField] private GameObject _signoffPanel;




    private void Awake()
    {
        _signoffBtn.onClick.AddListener(() => ShowSignoffPanel());
    }

    private void ShowSignoffPanel()
    {

        _signoffPanel.SetActive(true);
        
    }
}