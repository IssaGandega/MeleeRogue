using Controller;
using Scriptables.Events;
using UnityEngine;

namespace Managers
{
    // Runs its Awake before every other script's, so anyone can safely read
    // GameManager.Instance from their own Awake (not just Start).
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private HitEventChannel hitEventChannel;

        public PlayerController Player => player;
        public HitEventChannel HitEventChannel => hitEventChannel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (player == null)
            {
                player = FindAnyObjectByType<PlayerController>();
                if (player == null)
                    Debug.LogError("GameManager: no PlayerController found in the scene.", this);
            }

            if (hitEventChannel == null)
                Debug.LogError("GameManager: HitEventChannel not assigned.", this);
        }
    }
}
