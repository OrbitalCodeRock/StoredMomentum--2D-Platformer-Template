using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public static GameObject selectedObject;

    public bool isHovering;

    public GameObject selectionIndicator;

    private void Update()
    {
        if(this.gameObject != selectedObject && !isHovering)
        {
            selectionIndicator.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        selectionIndicator.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventdata)
    {
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventdata)
    {
        if (isHovering && eventdata.button == PointerEventData.InputButton.Left)
        {
            selectedObject = this.gameObject;
        }
    }
}
