using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Data;
using VARG.Input;

namespace VARG.UI
{
    public class TargetShowcaseUI : MonoBehaviour
    {
        [Header("Showcase Configuration")]
        [SerializeField] private Transform _showcaseAnchor;
        [SerializeField] private float _modelScale = 2.0f;
        [SerializeField] private float _modelSpacing = 2.5f;

        [Header("Timer & Controls")]
        [SerializeField] private GameObject _timerPanel;
        [SerializeField] private TextMeshProUGUI _timerCountdownText;
        [SerializeField] private Image _timerFillBar;
        [SerializeField] private Button _confirmButton;

        private readonly List<GameObject> _spawnedObjects = new();
        private Coroutine _countdownRoutine;
        private Action _onFinished;
        private bool _isCompleted;

        private void Awake()
        {
            ClearShowcase();
            if (_confirmButton != null) _confirmButton.onClick.AddListener(ConfirmAndProceed);
        }

        public void Configure(GameObject timerPanel, TextMeshProUGUI countdownText, Image fillBar, Button confirmButton, Transform showcaseAnchor = null)
        {
            _timerPanel = timerPanel;
            _timerCountdownText = countdownText;
            _timerFillBar = fillBar;
            _confirmButton = confirmButton;
            _showcaseAnchor = showcaseAnchor;
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || _isCompleted || _countdownRoutine == null) return;
            if (InputManager.HasInstance && InputManager.Instance.Player != null &&
                (InputManager.Instance.Player.InteractPressed || InputManager.Instance.Player.FirePressed))
            {
                ConfirmAndProceed();
            }
        }

        public void StartCountdown(float duration, Action onCompleted)
        {
            _isCompleted = false;
            _onFinished = onCompleted;
            if (_timerPanel != null) _timerPanel.SetActive(true);
            if (_countdownRoutine != null) StopCoroutine(_countdownRoutine);
            _countdownRoutine = StartCoroutine(CountdownRoutine(duration));
        }

        public void DisplayShowcase(IReadOnlyList<TargetData> order)
        {
            _isCompleted = false;
            gameObject.SetActive(true);
            ClearShowcase();
            if (order != null) SpawnTargets(order);
        }

        public void Populate(IReadOnlyList<TargetData> order, float memoryDuration, Action onCompleted)
        {
            DisplayShowcase(order);
            StartCountdown(memoryDuration, onCompleted);
        }

        public void HideShowcase()
        {
            if (_countdownRoutine != null) StopCoroutine(_countdownRoutine);
            if (_timerPanel != null) _timerPanel.SetActive(false);
            ClearShowcase();
            gameObject.SetActive(false);
        }

        public void ConfirmAndProceed()
        {
            if (_isCompleted) return;
            _isCompleted = true;
            if (_countdownRoutine != null) StopCoroutine(_countdownRoutine);
            if (_timerPanel != null) _timerPanel.SetActive(false);
            ClearShowcase();

            var cb = _onFinished;
            _onFinished = null;
            cb?.Invoke();
        }

        private void ClearShowcase()
        {
            for (var i = 0; i < _spawnedObjects.Count; i++)
            {
                if (_spawnedObjects[i] != null) Destroy(_spawnedObjects[i]);
            }
            _spawnedObjects.Clear();
        }

        private void SpawnTargets(IReadOnlyList<TargetData> order)
        {
            var count = order.Count;
            if (count == 0) return;

            var spacing = count <= 1 ? 0f : Mathf.Clamp(7.0f / (count - 1), 1.6f, _modelSpacing);
            var startX = -(count - 1) * 0.5f * spacing;
            var anchor = _showcaseAnchor != null ? _showcaseAnchor.position : new Vector3(0.5f, 1.4f, 0f);

            for (var i = 0; i < count; i++)
            {
                var target = order[i];
                if (target == null) continue;

                var prefab = target.MiniPrefab != null ? target.MiniPrefab : target.Prefab;
                if (prefab == null) continue;

                var pos = anchor + new Vector3(startX + i * spacing, 0f, 0f);
                var instance = Instantiate(prefab, pos, Quaternion.identity, transform);
                instance.transform.localScale = Vector3.one * _modelScale;

                var rb = instance.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                var colliders = instance.GetComponentsInChildren<Collider>();
                for (var c = 0; c < colliders.Length; c++) colliders[c].enabled = false;

                _spawnedObjects.Add(instance);
            }
        }

        private IEnumerator CountdownRoutine(float duration)
        {
            var remaining = Mathf.Max(1f, duration);
            var total = remaining;

            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                if (_timerFillBar != null) _timerFillBar.fillAmount = Mathf.Clamp01(remaining / total);
                if (_timerCountdownText != null) _timerCountdownText.text = $"MEMORY TIME: {Mathf.CeilToInt(remaining):00}s";
                yield return null;
            }

            ConfirmAndProceed();
        }
    }
}
