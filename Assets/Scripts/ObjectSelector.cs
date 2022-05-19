using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class ObjectSelector : MonoBehaviour
{
    public static ObjectSelector instance;

    public GameObject SelectedObject { get; private set; }
    public GameObject HoveredObject { get; private set; }

    [SerializeField]
    private Camera mainCamera;

    public InputAction mouseClick;

    private void OnEnable()
    {
        SelectedObject?.GetComponent<IHoverable>()?.OnHoverEnter();
        mouseClick.Enable();
        mouseClick.performed += MousePressed;
    }

    private void OnDisable()
    {
        mouseClick.performed -= MousePressed;
        mouseClick.Disable();
        SelectedObject?.GetComponent<IHoverable>()?.OnHoverExit();
        HoveredObject?.GetComponent<IHoverable>()?.OnHoverExit();
    }

    private void MousePressed(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        if (hit.collider?.GetComponent<IHoverable>() != null && hit.collider?.gameObject != SelectedObject)
        {
            SelectedObject?.GetComponent<IHoverable>().OnHoverExit();
            SelectedObject = hit.collider?.gameObject;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void Deselect()
    {
        SelectedObject?.GetComponent<IHoverable>()?.OnHoverExit();
        SelectedObject = null;
    }

    private void FixedUpdate()
    {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            GameObject hitObject = hit.collider?.gameObject;
            if(HoveredObject?.GetComponent<IHoverable>() != null && HoveredObject != SelectedObject && hitObject != HoveredObject.gameObject)
            {
                HoveredObject.GetComponent<IHoverable>().OnHoverExit();
            }
            HoveredObject = hit.collider?.gameObject;
            if(HoveredObject?.GetComponent<IHoverable>() != null)
            {
                HoveredObject.GetComponent<IHoverable>().OnHoverEnter();
            }
    }
}
