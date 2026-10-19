using UnityEngine;

namespace VARG.Targets
{
    [ExecuteAlways]
    public class ChickenBipedSetup : MonoBehaviour
    {
        [SerializeField] private Material _footMaterial;

        private Transform _legR;
        private Transform _legL;
        private Transform _nose;
        private Transform _earL;
        private Transform _earR;
        private Transform _tail1;
        private Transform _hideLegL;
        private Transform _hideLegR;

        private void Awake()
        {
            Setup();
        }

        private void OnEnable()
        {
            Setup();
        }

        private void Setup()
        {
            CacheTransforms();
            HideTransforms();
            CreateFeetIfNeeded();
        }

        private void CacheTransforms()
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name == "RigEarL") _earL = t;
                else if (t.name == "RigEarR") _earR = t;
                else if (t.name == "RigNose") _nose = t;
                else if (t.name == "RigTail1") _tail1 = t;
                else if (t.name == "RigL1Leg1") _hideLegL = t;
                else if (t.name == "RigR2Leg1") _hideLegR = t;
                else if (t.name == "RigR1Leg1") _legR = t;
                else if (t.name == "RigL2Leg1") _legL = t;
            }
        }

        private void LateUpdate()
        {
            HideTransforms();

            // Center the two remaining legs to make a balanced biped stride
            if (_legR != null)
            {
                var pos = _legR.localPosition;
                pos.x = 0.28f;
                pos.y = 0.0f;
                pos.z = -0.15f;
                _legR.localPosition = pos;
            }

            if (_legL != null)
            {
                var pos = _legL.localPosition;
                pos.x = 0.28f;
                pos.y = 0.0f;
                pos.z = 0.15f;
                _legL.localPosition = pos;
            }
        }

        private void HideTransforms()
        {
            if (_earL != null)
            {
                _earL.localScale = Vector3.zero;
                _earL.localPosition = new Vector3(0.20f, 0.15f, 0.00f);
            }
            if (_earR != null)
            {
                _earR.localScale = Vector3.zero;
                _earR.localPosition = new Vector3(0.20f, 0.15f, 0.00f);
            }
            if (_nose != null)
            {
                _nose.localScale = Vector3.zero;
                _nose.localPosition = new Vector3(0.03f, 0.10f, 0.00f);
            }
            if (_tail1 != null) _tail1.localScale = Vector3.zero;
            if (_hideLegL != null) _hideLegL.localScale = Vector3.zero;
            if (_hideLegR != null) _hideLegR.localScale = Vector3.zero;
        }

        public void SetFootMaterial(Material mat)
        {
            _footMaterial = mat;
            CreateFeetIfNeeded();
        }

        private void CreateFeetIfNeeded()
        {
            if (_legR != null && _legR.Find("Foot_Orange") == null)
            {
                BuildFoot(_legR);
            }
            if (_legL != null && _legL.Find("Foot_Orange") == null)
            {
                BuildFoot(_legL);
            }
        }

        private void BuildFoot(Transform legBone)
        {
            var foot = new GameObject("Foot_Orange");
            foot.transform.SetParent(legBone, false);
            foot.transform.localPosition = new Vector3(-0.25f, 0.04f, 0f);
            foot.transform.localRotation = Quaternion.identity;

            var footBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            footBase.name = "FootBase";
            var c1 = footBase.GetComponent<Collider>();
            if (c1 != null) DestroyImmediateOrRuntime(c1);
            footBase.transform.SetParent(foot.transform, false);
            footBase.transform.localPosition = Vector3.zero;
            footBase.transform.localScale = new Vector3(0.06f, 0.16f, 0.14f);
            if (_footMaterial != null) footBase.GetComponent<Renderer>().sharedMaterial = _footMaterial;

            var toeCenter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toeCenter.name = "ToeCenter";
            var c2 = toeCenter.GetComponent<Collider>();
            if (c2 != null) DestroyImmediateOrRuntime(c2);
            toeCenter.transform.SetParent(foot.transform, false);
            toeCenter.transform.localPosition = new Vector3(-0.01f, 0.12f, 0f);
            toeCenter.transform.localScale = new Vector3(0.05f, 0.12f, 0.05f);
            if (_footMaterial != null) toeCenter.GetComponent<Renderer>().sharedMaterial = _footMaterial;

            var toeLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toeLeft.name = "ToeLeft";
            var c3 = toeLeft.GetComponent<Collider>();
            if (c3 != null) DestroyImmediateOrRuntime(c3);
            toeLeft.transform.SetParent(foot.transform, false);
            toeLeft.transform.localPosition = new Vector3(-0.01f, 0.10f, -0.06f);
            toeLeft.transform.localRotation = Quaternion.Euler(0f, -22f, 0f);
            toeLeft.transform.localScale = new Vector3(0.05f, 0.10f, 0.04f);
            if (_footMaterial != null) toeLeft.GetComponent<Renderer>().sharedMaterial = _footMaterial;

            var toeRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toeRight.name = "ToeRight";
            var c4 = toeRight.GetComponent<Collider>();
            if (c4 != null) DestroyImmediateOrRuntime(c4);
            toeRight.transform.SetParent(foot.transform, false);
            toeRight.transform.localPosition = new Vector3(-0.01f, 0.10f, 0.06f);
            toeRight.transform.localRotation = Quaternion.Euler(0f, 22f, 0f);
            toeRight.transform.localScale = new Vector3(0.05f, 0.10f, 0.04f);
            if (_footMaterial != null) toeRight.GetComponent<Renderer>().sharedMaterial = _footMaterial;
        }

        private void DestroyImmediateOrRuntime(Object obj)
        {
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }
    }
}
