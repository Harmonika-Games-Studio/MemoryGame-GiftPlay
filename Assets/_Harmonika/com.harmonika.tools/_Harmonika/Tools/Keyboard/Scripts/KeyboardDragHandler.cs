using UnityEngine;
using UnityEngine.EventSystems;

namespace Harmonika.Tools.Keyboard
{
    public class KeyboardDragHandler : MonoBehaviour, IDragHandler
    {
        #region IDragHandler implementation

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
        }

        #endregion
    }
}