using UnityEngine;

namespace VARG.Humor
{
    [CreateAssetMenu(fileName = "BossLineSet", menuName = "VARG/Humor/BossLineSet")]
    public class BossLineSet : ScriptableObject
    {
        [Header("Capture Phase")]
        [SerializeField] private string[] _correctCaptureLines = new string[0];
        [SerializeField] private string[] _outOfOrderCaptureLines = new string[0];
        [SerializeField] private string[] _notInListCaptureLines = new string[0];

        [Header("Sorting Phase")]
        [SerializeField] private string[] _correctCageLines = new string[0];
        [SerializeField] private string[] _wrongCageLines = new string[0];
        [SerializeField] private string[] _correctDiscardLines = new string[0];
        [SerializeField] private string[] _wrongDiscardLines = new string[0];

        [Header("Game End")]
        [SerializeField] private string[] _victoryLines = new string[0];
        [SerializeField] private string[] _defeatLines = new string[0];

        public string[] CorrectCaptureLines => _correctCaptureLines;
        public string[] OutOfOrderCaptureLines => _outOfOrderCaptureLines;
        public string[] NotInListCaptureLines => _notInListCaptureLines;

        public string[] CorrectCageLines => _correctCageLines;
        public string[] WrongCageLines => _wrongCageLines;
        public string[] CorrectDiscardLines => _correctDiscardLines;
        public string[] WrongDiscardLines => _wrongDiscardLines;

        public string[] VictoryLines => _victoryLines;
        public string[] DefeatLines => _defeatLines;
    }
}
