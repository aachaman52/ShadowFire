using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Core;
using ShadowFire.Managers;
using ShadowFire.Player;

namespace ShadowFire.UI
{
    public class UpgradeUIController : MonoBehaviour
    {
        public static UpgradeUIController Instance { get; private set; }

        [Header("Card UI Elements (3 Cards)")]
        public GameObject Container;
        public Button[] CardButtons;
        public TextMeshProUGUI[] CardTitles;
        public TextMeshProUGUI[] CardDescriptions;

        private List<UpgradeCardData> _currentCards;
        private bool _subscribed = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            Subscribe();

            if (Container != null) Container.SetActive(false);

            if (CardButtons != null)
            {
                for (int i = 0; i < CardButtons.Length; i++)
                {
                    int index = i;
                    if (CardButtons[i] != null)
                    {
                        CardButtons[i].onClick.AddListener(() => OnCardClicked(index));
                    }
                }
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_subscribed) return;

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradeChoicesGenerated += DisplayUpgradeChoices;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }

            _subscribed = true;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                if (Container != null) Container.SetActive(false);
            }
        }

        public void DisplayUpgradeChoices(List<UpgradeCardData> choices)
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.GameOver) return;
            if (PlayerStats.Instance != null && !PlayerStats.Instance.IsAlive) return;

            _currentCards = choices;
            if (Container != null) Container.SetActive(true);

            if (CardButtons != null)
            {
                for (int i = 0; i < CardButtons.Length; i++)
                {
                    if (CardButtons[i] == null) continue;

                    if (i < choices.Count)
                    {
                        CardButtons[i].gameObject.SetActive(true);
                        if (CardTitles != null && i < CardTitles.Length && CardTitles[i] != null)
                            CardTitles[i].text = choices[i].Title;
                        if (CardDescriptions != null && i < CardDescriptions.Length && CardDescriptions[i] != null)
                            CardDescriptions[i].text = choices[i].Description;
                    }
                    else
                    {
                        CardButtons[i].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnCardClicked(int index)
        {
            if (_currentCards != null && index < _currentCards.Count)
            {
                if (Container != null) Container.SetActive(false);
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.SelectUpgrade(_currentCards[index].Type);
                }
            }
        }
    }
}
