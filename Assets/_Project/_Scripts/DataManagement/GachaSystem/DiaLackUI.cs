using UnityEngine;
using UnityEngine.UI;

public class DiaLackUI : MonoBehaviour
{



    [SerializeField] private GameObject _diaLackPopUI;
    [SerializeField] private Button _closeButton;



    private void Awake()
    {

        _closeButton.onClick.AddListener(Close);

    }
    private void Start()
    {
        _diaLackPopUI.SetActive(false);
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveListener(Close);
    }


    public void Open()
    {
        _diaLackPopUI.SetActive(true);
    }

    public void Close()
    {
        _diaLackPopUI.SetActive(false);
    }


}
