using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace EasyTransition
{
    public class TransitionManager : MonoBehaviour
    {
        [Header("Transition")]
        [SerializeField] private GameObject transitionTemplate;

        [Header("Transition Sound")]
        [SerializeField] private bool useTransitionSound = true;
        [SerializeField] private string transitionSound = "Brush";

        [Range(0f, 1f)]
        [SerializeField] private float transitionSoundVolume = 2f;

        private bool runningTransition;

        public UnityAction onTransitionBegin;
        public UnityAction onTransitionCutPointReached;
        public UnityAction onTransitionEnd;

        private static TransitionManager instance;

        private void Awake()
        {
            instance = this;
        }

        public static TransitionManager Instance()
        {
            if (instance == null)
                Debug.LogError("You tried to access the instance before it exists.");

            return instance;
        }

        public void Transition(TransitionSettings transition, float startDelay)
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assign a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(startDelay, transition));
        }

        public void Transition(
            string sceneName,
            TransitionSettings transition,
            float startDelay
        )
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assign a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(sceneName, startDelay, transition));
        }

        public void Transition(
            int sceneIndex,
            TransitionSettings transition,
            float startDelay
        )
        {
            if (transition == null || runningTransition)
            {
                Debug.LogError("You have to assign a transition.");
                return;
            }

            runningTransition = true;
            StartCoroutine(Timer(sceneIndex, startDelay, transition));
        }

        private int GetSceneIndex(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).buildIndex;
        }

        private void PlayTransitionSound()
        {
            if (!useTransitionSound)
                return;

            if (AudioManager.Instance == null)
                return;

            if (string.IsNullOrEmpty(transitionSound))
                return;

            AudioManager.Instance.PlayUISFX(
                transitionSound,
                transitionSoundVolume
            );
        }

        private IEnumerator Timer(
            string sceneName,
            float startDelay,
            TransitionSettings transitionSettings
        )
        {
            yield return new WaitForSecondsRealtime(startDelay);

            onTransitionBegin?.Invoke();
            PlayTransitionSound();

            GameObject template = Instantiate(transitionTemplate);

            template.GetComponent<Transition>().transitionSettings =
                transitionSettings;

            float transitionTime = transitionSettings.transitionTime;

            if (transitionSettings.autoAdjustTransitionTime)
            {
                transitionTime =
                    transitionTime / transitionSettings.transitionSpeed;
            }

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();

            SceneManager.LoadScene(sceneName);

            yield return new WaitForSecondsRealtime(
                transitionSettings.destroyTime
            );

            onTransitionEnd?.Invoke();

            runningTransition = false;
        }

        private IEnumerator Timer(
            int sceneIndex,
            float startDelay,
            TransitionSettings transitionSettings
        )
        {
            yield return new WaitForSecondsRealtime(startDelay);

            onTransitionBegin?.Invoke();
            PlayTransitionSound();

            GameObject template = Instantiate(transitionTemplate);

            template.GetComponent<Transition>().transitionSettings =
                transitionSettings;

            float transitionTime = transitionSettings.transitionTime;

            if (transitionSettings.autoAdjustTransitionTime)
            {
                transitionTime =
                    transitionTime / transitionSettings.transitionSpeed;
            }

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();

            SceneManager.LoadScene(sceneIndex);

            yield return new WaitForSecondsRealtime(
                transitionSettings.destroyTime
            );

            onTransitionEnd?.Invoke();

            runningTransition = false;
        }

        private IEnumerator Timer(
            float delay,
            TransitionSettings transitionSettings
        )
        {
            yield return new WaitForSecondsRealtime(delay);

            onTransitionBegin?.Invoke();
            PlayTransitionSound();

            GameObject template = Instantiate(transitionTemplate);

            template.GetComponent<Transition>().transitionSettings =
                transitionSettings;

            float transitionTime = transitionSettings.transitionTime;

            if (transitionSettings.autoAdjustTransitionTime)
            {
                transitionTime =
                    transitionTime / transitionSettings.transitionSpeed;
            }

            yield return new WaitForSecondsRealtime(transitionTime);

            onTransitionCutPointReached?.Invoke();

            template.GetComponent<Transition>().OnSceneLoad(
                SceneManager.GetActiveScene(),
                LoadSceneMode.Single
            );

            yield return new WaitForSecondsRealtime(
                transitionSettings.destroyTime
            );

            onTransitionEnd?.Invoke();

            runningTransition = false;
        }

        private IEnumerator Start()
        {
            while (gameObject.activeInHierarchy)
            {
                int managerCount = FindObjectsByType<TransitionManager>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                ).Length;

                if (managerCount > 1)
                {
                    Debug.LogError(
                        $"There are {managerCount} Transition Managers in your scene. " +
                        "Please ensure there is only one Transition Manager in your scene " +
                        "or overlapping transitions may occur."
                    );
                }

                yield return new WaitForSecondsRealtime(1f);
            }
        }
    }
}