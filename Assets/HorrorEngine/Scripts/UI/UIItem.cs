using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace HorrorEngine
{
    public class UIItem : MonoBehaviour
    {
        [SerializeField] private Image m_Image;
        [SerializeField] private GameObject m_InfoPanel;
        [SerializeField] private TextMeshProUGUI m_Text;
        
        [Header("Examination")]
        [SerializeField] private bool OpenExamination = true;
        [SerializeField] private bool InteractDuringExamination = true;

        [Header("Audio")]
        [SerializeField] private AudioClip m_ShowClip;
        [SerializeField] private AudioClip m_CloseClip;

        private IUIInput m_Input;
        

        // --------------------------------------------------------------------

        void Awake()
        {
            m_Input = GetComponentInParent<IUIInput>();
            MessageBuffer<ItemPickedUpMessage>.Subscribe(OnItemPickedUp);
            gameObject.SetActive(false);
        }

        // --------------------------------------------------------------------

        private void OnDestroy()
        {
            MessageBuffer<ItemPickedUpMessage>.Unsubscribe(OnItemPickedUp);
        }

        // --------------------------------------------------------------------

        void OnItemPickedUp(ItemPickedUpMessage msg)
        {
            var itemData = msg.Data;

            if (gameObject.activeInHierarchy)
            {
                UIManager.PushAction(new UIStackedAction()
                {
                    Action = () => { Show(itemData); },
                    StopProcessingActions = true
                });
            }
            else
            {
                Show(itemData);
            }
        }

        // --------------------------------------------------------------------

        public void Show(ItemData item)
        {
            PauseController.Instance.Pause();

            m_Image.gameObject.SetActive(!OpenExamination);
            m_InfoPanel.SetActive(!OpenExamination);

            gameObject.SetActive(true);

            if (OpenExamination)
            {
                this.InvokeActionNextFrame(() => // This is needed in case we come from UIExamineItem itself
                {
                    UIManager.Get<UIExamineItem>().Show(item, InteractDuringExamination, false);
                });
            }
            else
            {
                m_Image.sprite = item.Image;
                m_Text.text = item.Name;
            }

            UIManager.Get<UIAudio>().Play(m_ShowClip);
        }

        // --------------------------------------------------------------------

        private void Update()
        {
            if (m_Input.IsConfirmDown() || m_Input.IsCancelDown())
            {
                Hide();
            }
        }

        // --------------------------------------------------------------------

        void Hide()
        {
            if (OpenExamination)
            {
                UIManager.Get<UIExamineItem>().Hide();
            }

            PauseController.Instance.Resume();
            gameObject.SetActive(false);

            if (!UIManager.PopAction())
                UIManager.Get<UIAudio>().Play(m_CloseClip);
        }
    }
}