using UnityEngine;
using TMPro;

namespace ProjectOni.UI
{
    public class TraitRowUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI traitText;

        public void SetText(string text)
        {
            if (traitText != null)
            {
                traitText.text = text;
            }
        }
    }
}
