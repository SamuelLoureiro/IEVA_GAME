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
        [Header("Header Elements")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _subtitleText;

        [Header("Showcase Configuration")]
        [SerializeField] private Transform _showcaseAnchor;
        [SerializeField] private Vector3 _defaultAnchorPosition = new(0.5f, 1.4f, 0f);
        [SerializeField] private float _modelScale = 2.0f;
        [SerializeField] private float _modelSpacing = 2.5f;

        [Header("Timer & Controls")]
        [SerializeField] private GameObject _timerPanel;
        [SerializeField] private TextMeshProUGUI _timerCountdownText;
        [SerializeField] private Image _timerFillBar;
        [SerializeField] private Button _confirmButton;

        private readonly List<GameObject> _spawnedShowcaseObjects = new();
        private Coroutine _countdownCoroutine;
        private Action _onMemorizationFinished;
        private bool _isCompleted;

        private void Awake()
        {
            ClearShowcase();

            if (_timerPanel != null)
            {
                _timerPanel.SetActive(false);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.AddListener(ConfirmAndProceed);
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || _isCompleted || _countdownCoroutine == null) return;

            if (InputManager.HasInstance &&
                InputManager.Instance.Player != null &&
                (InputManager.Instance.Player.InteractPressed || InputManager.Instance.Player.FirePressed))
            {
                ConfirmAndProceed();
            }
        }

        public void StartCountdown(float duration, Action onCompleted)
        {
            _isCompleted = false;
            _onMemorizationFinished = onCompleted;

            if (_timerPanel != null)
            {
                _timerPanel.SetActive(true);
            }

            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
            }

            _countdownCoroutine = StartCoroutine(MemoryCountdownRoutine(duration));
        }

        public void DisplayShowcase(IReadOnlyList<TargetData> order)
        {
            _isCompleted = false;
            gameObject.SetActive(true);
            ClearShowcase();

            if (order != null && order.Count > 0)
            {
                SpawnShowcaseTargets(order);
            }
        }

        public void HideShowcase()
        {
            StopCountdownAndClearUI();
            gameObject.SetActive(false);
        }

        public void ConfirmAndProceed()
        {
            if (_isCompleted) return;
            _isCompleted = true;

            StopCountdownAndClearUI();

            var callback = _onMemorizationFinished;
            _onMemorizationFinished = null;
            callback?.Invoke();
        }

        private void StopCountdownAndClearUI()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            if (_timerPanel != null) _timerPanel.SetActive(false);
            ClearShowcase();
        }

        private void ClearShowcase()
        {
            for (var i = 0; i < _spawnedShowcaseObjects.Count; i++)
            {
                if (_spawnedShowcaseObjects[i] != null) Destroy(_spawnedShowcaseObjects[i]);
            }
            _spawnedShowcaseObjects.Clear();
        }

        private void SpawnShowcaseTargets(IReadOnlyList<TargetData> order)
        {
            var count = order.Count;
            if (count == 0) return;

            var spacing = count <= 1 ? 0f : Mathf.Clamp(7.0f / (count - 1), 1.6f, _modelSpacing);
            var startX = -(count - 1) * 0.5f * spacing;
            var anchor = _showcaseAnchor != null ? _showcaseAnchor.position : _defaultAnchorPosition;

            for (var i = 0; i < count; i++)
            {
                var target = order[i];
                if (target == null) continue;

                var pos = anchor + new Vector3(startX + i * spacing, 0f, 0f);
                var prefab = target.MiniPrefab != null ? target.MiniPrefab : target.Prefab;
                if (prefab == null) continue;

                var instance = Instantiate(prefab, pos, Quaternion.Euler(0f, 180f, 0f));
                instance.name = $"Showcase_{i + 1}_{target.name}";
                instance.transform.SetParent(transform, true);
                instance.transform.localScale = Vector3.one * _modelScale;

                if (instance.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                var colliders = instance.GetComponentsInChildren<Collider>();
                for (var c = 0; c < colliders.Length; c++)
                {
                    colliders[c].enabled = false;
                }

                if (instance.TryGetComponent<Sorting.SortableItem>(out var sortable)) Destroy(sortable);
                if (instance.TryGetComponent<Sorting.Grabbable>(out var grabbable)) Destroy(grabbable);

                _spawnedShowcaseObjects.Add(instance);
            }
        }

        private IEnumerator MemoryCountdownRoutine(float duration)
        {
            var remaining = Mathf.Max(1f, duration);
            var totalTime = remaining;

            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                var progress = Mathf.Clamp01(remaining / totalTime);

                if (_timerFillBar != null)
                {
                    _timerFillBar.fillAmount = progress;
                }

                if (_timerCountdownText != null)
                {
                    var sec = Mathf.CeilToInt(remaining);
                    _timerCountdownText.text = $"MEMORY TIME: {sec:00}s";
                }

                yield return null;
            }

            ConfirmAndProceed();
        }
    }
}
