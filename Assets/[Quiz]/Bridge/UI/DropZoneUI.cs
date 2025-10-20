using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Anteena.Quiz.Bridge
{
    public class DropZoneUI : MonoBehaviour, IDropHandler
    {
        public DragAnswerUI Current;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null)
            {
                DragAnswerUI draggedItem = eventData.pointerDrag.GetComponent<DragAnswerUI>();

                if (draggedItem != null)
                {
                    if (Current != null)  // If drop zone already has an item, return it to grid
                    {
                        Current.transform.SetParent(draggedItem.transform.parent, false);
                    }

                    draggedItem.transform.SetParent(transform, false); // Snap to drop zone
                    draggedItem.transform.localPosition = Vector3.zero; // Align perfectly
                    Current = draggedItem; // Assign item to drop zone
                }
            }
        }
    }
}