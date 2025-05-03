using System.Collections;
using UnityEngine;
public class GameController : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private TetrisShape[] shapePrefabs;

    private bool isGameOver;

    private void Start ()
    {
        StartGame();
    }

    private void StartGame ()
    {
        StartCoroutine(ResumeGame());
    }

    private IEnumerator ResumeGame ()
    {
        TetrisShape currentShape = SpawnShape();

        yield return new WaitUntil(() => currentShape.IsPlaced);

        if (!isGameOver)
        {
            StartCoroutine(ResumeGame());
        }
    }

    private TetrisShape SpawnShape ()
    {
        TetrisShape chosenShape = shapePrefabs[Random.Range(0, shapePrefabs.Length - 1)];

        TetrisShape temp = Instantiate(chosenShape, transform);
        temp.transform.position = spawnPos;
        return temp;
    }
}
