using TMPro;
using UnityEngine.UI;

namespace Harmonika.Tools
{
    public class ToggleWithText : Toggle
    {
        public TMP_Text tmpText;

        protected override void Awake()
        {
            base.Awake();
            tmpText = GetComponentInChildren<TMP_Text>();
        }
    }
}