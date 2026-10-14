using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VARG.Data;

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
        [Tooltip("Base scale of 3D targets when 3 or fewer are displayed.")]
        [SerializeField] private float _modelScale = 2.0f;
        [Tooltip("Maximum horizontal spacing between targets.")]
        [SerializeField] private float _modelSpacing = 3.6f;
        [Tooltip("Minimum horizontal spacing between targets to prevent overlap.")]
        [SerializeField] private float _minSpacing = 1.6f;
        [Tooltip("Maximum total width occupied by the showcase sequence.")]
        [SerializeField] private float _maxShowcaseWidth = 9.0f;
        [Tooltip("Minimum scale multiplier when sequence has many targets.")]
        [SerializeField] private float _minScaleMultiplier = 0.65f;

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

        public void Configure(GameObject timerPanel, TextMeshProUGUI countdownText, Image fillBar, Button confirmButton, Transform showcaseAnchor = null)
        {
            _timerPanel = timerPanel;
            _timerCountdownText = countdownText;
            _timerFillBar = fillBar;
            _confirmButton = confirmButton;
            _showcaseAnchor = showcaseAnchor;

            if (_timerPanel != null)
            {
                _timerPanel.SetActive(false);
            }

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveListener(ConfirmAndProceed);
                _confirmButton.onClick.AddListener(ConfirmAndProceed);
            }
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy || _isCompleted || _countdownCoroutine == null) return;

            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
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

            var list = order;
            if (list == null || list.Count == 0)
            {
#if UNITY_EDITOR
                var fallback = new List<TargetData>();
                var cow = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Cow.asset");
                var pig = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Pig.asset");
                var chicken = UnityEditor.AssetDatabase.LoadAssetAtPath<TargetData>("Assets/_Project/Data/Targets/Target_Chicken.asset");
                if (cow != null) fallback.Add(cow);
                if (pig != null) fallback.Add(pig);
                if (chicken != null) fallback.Add(chicken);
                list = fallback;
#endif
            }

            if (list != null && list.Count > 0)
            {
                SpawnShowcaseTargets(list);
            }
        }

        public void Populate(IReadOnlyList<TargetData> order, float memoryDuration, Action onCompleted)
        {
            _isCompleted = false;
            _onMemorizationFinished = onCompleted;

            gameObject.SetActive(true);
            ClearShowcase();

            if (order != null && order.Count > 0)
            {
                SpawnShowcaseTargets(order);
            }

            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
            }

            _countdownCoroutine = StartCoroutine(MemoryCountdownRoutine(memoryDuration));
        }

        public void HideShowcase()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            if (_timerPanel != null)
            {
                _timerPanel.SetActive(false);
            }

            ClearShowcase();
            gameObject.SetActive(false);
        }

        public void ConfirmAndProceed()
        {
            if (_isCompleted) return;
            _isCompleted = true;

            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }

            if (_timerPanel != null)
            {
                _timerPanel.SetActive(false);
            }

            ClearShowcase();

            var callback = _onMemorizationFinished;
            _onMemorizationFinished = null;
            callback?.Invoke();
        }

        private void ClearShowcase()
        {
            for (var i = 0; i < _spawnedShowcaseObjects.Count; i++)
            {
                if (_spawnedShowcaseObjects[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(_spawnedShowcaseObjects[i]);
                    }
                    else
                    {
                        DestroyImmediate(_spawnedShowcaseObjects[i]);
                    }
                }
            }
            _spawnedShowcaseObjects.Clear();

            var children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("Showcase_"))
                {
                    children.Add(child.gameObject);
                }
            }

            foreach (var child in children)
            {
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private void SpawnShowcaseTargets(IReadOnlyList<TargetData> order)
        {
            var count = order.Count;
            if (count == 0) return;

            var (spacing, scale) = CalculateLayout(count);
            var startX = -(count - 1) * 0.5f * spacing;
            var anchor = _showcaseAnchor != null ? _showcaseAnchor.position : _defaultAnchorPosition;

            for (var i = 0; i < count; i++)
            {
                var target = order[i];
                if (target == null) continue;

                var pos = anchor + new Vector3(startX + i * spacing, 0f, 0f);
                var prefab = target.MiniPrefab != null ? target.MiniPrefab : target.Prefab;

                if (prefab != null)
                {
                    var instance = Instantiate(prefab, pos, Quaternion.identity);
                    instance.name = $"Showcase_{i + 1}_{target.name}";
                    instance.transform.SetParent(transform, true);
                    instance.transform.localScale = Vector3.one * scale;

                    // Remove/disable physics and sorting logic so it's a pure visual prop
                    var rb = instance.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.useGravity = false;
                    }

                    var colliders = instance.GetComponentsInChildren<Collider>();
                    foreach (var col in colliders)
                    {
                        col.enabled = false;
                    }

                    var sortable = instance.GetComponent<Sorting.SortableItem>();
                    if (sortable != null) Destroy(sortable);

                    var grabbable = instance.GetComponent<Sorting.Grabbable>();
                    if (grabbable != null) Destroy(grabbable);

                    var highlighter = instance.GetComponent<Sorting.HighlightOnHover>();
                    if (highlighter != null) Destroy(highlighter);

                    var spin = instance.AddComponent<SpinAndBob>();
                    spin.Initialize(pos, i * 0.4f);

                    _spawnedShowcaseObjects.Add(instance);
                }
            }
        }

        private (float spacing, float scale) CalculateLayout(int count)
        {
            if (count <= 1)
            {
                return (0f, _modelScale);
            }

            var desiredSpacing = _maxShowcaseWidth / (count - 1);
            var spacing = Mathf.Clamp(desiredSpacing, _minSpacing, _modelSpacing);

            var spacingRatio = Mathf.InverseLerp(_minSpacing, _modelSpacing, spacing);
            var scaleMultiplier = Mathf.Lerp(_minScaleMultiplier, 1.0f, spacingRatio);
            var scale = _modelScale * scaleMultiplier;

            return (spacing, scale);
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
