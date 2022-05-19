using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IHoverable
{
    public GameObject hoverIndicator;

    public void OnHoverEnter()
    {
        hoverIndicator.SetActive(true);
    }
    public void OnHoverExit()
    {
        hoverIndicator.SetActive(false);
    }

}
