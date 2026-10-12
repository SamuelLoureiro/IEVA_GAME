using UnityEngine;

namespace VARG.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "VARG/Data/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [SerializeField] private bool _useShipFlight = false;

        public bool UseShipFlight
        {
            get => _useShipFlight;
            set => _useShipFlight = value;
        }
    }
}
