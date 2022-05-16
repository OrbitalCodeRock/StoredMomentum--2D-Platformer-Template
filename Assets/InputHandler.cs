using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;

    private GameControls controls;

    public class InputArgs
    {
        public InputAction.CallbackContext context;
    }

    public Action<InputArgs> OnJumpPressed;
    public Action<InputArgs> OnJumpReleased;

    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        controls = new GameControls();

        controls.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        controls.Player.JumpStart.performed += ctx => OnJumpPressed(new InputArgs { context = ctx });
        controls.Player.JumpEnd.performed += ctx => OnJumpReleased(new InputArgs { context = ctx });
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
