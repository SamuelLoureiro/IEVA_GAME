using UnityEngine;
using VARG.Core;

namespace VARG.Humor
{
    public class BossDialogueManager : Singleton<BossDialogueManager>
    {
        [SerializeField] private BossLineSet _lines;

        public BossLineSet Lines => _lines;
    }
}
