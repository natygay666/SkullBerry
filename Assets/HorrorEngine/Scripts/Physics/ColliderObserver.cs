using System;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorEngine
{
    [Serializable]
#if GAME_2D
public class OnTriggerAction : UnityEvent<Collider2D> { }
#else
    public class OnTriggerAction : UnityEvent<Collider> { }
#endif

    public class ColliderObserver : MonoBehaviour
    {
        public OnTriggerAction TriggerEnter;
        public OnTriggerAction TriggerExit;

        private Action<OnDisableNotifier> mOnColliderDisabled;

        private void Awake()
        {
            mOnColliderDisabled = OnColliderDisabled;
        }

#if GAME_2D
    private void OnTriggerEnter2D(Collider2D other)
#else
        private void OnTriggerEnter(Collider other)
#endif
        {
            other.GetComponentInParent<OnDisableNotifier>().AddCallback(mOnColliderDisabled);
            TriggerEnter?.Invoke(other);
        }
#if GAME_2D
    private void OnTriggerExit2D(Collider2D other)
#else
        private void OnTriggerExit(Collider other)
#endif
        {
            other.GetComponentInParent<OnDisableNotifier>().RemoveCallback(mOnColliderDisabled);
            TriggerExit?.Invoke(other);
        }

        private void OnColliderDisabled(OnDisableNotifier notifier)
        {
            notifier.RemoveCallback(mOnColliderDisabled);
#if GAME_2D
        TriggerExit?.Invoke(notifier.GetComponent<Collider2D>());
#else
            TriggerExit?.Invoke(notifier.GetComponent<Collider>());
#endif

        }
    }
}