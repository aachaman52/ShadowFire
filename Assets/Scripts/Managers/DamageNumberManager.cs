using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ShadowFire.Player;

namespace ShadowFire.Managers
{
    public class DamageNumberManager : MonoBehaviour
    {
        public static DamageNumberManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        public void ShowDamageNumber(float amount, Vector3 position, bool isCritical = false)
        {
            GameObject numObj = new GameObject("DamageNumber");
            numObj.transform.position = position + UnityEngine.Random.insideUnitSphere * 0.25f;

            TextMeshPro text = numObj.AddComponent<TextMeshPro>();
            text.text = Mathf.RoundToInt(amount).ToString();
            text.fontSize = isCritical ? 7f : 5f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = isCritical ? new Color(1f, 0.85f, 0.1f) : new Color(1f, 0.3f, 0.2f);

            var floater = numObj.AddComponent<FloatingDamageNumber>();
            floater.Initialize(isCritical);
        }
    }

    public class FloatingDamageNumber : MonoBehaviour
    {
        private TextMeshPro _tmp;
        private Camera _mainCam;
        private float _lifetime = 0.75f;
        private float _timer = 0;
        private Vector3 _velocity;

        public void Initialize(bool isCritical)
        {
            _tmp = GetComponent<TextMeshPro>();
            _mainCam = Camera.main;
            _velocity = new Vector3(UnityEngine.Random.Range(-0.8f, 0.8f), isCritical ? 3.2f : 2.2f, UnityEngine.Random.Range(-0.8f, 0.8f));
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += _velocity * Time.deltaTime;

            if (_mainCam != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _mainCam.transform.position);
            }

            if (_tmp != null)
            {
                Color c = _tmp.color;
                c.a = Mathf.Clamp01(1f - (_timer / _lifetime));
                _tmp.color = c;
            }

            if (_timer >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
