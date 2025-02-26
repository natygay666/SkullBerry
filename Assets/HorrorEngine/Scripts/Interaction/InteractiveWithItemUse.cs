using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    [RequireComponent(typeof(Interactive))]
    public class InteractiveWithItemUse : MonoBehaviour
    {
        [Tooltip("Item that can be used with this interactable")]
        [SerializeField] private ItemData m_Item;
        
        [Tooltip("This indicates if the item can be used automatically if present in the inventory")]
        [SerializeField] private bool m_AutoUseOnInteraction = true;
        
        [Tooltip("This indicates if the item will be removed from the inventory after use")]
        [SerializeField] private bool m_RemoveItemFromInventory;

        [Tooltip("This choice will be shown to the player before using the time. Leave empty to ignore. Call Use() method on the affirmative choice")]
        [SerializeField] private Choice m_BeforeUseChoice;

        [Tooltip("This tag will be replaced in choice and dialog with the item name")]
        [SerializeField] private string m_ItemNameTag = "{ITEMNAME}";

        [HideInInspector]
        [FormerlySerializedAs("m_NoItemDialog")]
        [SerializeField] private string[] m_NoItemDialog_DEPRECATED;

        public UnityEvent OnItemNotInInventory;
        public UnityEvent OnItemUsed;

        private Interactive m_Interactive;

        // --------------------------------------------------------------------

        private void Awake()
        {
            m_Interactive = GetComponentInChildren<Interactive>();
            m_Interactive.OnInteract.AddListener(OnInteract);

            Debug.Assert(m_Item, "Item has not been set on InteractWithItemUse", gameObject);

            if (m_BeforeUseChoice)
            {
                m_BeforeUseChoice.Data.ChoiceDialog.ReplaceTag(m_ItemNameTag, m_Item.Name);
            }
        }

        // --------------------------------------------------------------------

        private void OnDestroy()
        {
            m_Interactive.OnInteract.RemoveListener(OnInteract);
        }

        // --------------------------------------------------------------------

        private void OnInteract(IInteractor interactor)
        {
            if (m_Item == null)
            {
                return;
            }

            if (GameManager.Instance.Inventory.Contains(m_Item) && m_AutoUseOnInteraction)
            {
                if (m_BeforeUseChoice)
                {
                    m_BeforeUseChoice.Choose();
                }
                else
                {
                    Use();
                }
            }
            else
            {
                OnItemNotInInventory?.Invoke();
            }
        }

        // --------------------------------------------------------------------

        public bool UseWithItem(ItemData item)
        {
            if (item != m_Item)
            {
                return false;
            }

            if (m_BeforeUseChoice)
            {
                m_BeforeUseChoice.Choose();
            }
            else
            {
                Use();
            }

            return true;
        }

        // --------------------------------------------------------------------

        public void Use()
        {
            OnItemUsed?.Invoke();

            if (m_RemoveItemFromInventory)
            {
                GameManager.Instance.Inventory.Remove(m_Item);
            }
        }
    }
}