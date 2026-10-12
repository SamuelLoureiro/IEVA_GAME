using UnityEngine;

namespace VARG.Data
{
    [CreateAssetMenu(fileName = "TargetData", menuName = "VARG/Data/TargetData")]
    public class TargetData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private TargetType _type;
        [SerializeField] private string _displayName;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private GameObject _miniPrefab;
        [SerializeField] private Sprite _icon;
        [SerializeField] private float _walkSpeed = 3f;
        [SerializeField] private float _fleeSpeed = 6f;

        public string Id => _id;
        public TargetType Type => _type;
        public string DisplayName => _displayName;
        public GameObject Prefab => _prefab;
        public GameObject MiniPrefab => _miniPrefab;
        public Sprite Icon => _icon;
        public float WalkSpeed => _walkSpeed;
        public float FleeSpeed => _fleeSpeed;
    }
}
