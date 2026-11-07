using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSelection : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject outline;

    public void OnSelect(BaseEventData eventData)
    { outline.SetActive(true); }

    public void OnDeselect(BaseEventData eventData)
    { outline.SetActive(false); }
}
