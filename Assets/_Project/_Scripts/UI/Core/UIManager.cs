using UnityEngine;
using System.Collections.Generic;

namespace Shield_Shot.UI.Core
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindFirstObjectByType<UIManager>();

                    if (_instance == null)
                    { 
                        GameObject container = new GameObject("UIManager");
                        _instance = container.AddComponent<UIManager>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, IPopup> _popupRegistry = new Dictionary<string, IPopup>();

        private void Awake()
        {
            if(_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterPopup(string key, IPopup popup)
        {
            if(_popupRegistry.ContainsKey(key))
            {
                _popupRegistry[key] = popup;
            }
            else
            {
                _popupRegistry.Add(key, popup);
            }

        }

        public void ShowPopup(string key)
        {
            if(_popupRegistry.TryGetValue(key, out var popup))
            {
                popup.Open();
            }
        }

        public void ClosePopup(string key)
        {
            if (_popupRegistry.TryGetValue(key, out var popup))
            {
                popup.Close();
            }
        }

        public void ClearRegistry()
        {
            _popupRegistry.Clear();
        }
    }
}
