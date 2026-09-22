using UnityEngine;

public class TestController : MonoBehaviour
{
    private void Start()
    {
        InputReader inputReader = GetComponent<InputReader>();
        inputReader.DashRequested += () => Debug.Log("Dash reçu !");
    }
}
