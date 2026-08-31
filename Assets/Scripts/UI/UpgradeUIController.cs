using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowFire.Managers;

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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradeChoicesGenerated += DisplayUpgradeChoices;
            }

            if (Container != null) Container.SetActive(false);

            for (int i = 0; i < CardButtons.Length; i++)
            {
                int index = i;
                if (CardButtons[i] != null)
                {
                    CardButtons[i].onClick.AddListener(() => OnCardClicked(index));
                }
            }
        }

        public void DisplayUpgradeChoices(List<UpgradeCardData> choices)
        {
            _currentCards = choices;
            if (Container != null) Container.SetActive(true);

            for (int i = 0; i < CardButtons.Length; i++)
            {
                if (i < choices.Count)
                {
                    CardButtons[i].gameObject.SetActive(true);
                    if (CardTitles[i] != null) CardTitles[i].text = choices[i].Title;
                    if (CardDescriptions[i] != null) CardDescriptions[i].text = choices[i].Description;
                }
                else
                {
                    CardButtons[i].gameObject.SetActive(false);
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
