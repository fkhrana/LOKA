using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

namespace Christina.UI
{
    public class Carousel : MonoBehaviour, IEndDragHandler
    {
        [Header("Parts Setup")]
        [SerializeField] private List<CarouselEntry> entries = new List<CarouselEntry>();

        [Space]
        [SerializeField] private ScrollRect scrollRect;

        [Space]
        [SerializeField] private RectTransform contentBoxHorizontal;
        [SerializeField] private Image carouselEntryPrefab;
        private List<Image> _imagesForEntries = new List<Image>();

        [Space]
        [SerializeField] private Transform indicatorParent;
        [SerializeField] private CarouselIndicator indicatorPrefab;
        private List<CarouselIndicator> _indicators = new List<CarouselIndicator>();

        [Header("Animation Setup")]
        [SerializeField, Range(0.25f, 1f)]
        private float duration = 0.5f;

        [SerializeField]
        private AnimationCurve easeCurve;

        [Header("Auto Scroll Setup")]
        [SerializeField]
        private bool autoScroll = false;

        [SerializeField]
        private float autoScrollInterval = 5f;

        private float _autoScrollTimer;

        [Header("Info Setup")]
        [SerializeField]
        private CarouselTextBox textBoxController;

        private int _currentIndex = 0;
        private Coroutine _scrollCoroutine;

        private void Reset()
        {
            scrollRect = GetComponentInChildren<ScrollRect>();
            textBoxController = GetComponentInChildren<CarouselTextBox>();
        }

        private void Start()
        {
            foreach (var entry in entries)
            {
                Image carouselEntry = Instantiate(
                    carouselEntryPrefab,
                    contentBoxHorizontal
                );

                carouselEntry.sprite = entry.EntryGraphic;
                _imagesForEntries.Add(carouselEntry);

                var indicator = Instantiate(
                    indicatorPrefab,
                    indicatorParent
                );

                indicator.Initialize(
                    () => ScrollToSpecificIndex(entries.IndexOf(entry))
                );

                _indicators.Add(indicator);
            }

            if (_indicators.Count > 0)
            {
                _indicators[0].Activate(0.1f);
            }

            _autoScrollTimer = autoScrollInterval;

            if (entries.Count > 0)
            {
                var headline = entries[0].Headline;
                var description = entries[0].Description;

                textBoxController.SetTextWithoutFade(
                    headline,
                    description
                );
            }
        }

        private void ClearCurrentIndex()
        {
            if (_indicators.Count > 0)
            {
                _indicators[_currentIndex].Deactivate(duration);
            }
        }

        private void ScrollToSpecificIndex(int index)
        {
            ClearCurrentIndex();
            ScrollTo(index);
        }

        public void ScrollToNext()
        {
            if (_imagesForEntries.Count == 0)
                return;

            ClearCurrentIndex();

            _currentIndex = (_currentIndex + 1) % _imagesForEntries.Count;

            ScrollTo(_currentIndex);
        }

        public void ScrollToPrevious()
        {
            if (_imagesForEntries.Count == 0)
                return;

            ClearCurrentIndex();

            _currentIndex =
                (_currentIndex - 1 + _imagesForEntries.Count)
                % _imagesForEntries.Count;

            ScrollTo(_currentIndex);
        }

        private void ScrollTo(int index)
        {
            _currentIndex = index;
            _autoScrollTimer = autoScrollInterval;

            float targetHorizontalPosition = 0f;

            if (_imagesForEntries.Count > 1)
            {
                targetHorizontalPosition =
                    (float)_currentIndex / (_imagesForEntries.Count - 1);
            }

            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
            }

            _scrollCoroutine = StartCoroutine(
                LerpToPos(targetHorizontalPosition)
            );

            if (entries.Count > 0)
            {
                var headline = entries[_currentIndex].Headline;
                var description = entries[_currentIndex].Description;

                textBoxController.SetText(
                    headline,
                    description,
                    duration
                );
            }

            if (_indicators.Count > 0)
            {
                _indicators[_currentIndex].Activate(duration);
            }
        }

        private IEnumerator LerpToPos(float targetHorizontalPosition)
        {
            float elapsedTime = 0f;
            float initialPos = scrollRect.horizontalNormalizedPosition;

            if (duration > 0)
            {
                while (elapsedTime <= duration)
                {
                    float easeValue =
                        easeCurve.Evaluate(elapsedTime / duration);

                    float newPosition = Mathf.Lerp(
                        initialPos,
                        targetHorizontalPosition,
                        easeValue
                    );

                    scrollRect.horizontalNormalizedPosition = newPosition;

                    // FIX: pakai unscaledDeltaTime supaya tetap jalan walau Time.timeScale = 0
                    elapsedTime += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            scrollRect.horizontalNormalizedPosition =
                targetHorizontalPosition;
        }

        private void Update()
        {
            if (!autoScroll)
                return;

            // FIX: pakai unscaledDeltaTime supaya auto scroll tetap jalan walau timeScale = 0
            _autoScrollTimer -= Time.unscaledDeltaTime;

            if (_autoScrollTimer <= 0)
            {
                ScrollToNext();
                _autoScrollTimer = autoScrollInterval;
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (data.delta.x != 0)
            {
                if (data.delta.x > 0)
                {
                    ScrollToPrevious();
                }
                else
                {
                    ScrollToNext();
                }
            }
            else
            {
                ScrollToSpecificIndex(_currentIndex);
            }
        }
    }
}