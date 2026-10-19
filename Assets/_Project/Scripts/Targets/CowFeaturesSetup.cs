using UnityEngine;

namespace VARG.Targets
{
    [ExecuteAlways]
    public class CowFeaturesSetup : MonoBehaviour
    {
        private Transform _earL;
        private Transform _earR;
        private Transform _hornL;
        private Transform _hornR;

        private void Awake()
        {
            CacheTransforms();
            HidePigEars();
        }

        private void OnEnable()
        {
            CacheTransforms();
            HidePigEars();
        }

        private void Start()
        {
            CacheTransforms();
            HidePigEars();
        }

        private void LateUpdate()
        {
            HidePigEars();
        }

        private void CacheTransforms()
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == "RigEarL") _earL = t;
                else if (t.name == "RigEarR") _earR = t;
                else if (t.name == "Horn_L") _hornL = t;
                else if (t.name == "Horn_R") _hornR = t;
            }
        }

        private void HidePigEars()
        {
            if (_earL != null && _earL.localScale != Vector3.zero)
            {
                _earL.localScale = Vector3.zero;
            }

            if (_earR != null && _earR.localScale != Vector3.zero)
            {
                _earR.localScale = Vector3.zero;
            }
        }
    }
}
