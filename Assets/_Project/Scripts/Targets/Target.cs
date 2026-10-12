using UnityEngine;
using VARG.Data;
using VARG.Player;

namespace VARG.Targets
{
    public class Target : MonoBehaviour, IInteractable
    {
        [SerializeField] private TargetData _data;
        [SerializeField] private bool _canInteract = true;

        public TargetData Data
        {
            get => _data;
            set => _data = value;
        }

        public bool CanInteract
        {
            get => _canInteract;
            set => _canInteract = value;
        }

        public string Prompt => _data != null ? $"Abduzir {_data.DisplayName}" : "Abduzir";

        public void Interact(GameObject interactor)
        {
            Debug.Log($"[Target] Interacted with: {_data?.DisplayName ?? name}");
        }
    }
}
