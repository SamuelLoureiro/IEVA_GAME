using UnityEngine;

namespace VARG.Data
{
    [CreateAssetMenu(fileName = "CampaignConfig", menuName = "VARG/Data/CampaignConfig")]
    public class CampaignConfig : ScriptableObject
    {
        [SerializeField] private TargetData[] _targets;
        [SerializeField] private LevelConfig[] _levels;

        public TargetData[] Targets => _targets;
        public LevelConfig[] Levels => _levels;
    }
}
