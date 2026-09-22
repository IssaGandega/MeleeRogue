using UnityEngine;
using UnityEngine.InputSystem;

public class KBMReader : InputReader
{
    private KeyboardInputActions actions;

    private void Awake()
    {
        // Created once. Not in OnEnable: OnEnable can run several times
        // if this object gets disabled and re-enabled later.
        actions = new KeyboardInputActions();
    }

    private void OnEnable()
    {
        actions.Player.Enable(); // without this, no callback ever fires
        actions.Player.Attack.performed += HandleAttack;
        actions.Player.Dash.performed += HandleDash;
    }

    private void OnDisable()
    {
        actions.Player.Attack.performed -= HandleAttack;
        actions.Player.Dash.performed -= HandleDash;
        actions.Player.Disable();
    }

    private void OnDestroy()
    {
        actions.Dispose(); // releases native resources held by the generated class
    }

    private void Update()
    {
        // Continuous values: read every frame, not driven by events.
        SetMoveDirection(actions.Player.Move.ReadValue<Vector2>());
        SetAimPosition(MouseWorldPosition());
    }

    private void HandleAttack(InputAction.CallbackContext ctx) => RaiseAttackRequested();
    private void HandleDash(InputAction.CallbackContext ctx) => RaiseDashRequested();

    private Vector3 MouseWorldPosition()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}