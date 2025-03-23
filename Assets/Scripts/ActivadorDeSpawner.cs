using UnityEngine;


public class ActivadorDeSpawner : MonoBehaviour
{
    [SerializeField] ArenaBase _arena;
    [SerializeField] GameManager _gameManager;
    GOAPPlanner _goapPlanner;
    public GameManager.GameState gameState;


    private void OnTriggerEnter(Collider other)
    {
        GameManager.instance.ChangeState(gameState);
        
        if (GameManager.instance.GetCurrentState() != GameManager.GameState.aStarState)
        {
            _gameManager.updateList = true;
            _arena.IniciarHorda();
        }
        _goapPlanner = FindObjectOfType<GOAPPlanner>();
        if (_goapPlanner != null)
        {
            _goapPlanner.StartCoroutine(_goapPlanner.Plan());
            Debug.Log("Planificando");
        }

        gameObject.SetActive(false);

    }
}
