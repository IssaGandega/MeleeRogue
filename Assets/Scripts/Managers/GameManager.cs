using Controller;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player;
        public PlayerController Player => player;

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
        }
    }
}
