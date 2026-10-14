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
        [SerializeField] private string[] _beamDeniedLines = new string[0];

        [Header("Sorting Phase")]
        [SerializeField] private string[] _correctCageLines = new string[0];
        [SerializeField] private string[] _wrongCageLines = new string[0];
        [SerializeField] private string[] _correctDiscardLines = new string[0];
        [SerializeField] private string[] _wrongDiscardLines = new string[0];

        [Header("Lives Escalation (Anger Tiers)")]
        [Tooltip("Lives 4-5: Sarcastic or mildly annoyed")]
        [SerializeField] private string[] _lifeLostTier1Lines = new string[0];
        [Tooltip("Lives 2-3: Genuinely angry and impatient")]
        [SerializeField] private string[] _lifeLostTier2Lines = new string[0];
        [Tooltip("Life 1: Furious, desperate corporate panic")]
        [SerializeField] private string[] _lifeLostTier3Lines = new string[0];

        [Header("Timer & State")]
        [SerializeField] private string[] _timeLowLines = new string[0];
        [SerializeField] private string[] _victoryLines = new string[0];
        [SerializeField] private string[] _defeatLines = new string[0];
        [SerializeField] private string[] _idleLines = new string[0];

        public string[] CorrectCaptureLines => _correctCaptureLines;
        public string[] OutOfOrderCaptureLines => _outOfOrderCaptureLines;
        public string[] NotInListCaptureLines => _notInListCaptureLines;
        public string[] BeamDeniedLines => _beamDeniedLines;

        public string[] CorrectCageLines => _correctCageLines;
        public string[] WrongCageLines => _wrongCageLines;
        public string[] CorrectDiscardLines => _correctDiscardLines;
        public string[] WrongDiscardLines => _wrongDiscardLines;

        public string[] LifeLostTier1Lines => _lifeLostTier1Lines;
        public string[] LifeLostTier2Lines => _lifeLostTier2Lines;
        public string[] LifeLostTier3Lines => _lifeLostTier3Lines;

        public string[] TimeLowLines => _timeLowLines;
        public string[] VictoryLines => _victoryLines;
        public string[] DefeatLines => _defeatLines;
        public string[] IdleLines => _idleLines;
    }
}
