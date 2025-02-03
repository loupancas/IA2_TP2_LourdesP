using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        fsmState = 0,
        gridState = 1,
        aStarState = 2
    }

    public static GameManager instance;

    [SerializeField]
    private GameState currentState;

    [Header("Components")]
    public FirstPersonPlayer pj;
    public ArenaFSM arenaFSM;
    public ArenaGrid arenaGrid;
    [SerializeField] private float distanceThreshold = 20f;
    [SerializeField] private float checkInterval = 3f;
    private float timeSinceLastCheck = 0f;
    public List<EnemigoVolador> enemies;
    public bool updateList = false;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            EnemigoVolador.InitializeGrid();
        }
    }
    private void Start()
    {
    }
    private void Update()
    {
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkInterval)
        {
            timeSinceLastCheck = 0f;
            if (GameManager.instance.IsInGridState())
            {
                UpdateDistanceEnemies();
                FuseNearbyEnemies();
            }
        }
    }

    private void UpdateDistanceEnemies()
    {
        if (updateList)
        {
            enemies = new List<EnemigoVolador>(FindObjectsOfType<EnemigoVolador>());
            updateList = false;
        }

        EnemigoVolador.DeactivateEnemiesByDistance(pj, enemies, distanceThreshold);
    }

    private void FuseNearbyEnemies()
    {
        if (updateList)
        {
            enemies = new List<EnemigoVolador>(FindObjectsOfType<EnemigoVolador>());
            updateList = false;
        }

        foreach (var enemigo in enemies)
        {
            var cellPosition = EnemigoVolador._spatialGrid.GetPositionInGrid(enemigo.Position);
            EnemigoVolador.FuseEnemiesInRange(enemigo.Position, EnemigoVolador._spatialGrid.cellWidth); // Usa el tamaño de la celda para el rango
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log("Estado cambiado a: " + currentState);
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    public bool IsInGridState()
    {
        return currentState == GameState.gridState;
    }
}