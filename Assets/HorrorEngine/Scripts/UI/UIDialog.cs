using UnityEngine;
using TMPro;
using System;
using System.Collections;

namespace HorrorEngine
{
    [Serializable]
    public class DialogLine
    {
        public float Delay;
        public string Text;
    }

    [Serializable]
    public class DialogData
    {
        public DialogLine[] Lines;

        public bool IsValid()
        {
            return Lines != null && Lines.Length > 0;
        }

        public void ReplaceTag(string tag, string withText)
        {
            for (int i = 0; i < Lines.Length; ++i)
            {
                Lines[i].Text = Lines[i].Text.Replace(tag, withText);
            }
        }
    }

    public class UIDialog : MonoBehaviour
    {
        [SerializeField] GameObject m_Content;
        [SerializeField] TextMeshProUGUI m_Text;

        private int m_CurrentLine;
        private DialogData m_Dialog;

        private IUIInput m_Input;
        private bool m_HideOnEnd;

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_Input = GetComponentInParent<IUIInput>();

            gameObject.SetActive(false);
        }

        // --------------------------------------------------------------------

        public void Show(DialogData dialog, bool hideOnEnd = true)
        {
            m_Dialog = dialog;
            m_HideOnEnd = hideOnEnd;
            PauseController.Instance.Pause();
            gameObject.SetActive(true);
            m_CurrentLine = -1;
            ShowNextLine();
        }

        // --------------------------------------------------------------------

        private void ShowNextLine()
        {
            ++m_CurrentLine;
            
            var line = m_Dialog.Lines[m_CurrentLine];
            if (line.Delay > 0)
            {
                m_Content.SetActive(false);
                m_Text.text = "";
                StartCoroutine(ShowLineWithDelay(line));
            }
            else
            {
                m_Text.text = line.Text;
                m_Content.SetActive(true);
            }
        }

        // --------------------------------------------------------------------

        private IEnumerator ShowLineWithDelay(DialogLine line)
        {
            yield return Yielders.UnscaledTime(line.Delay);
            m_Text.text = m_Dialog.Lines[m_CurrentLine].Text;
            m_Content.SetActive(true);
        }

        // --------------------------------------------------------------------

        private void Update()
        {
            if (m_Input.IsConfirmDown() && m_Content.activeSelf)
            {
                
                if (m_CurrentLine == m_Dialog.Lines.Length - 1)
                {
                    if (m_HideOnEnd)
                    {
                        Hide();
                    }
                    else
                    {
                       UIManager.PopAction();
                    }
                }
                else
                {
                    ShowNextLine();
                }
            }
        }

        // --------------------------------------------------------------------

        public void Hide()
        {
            gameObject.SetActive(false);
            PauseController.Instance.Resume();
            
            UIManager.PopAction();
        }

    }
}