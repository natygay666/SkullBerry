using System;
using UnityEngine;
using UnityEngine.UI;

namespace HorrorEngine
{
    public class UIChoices : MonoBehaviour
    {
        public GameObject[] Choices;

        // --------------------------------------------------------------------

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        // --------------------------------------------------------------------

        public void Show(ChoiceData data)
        {
            PauseController.Instance.Pause();
            CursorController.Instance.SetInUI(true);

            if (data.ChoiceDialog.IsValid())
            {
                UIManager.PushAction(new UIStackedAction()
                {
                    Action = () =>
                    {
                        ShowChoices(data.Choices);
                    },
                    StopProcessingActions = true
                });
                UIManager.Get<UIDialog>().Show(data.ChoiceDialog, false);

                return;
            }
            else
            {
                ShowChoices(data.Choices);
            }
        }

        // --------------------------------------------------------------------

        private void ShowChoices(ChoiceEntry[] choices)
        {
            for (int i = 0; i < Choices.Length; ++i)
            {
                bool active = i < choices.Length;
                Choices[i].SetActive(active);

                var button = Choices[i].GetComponent<Button>();
                button.onClick.RemoveAllListeners();

                if (active)
                {
                    Choices[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = choices[i].Text;

                    var action = choices[i].OnSelected;
                    button.onClick.AddListener(() =>
                    {
                        action?.Invoke();
                        Hide();
                    });
                }
            }

            if (Choices[0].activeSelf)
                Choices[0].GetComponentInChildren<Selectable>().Select();

            gameObject.SetActive(true);
        }

        // --------------------------------------------------------------------

        private void Hide()
        {
            PauseController.Instance.Resume();
            CursorController.Instance.SetInUI(false);
            gameObject.SetActive(false);

            UIManager.Get<UIDialog>().Hide();

            // This has to be the last in case it reactivates
           // Action closeaction = m_OnCloseCallback;
           // m_OnCloseCallback = null; // Clear before
            //closeaction?.Invoke();

            UIManager.PopAction();
        }
    }

}