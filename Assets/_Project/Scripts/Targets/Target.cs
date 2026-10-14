using UnityEngine;
using VARG.Data;
using VARG.Mission;
using VARG.Player;

namespace VARG.Targets
{
    [RequireComponent(typeof(Shrinkable))]
    public class Target : MonoBehaviour, IInteractable
    {
        [SerializeField] private TargetData _data;
        [SerializeField] private bool _canInteract = true;

        private Shrinkable _shrinkable;

        public TargetData Data
        {
            get => _data;
            set => _data = value;
        }

        public bool CanInteract => _canInteract && (_shrinkable == null || !_shrinkable.IsShrinking);

        public string Prompt => _data != null ? $"Abduct {_data.DisplayName}" : "Abduct";

        private void Awake()
        {
            _shrinkable = GetComponent<Shrinkable>();
        }

        public void Interact(GameObject interactor)
        {
            StartShrinkCapture();
        }

        public void StartShrinkCapture()
        {
            if (!CanInteract) return;

            if (_shrinkable != null)
            {
                _shrinkable.StartShrink(OnCaptureCompleted);
            }
            else
            {
                OnCaptureCompleted();
            }
        }

        private void OnCaptureCompleted()
        {
            if (MissionManager.HasInstance && _data != null)
            {
                MissionManager.Instance.ValidateCapture(_data);
            }
            else
            {
                Debug.Log($"[Target] Captured without MissionManager: {_data?.DisplayName ?? name}");
            }
        }
    }
}
