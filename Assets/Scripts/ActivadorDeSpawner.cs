using UnityEngine;

public class ActivadorDeSpawner : MonoBehaviour
{
    [SerializeField] ArenaBase _arena;
    [SerializeField] GameManager _gameManager;

    public GameManager.GameState gameState;
    private void OnTriggerEnter(Collider other)
    {
        GameManager.instance.ChangeState(gameState);
        
        if (GameManager.instance.GetCurrentState() != GameManager.GameState.aStarState)
        {
            _gameManager.updateList = true;
            _arena.IniciarHorda();
        }

        gameObject.SetActive(false);

    }
}
