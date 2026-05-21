using UnityEngine;
using TMPro;

namespace ProjectOni.UI
{
    /// <summary>
    /// A single row in the stat menu displaying a stat name and its value.
    /// </summary>
    public class StatRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _valueLabel;

        public void Set(string statName, float value)
        {
            if (_nameLabel != null) _nameLabel.text = statName;
            if (_valueLabel != null) _valueLabel.text = Mathf.CeilToInt(value).ToString();
        }
    }
}
