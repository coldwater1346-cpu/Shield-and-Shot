using UnityEngine;
using System.Collections.Generic;

namespace Shield_Shot.UI
{
    public class HpBarManager : MonoBehaviour
    {
        public static HpBarManager Instance { get; private set; }

        [SerializeField] private GameObject _hpBarPrefab;
        [SerializeField] private Transform _hpBarContainer;
        [SerializeField] private int _poolSize = 50;

        private List<MonsterHPBarUI> _hpBarPool = new List<MonsterHPBarUI>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            PreWarmPool();
        }

        public MonsterHPBarUI RequestHPBar(Transform monsterTransform, int maxHp, Vector3 offset)
        {
            MonsterHPBarUI hpBar = GetPooledHPBar();

            if(_hpBarContainer != null)
            {
                hpBar.transform.SetParent(_hpBarContainer, false);
            }

            hpBar.SetupHpBar(monsterTransform, maxHp, offset);
            return hpBar;
        }

        private MonsterHPBarUI GetPooledHPBar()
        { 
            foreach(var bar in _hpBarPool)
            {
                if(!bar.gameObject.activeInHierarchy)
                {
                    return bar;
                }
            }

            GameObject go = Instantiate(_hpBarPrefab);
            MonsterHPBarUI newbar = go.GetComponent<MonsterHPBarUI>();
            _hpBarPool.Add(newbar);
        
            return newbar;
        }

        private void PreWarmPool()
        {
            for(int i = 0; i < _poolSize; i++)
            {
                GameObject go = Instantiate(_hpBarPrefab);
                MonsterHPBarUI newbar = go.GetComponent<MonsterHPBarUI>();

                if(_hpBarContainer != null)
                {
                    go.transform.SetParent(_hpBarContainer, false);
                }

                go.SetActive(false);
                _hpBarPool.Add(newbar);
            }
            Debug.Log($"[HpBarManager] 체력바 {_poolSize}개 사전 생성");
        }
    }
}

