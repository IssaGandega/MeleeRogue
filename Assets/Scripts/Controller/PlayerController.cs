using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public enum PlayerState
    {
        Idle,
        Dashing, 
        Attacking,
    }
    
    public PlayerState State { get; private set; } =  PlayerState.Idle;
    InputReader inputReader;
    //[SerializeField] private ScriptableObject playerStats;
    [SerializeField] private float playerSpeed = 5;
    
    
    void Awake()
    {
        inputReader = GetComponent<InputReader>();
    }

    private void OnEnable()
    {
        inputReader.DashRequested += Dash;
        inputReader.AttackRequested += Attack;
    }

    private void OnDisable()
    {
        inputReader.DashRequested -=  Dash;
        inputReader.AttackRequested -= Attack;

    }

    private void Dash()
    {
        State = PlayerState.Dashing;
    }

    private void Attack()
    {
        State = PlayerState.Attacking;
    }

    private void Update()
    {
        if (inputReader.MoveDirection != Vector2.zero)
        {
            transform.position += (Vector3)(inputReader.MoveDirection * (playerSpeed * Time.deltaTime));
        }
        
    }
}
