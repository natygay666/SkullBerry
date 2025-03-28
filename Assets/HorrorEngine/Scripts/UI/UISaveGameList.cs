using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HorrorEngine
{
    public class UISaveGameList : MonoBehaviour
    {
        [SerializeField] private Button[] m_Slots;

        public UnityEvent<GameObject> OnSubmit;

        [Header("Audio")]
        [SerializeField] private AudioClip m_NavigateClip;
        
        private GameObject m_SelectedSlot;

        protected void Awake()
        {
            foreach (var slot in m_Slots)
            {
                UISelectableCallbacks selectable = slot.GetComponent<UISelectableCallbacks>();
                selectable.OnSelected.AddListener(OnSlotSelected);

                UIPointerClickEvents pointerEvents = slot.GetComponent<UIPointerClickEvents>();
                pointerEvents.OnDoubleClick.AddListener(OnSlotSubmitted);
            }
        }


        // --------------------------------------------------------------------

        void OnSlotSelected(GameObject obj)
        {
            m_SelectedSlot = obj;
            if (m_SelectedSlot)
            {
                UIManager.Get<UIAudio>().Play(m_NavigateClip);
            }
        }

        // --------------------------------------------------------------------

        void OnSlotSubmitted()
        {
            OnSubmit?.Invoke(m_SelectedSlot);
        }

        // --------------------------------------------------------------------

        public GameObject GetSelected()
        {
            return m_SelectedSlot;
        }

        // --------------------------------------------------------------------

        public int GetSelectedIndex()
        {
            int slotIndex = 0;
            foreach (var slot in m_Slots)
            {
                if (slot.gameObject == m_SelectedSlot)
                    break;
                ++slotIndex;
            }
            return slotIndex;
        }

        // --------------------------------------------------------------------

        public void SelectDefault()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(m_Slots[0].gameObject);
        }

        // --------------------------------------------------------------------

        public void FillSlotsInfo(bool saveInProgress = false)
        {
            SaveDataManager<GameSaveData> saveMgr = SaveDataManager<GameSaveData>.Instance;
            int slotIndex = 0;
            foreach(var slot in m_Slots)
            {
                bool exists = saveMgr.SlotExists(slotIndex);
                TMPro.TextMeshProUGUI tmText = slot.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (exists)
                {
                    SaveDataManager<GameSaveData>.SaveData saveData = saveMgr.GetSaveData(slotIndex);
                    tmText.text = $"{slotIndex} .{saveData.GameData.PlayerName} /{saveData.GameData.SaveCount} /{saveData.GameData.SaveLocation}";
                }
                else
                {
                    tmText.text = $"{slotIndex} .No Data";
                }

                if (saveInProgress && slot.gameObject == m_SelectedSlot)
                {
                    m_SelectedSlot.GetComponent<AppearingText>().Show();
                }

                ++slotIndex;
            }
        }


       
    }

}