using UnityEngine;

namespace VARG.Humor
{
    [CreateAssetMenu(fileName = "BossLineSet", menuName = "VARG/Humor/BossLineSet")]
    public class BossLineSet : ScriptableObject
    {
        [SerializeField] private string[] _correctCaptureLines;
        [SerializeField] private string[] _outOfOrderCaptureLines;
        [SerializeField] private string[] _notInListCaptureLines;
        [SerializeField] private string[] _wrongCageLines;
        [SerializeField] private string[] _wrongDiscardLines;
        [SerializeField] private string[] _idleLines;

        public string[] CorrectCaptureLines => _correctCaptureLines;
        public string[] OutOfOrderCaptureLines => _outOfOrderCaptureLines;
        public string[] NotInListCaptureLines => _notInListCaptureLines;
        public string[] WrongCageLines => _wrongCageLines;
        public string[] WrongDiscardLines => _wrongDiscardLines;
        public string[] IdleLines => _idleLines;
    }
}
