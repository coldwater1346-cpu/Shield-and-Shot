using TMPro;
using Shield_Shot.GameplayCore.Network.Match;
using UnityEngine;
using UnityEngine.UI;

namespace Shield_Shot.GameplayCore.Network.Pvp
{
    public sealed class PvpBattleHudPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PvpMatchStateController _matchStateController;
        [SerializeField] private PvpLocalPlayerSideProvider _localPlayerSideProvider;

        [Header("Score")]
        [SerializeField] private TMP_Text _bottomScoreText;
        [SerializeField] private TMP_Text _topScoreText;

        [Header("Health")]
        [SerializeField] private Slider _bottomHpSlider;
        [SerializeField] private Slider _topHpSlider;

        [Header("Center State")]
        [SerializeField] private GameObject _centerStateRoot;
        [SerializeField] private TMP_Text _centerStateText;

        private PvpWeaponHealth _localHealth;
        private PvpWeaponHealth _opponentHealth;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            ResolveReferences();

            if (_matchStateController == null)
            {
                return;
            }

            if (!CanReadMatchState())
            {
                return;
            }

            UpdateScore();
            UpdateHealth();
            UpdateCenterState();
        }

        private void ResolveReferences()
        {
            if (_matchStateController == null)
            {
                _matchStateController = FindFirstObjectByType<PvpMatchStateController>();
            }

            if (_localPlayerSideProvider == null)
            {
                _localPlayerSideProvider = FindFirstObjectByType<PvpLocalPlayerSideProvider>();
            }

            if (_localHealth != null && _opponentHealth != null)
            {
                return;
            }

            if (_localPlayerSideProvider == null ||
                !_localPlayerSideProvider.TryGetLocalSide(out PlayerSide localSide))
            {
                return;
            }

            PvpWeaponHealth[] healths = FindObjectsByType<PvpWeaponHealth>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            for (int i = 0; i < healths.Length; i++)
            {
                PvpWeaponHealth health = healths[i];
                if (health == null)
                {
                    continue;
                }

                PvpWeaponActorIdentity identity = health.GetComponent<PvpWeaponActorIdentity>();
                if (identity == null)
                {
                    continue;
                }

                if (identity.Side == localSide)
                {
                    _localHealth = health;
                }
                else
                {
                    _opponentHealth = health;
                }
            }
        }

        private void UpdateScore()
        {
            if (!TryGetLocalSide(out PlayerSide localSide))
            {
                UpdateText(_bottomScoreText, _matchStateController.BottomScore);
                UpdateText(_topScoreText, _matchStateController.TopScore);
                return;
            }

            int localScore = localSide == PlayerSide.Bottom
                ? _matchStateController.BottomScore
                : _matchStateController.TopScore;

            int opponentScore = localSide == PlayerSide.Bottom
                ? _matchStateController.TopScore
                : _matchStateController.BottomScore;

            UpdateText(_bottomScoreText, localScore);
            UpdateText(_topScoreText, opponentScore);
        }

        private bool CanReadMatchState()
        {
            return _matchStateController != null &&
                   _matchStateController.Object != null &&
                   _matchStateController.Object.IsValid;
        }

        private static void UpdateText(TMP_Text text, int value)
        {
            if (text != null)
            {
                text.text = $"{value}";
            }
        }

        private bool TryGetLocalSide(out PlayerSide localSide)
        {
            localSide = PlayerSide.Bottom;

            if (_localPlayerSideProvider == null)
            {
                _localPlayerSideProvider = FindFirstObjectByType<PvpLocalPlayerSideProvider>();
            }

            return _localPlayerSideProvider != null &&
                   _localPlayerSideProvider.TryGetLocalSide(out localSide);
        }

        private void UpdateHealth()
        {
            UpdateHealthSlider(_bottomHpSlider, _localHealth);
            UpdateHealthSlider(_topHpSlider, _opponentHealth);
        }

        private static void UpdateHealthSlider(Slider slider, PvpWeaponHealth health)
        {
            if (slider == null || health == null)
            {
                return;
            }

            slider.value = health.HealthRatio;
        }

        private void UpdateCenterState()
        {
            if (_centerStateRoot == null || _centerStateText == null)
            {
                return;
            }

            PvpMatchState state = _matchStateController.CurrentState;

            if (state == PvpMatchState.Fighting)
            {
                _centerStateRoot.SetActive(false);
                return;
            }

            _centerStateRoot.SetActive(true);

            switch (state)
            {
                case PvpMatchState.WaitingForPlayers:
                    _centerStateText.text = "상대를 기다리는 중...";
                    break;

                case PvpMatchState.Countdown:
                    int remain = Mathf.CeilToInt(_matchStateController.CountdownRemaining);
                    _centerStateText.text = remain > 0 ? remain.ToString() : "START";
                    break;

                case PvpMatchState.RoundEnded:
                    _centerStateText.text = "라운드 종료";
                    break;

                case PvpMatchState.AugmentSelection:
                    _centerStateText.text = "증강 선택";
                    break;

                case PvpMatchState.MatchEnded:
                    _centerStateText.text = "전투 종료";
                    break;

                case PvpMatchState.ReturningToLobby:
                    _centerStateText.text = "로비로 돌아가는 중...";
                    break;

                default:
                    _centerStateText.text = string.Empty;
                    break;
            }
        }
    }
}


