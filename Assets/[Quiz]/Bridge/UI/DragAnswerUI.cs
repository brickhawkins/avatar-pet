using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Anteena.Quiz.Bridge
{
    public class DragAnswerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] TMP_Text answer;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Transform originalParent;  // Stores the original grid parent
        private Canvas canvas;
        
        public QuizData Data;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            
            originalParent = transform.parent; // Store Grid Layout as original parent
        }

        public void SetData(QuizData data)
        {
            Data = data;

            answer.text = data.Content;

            Canvas[] canvases = GetComponentsInParent<Canvas>();
            canvas = canvases[^1];

        }

        public QuizData GetData()
        {
            return Data;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent; // Store last valid parent
            transform.SetParent(canvas.transform, true); // Remove from Grid
            canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through
            canvasGroup.alpha = 0.6f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor; // Move item with cursor
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            if (transform.parent == canvas.transform)  // If not dropped in a valid place
            {
                transform.SetParent(originalParent, false); // Return to Grid
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            // This ensures only one item can be placed in a drop zone
        }
    }
}