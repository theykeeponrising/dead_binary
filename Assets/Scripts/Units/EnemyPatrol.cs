using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    private List<Tile> _patrolTiles = new();
    private bool _reversePath = false;
    [SerializeField] private PatrolType _patrolType;

    private void Start()
    {
        InitPatrolPoints();
        DrawPatrolPath();
    }

    private void InitPatrolPoints()
    {
        // Convert patrol node transforms into tiles, assign tiles to the list

        _patrolTiles = new();
        List<Transform> patrolNodes = GetComponentsInChildren<Transform>().ToList();

        foreach (Transform patrolNode in patrolNodes.GetRange(1, patrolNodes.Count - 1))
        {
            _patrolTiles.Add(Map.MapGrid.GetTile(patrolNode.position));
            patrolNode.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void DrawPatrolPath()
    {
        // Used to visualize patrol paths in the scene view

        int i = 0;
        int lastIndex = _patrolTiles.Count - 1;

        while (i < lastIndex)
        {
            Debug.DrawLine(_patrolTiles[i].transform.position, _patrolTiles[i + 1].transform.position, Color.cyan, Mathf.Infinity);
            i++;
        }

        if (_patrolType == PatrolType.CIRCULAR)
        {
            Debug.DrawLine(_patrolTiles[lastIndex].transform.position, _patrolTiles[0].transform.position, Color.cyan, Mathf.Infinity);
        }
    }

    public Tile GetNearestPatrolTile(Unit unit)
    {
        if (_patrolTiles.Count == 0)
            InitPatrolPoints();

        List<Tile> patrolPoints = new();
        patrolPoints.AddRange(_patrolTiles);

        patrolPoints.Sort(delegate (Tile a, Tile b)
        {
            return Vector2.Distance(unit.transform.position, a.transform.position).CompareTo(
                Vector2.Distance(unit.transform.position, b.transform.position));

        });
        return patrolPoints[0];
    }

    private Tile GetNextPatrolTile(Tile previousTile)
    {
        int currentIndex = _patrolTiles.IndexOf(previousTile);
        int nextIndex = currentIndex + 1;

        // Patrol type linear -- Go back and forth between patrol points
        switch (_patrolType)
        {
            case (PatrolType.LINEAR):
            {
                if (currentIndex == _patrolTiles.Count - 1) _reversePath = true;
                else if (currentIndex == 0)  _reversePath = false;

                nextIndex = _reversePath ? currentIndex - 1 : currentIndex + 1;
                break;
            }
            // Patrol type circular -- Patrol in a continuous path
            case (PatrolType.CIRCULAR):
            {
                if (currentIndex == _patrolTiles.Count - 1) nextIndex = 0;
                break;
            }
            // Patrol type finite -- Once we reach destination, stay there
            case (PatrolType.FINITE):
            {
                if (currentIndex == _patrolTiles.Count - 1) nextIndex = currentIndex;
                break;
            }
        }

        return _patrolTiles[nextIndex];
    }

    public Tile GetPatrolTile(Unit unit, Tile tile)
    {
        Tile nextTile = tile;

        if (IsDestinationReached(unit, tile))
            nextTile = GetNextPatrolTile(tile);

        return nextTile;
    }

    public bool IsDestinationReached(Unit unit, Tile tile)
    {
        float distance = Vector3.Distance(unit.transform.position, tile.transform.position);
        return distance <= MapGrid.TileSpacing * 2;
    }

    private enum PatrolType { LINEAR, CIRCULAR, FINITE }
}
