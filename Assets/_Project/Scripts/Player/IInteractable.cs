using UnityEngine;

namespace VARG.Player
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        void Interact(GameObject interactor);
        string Prompt { get; }
    }
}
