using UnityEngine;

namespace VARG.Data
{
    [CreateAssetMenu(fileName = "PenaltyConfig", menuName = "VARG/Data/PenaltyConfig")]
    public class PenaltyConfig : ScriptableObject
    {
        [SerializeField] private float _outOfOrderCapturePenalty = 10f;
        [SerializeField] private float _notInListSortingDebt = 10f;
        [SerializeField] private float _wrongCagePenalty = 5f;
        [SerializeField] private float _wrongDiscardPenalty = 5f;
        [SerializeField] private int _startingLives = 5;

        public float OutOfOrderCapturePenalty => _outOfOrderCapturePenalty;
        public float NotInListSortingDebt => _notInListSortingDebt;
        public float WrongCagePenalty => _wrongCagePenalty;
        public float WrongDiscardPenalty => _wrongDiscardPenalty;
        public int StartingLives => _startingLives;
    }
}
