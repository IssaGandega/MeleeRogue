using System;
using UnityEngine;

/// <summary>
/// Contract shared by every input source (human player, bot).
/// Knows nothing about the game state: only exposes intentions.
/// </summary>
public abstract class InputReader : MonoBehaviour
{
    // Continuous values: read by the Controller whenever it needs them.
    public Vector2 MoveDirection { get; private set; }
    public Vector3 AimPosition { get; private set; }

    // Punctual events: the Controller subscribes, decides what to do with the request.
    public event Action DashRequested;
    public event Action AttackRequested;

    /// <summary>
    /// The only way to write MoveDirection. Guarantees its length is never above 1,
    /// no matter which reader (keyboard, bot...) provides the raw value.
    /// </summary>
    protected void SetMoveDirection(Vector2 rawDirection)
    {
        MoveDirection = Vector2.ClampMagnitude(rawDirection, 1f);
    }

    protected void SetAimPosition(Vector3 worldPosition)
    {
        AimPosition = worldPosition;
    }

    protected void RaiseDashRequested() => DashRequested?.Invoke();
    protected void RaiseAttackRequested() => AttackRequested?.Invoke();
}