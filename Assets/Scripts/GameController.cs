using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;
public class GameController : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private TetrisShape[] shapePrefabs;

    private bool isGameOver;

    private Vector2Int centerCell;
    TetrisShape currentShape;

    private IEnumerator Start ()
    {
        centerCell = new Vector2Int(CircularGridSpawner.Instance.gridWidth / 2, CircularGridSpawner.Instance.gridHeight / 2);
        yield return new WaitForSeconds(2f);

        StartGame();
    }
    private void Update ()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            TryMoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            TryMoveRight();
        }
    }
    private void TryMoveLeft ()
    {
        if (currentShape == null) return;
        if(CircularGridSpawner.Instance.CanMoveShapeInDirection(currentShape, Direction.Left))
        {
            currentShape.transform.position += new Vector3(-1, 0, 0);
            currentShape.SetPosOnGrid(currentShape.CurrentPosOnGrid + new Vector2Int(-1, 0));
        }
    }
    private void TryMoveRight ()
    {
        if (currentShape == null) return;
        if (CircularGridSpawner.Instance.CanMoveShapeInDirection(currentShape, Direction.Right))
        {
            currentShape.transform.position += new Vector3(1, 0, 0);
            currentShape.SetPosOnGrid(currentShape.CurrentPosOnGrid + new Vector2Int(1, 0));
        }
    }
    private void StartGame ()
    {
        StartCoroutine(ResumeGame());
    }

    private IEnumerator ResumeGame ()
    {
        currentShape = SpawnShape();
        currentShape.SetPosOnGrid(centerCell);

        StartCoroutine(MoveShape(currentShape));

        yield return new WaitUntil(() => currentShape.IsPlaced);
        Destroy(currentShape.gameObject);
        if (!isGameOver)
        {
            StartCoroutine(ResumeGame());
        }
    }
    private IEnumerator MoveShape(TetrisShape _shape)
    {
        

        yield return new WaitForSeconds(0.5f);

        if (!CanMoveNext(_shape))
        {
            PlaceShape(_shape);
        }
        else
        {
            _shape.transform.position += new Vector3(0, 0, 1);
            _shape.SetPosOnGrid(_shape.CurrentPosOnGrid + new Vector2Int(0, 1));
            //switch (CircularGridSpawner.Instance.CurrentDirection)
            //{
            //    case Direction.Up:
            //        break;
            //}
            StartCoroutine(MoveShape(_shape));
        }
    }
    private void PlaceShape(TetrisShape _shape)
    {
        foreach(var _cell in _shape.CellsOnGrid)
        {
            Vector2Int cellPos = _shape.CurrentPosOnGrid + _cell.CellOffset;
            GridCell gridCell = CircularGridSpawner.Instance.GetCell(cellPos);
            gridCell.SetIsOccuped(_cell);
        }
        _shape.SetIsPlaced();
    }

    private bool CanMoveNext (TetrisShape _shape)
    {
        bool found = true;
        foreach(var _cell in _shape.CellsOnGrid)
        {
            if(!CircularGridSpawner.Instance.HasNextDestinationInCurrentDirection(_cell.CellOffset + _shape.CurrentPosOnGrid))
            {
                found = false;
                break;
            }
        }
        return found;

    }
    private TetrisShape SpawnShape ()
    {
        TetrisShape chosenShape = shapePrefabs[Random.Range(0, shapePrefabs.Length)];

        TetrisShape temp = Instantiate(chosenShape);
        temp.transform.position = CircularGridSpawner.Instance.GetCell(centerCell).transform.position;
        //temp.transform.position = spawnPos;
        return temp;
    }
}
