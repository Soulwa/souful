using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace soulful
{
    public class SongUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI notesText;
        [SerializeField]
        private TextMeshProUGUI funkysText;
        [SerializeField]
        private TextMeshProUGUI comboText;
        [SerializeField]
        private TextMeshPro noteHitTextPrefab;
        private TextMeshPro activeNoteHitText;

        [SerializeField]
        private CanvasGroup songGroup;

        private void Awake()
        {

        }

        public void CreateNoteHitText(string text, Direction direction)
        {
            if (activeNoteHitText != null)
            {
                Destroy(activeNoteHitText.gameObject);
            }
            // should be a custom class with text renderer on, lerps alpha & disappears? or animation to disappear
            activeNoteHitText = Instantiate(noteHitTextPrefab);
            activeNoteHitText.text = text;
            activeNoteHitText.transform.position = Vector2.one * 1;
        }

        public void SetNotesText(int current, int max)
        {
            notesText.text = string.Format("{0}/{1}", current, max);
        }

        public void SetFunkysText(int current, int max)
        {
            funkysText.text = string.Format("{0}/{1}", current, max);
        }

        public void SetComboText(int combo)
        {
            comboText.text = combo.ToString();
        }

        public void Show()
        {
            songGroup.alpha = 1f;
            songGroup.interactable = true;
        }

        public void Hide()
        {
            songGroup.alpha = 0f;
            songGroup.interactable = false;
        }

        public void BeginFadeOut() { }
    }
}