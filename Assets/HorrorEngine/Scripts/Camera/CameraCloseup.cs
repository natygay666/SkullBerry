using System;
using System.Collections;
using UnityEngine;

namespace HorrorEngine
{
    public abstract class Closeup : MonoBehaviour
    {
        [SerializeField] bool m_PauseGame = true;
        [SerializeField] float m_FadeOutDuration = 0.5f;
        [SerializeField] float m_FadeInDuration = 0.5f;

        public abstract IEnumerator ActivationRoutine();
        public abstract IEnumerator DectivationRoutine();

        private UIFade m_UIFade;

        protected virtual void Start()
        {
            m_UIFade = UIManager.Get<UIFade>();
        }

        public void Activate()
        {
            StartCoroutine(StartCloseupRoutine(ActivationRoutine));
        }

        public void Deactivate()
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(EndCloseupRoutine(DectivationRoutine));
        }

        private IEnumerator StartCloseupRoutine(Func<IEnumerator> activationRoutine)
        {
            if (m_PauseGame)
                PauseController.Instance.Pause();

            // Fade Out
            yield return m_UIFade.Fade(0f, 1f, m_FadeOutDuration);

            yield return StartCoroutine(activationRoutine?.Invoke());

            // Fade In
            yield return m_UIFade.Fade(1f, 0f, m_FadeInDuration);

        }

        private IEnumerator EndCloseupRoutine(Func<IEnumerator> deactivationRoutine)
        {
            // Fade Out
            yield return m_UIFade.Fade(0f, 1f, m_FadeOutDuration);

            yield return StartCoroutine(deactivationRoutine?.Invoke());

            // Fade In
            yield return m_UIFade.Fade(1f, 0f, m_FadeInDuration);

            if (m_PauseGame)
                PauseController.Instance.Resume();
        }
    }

    public class CameraCloseup : Closeup
    {
        [SerializeField] bool m_HidePlayer = false;
        [SerializeField] Cinemachine.CinemachineVirtualCamera m_Camera;

        protected override void Start()
        {
            base.Start();
            m_Camera.gameObject.SetActive(false);
        }

        public override IEnumerator ActivationRoutine()
        {
            if (m_HidePlayer)
            {
                GameManager.Instance.Player.SetVisible(false);
            }
            m_Camera.gameObject.SetActive(true);
            yield return null;
        }

        public override IEnumerator DectivationRoutine()
        {
            if (m_HidePlayer)
            {
                GameManager.Instance.Player.SetVisible(true);
            }
            m_Camera.gameObject.SetActive(false);
            yield return null;
        }
    }
}