using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HorrorEngine
{
    public class PointOfInterest : MonoBehaviour
    {
        [SerializeField] private DialogData m_Dialog;
        public UnityEvent OnCheckStart;
        public UnityEvent OnCheckEnd;
        
        [HideInInspector]
        [FormerlySerializedAs("Dialog")]
        [SerializeField] private string[] Dialog_DEPRECATED;

        public void Check()
        {
            OnCheckStart?.Invoke();
            if (m_Dialog.IsValid())
            {
                UIManager.PushAction(new UIStackedAction()
                {
                    Action = () =>
                    {
                        OnCheckEnd?.Invoke();
                    }
                });
                UIManager.Get<UIDialog>().Show(m_Dialog);
            }
        }
    }
}