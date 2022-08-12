using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InCombatGrid : MonoBehaviour
{
    public List<TilePath> tilePaths;
}

public class TilePath
{
    List<Tile> tilePath;
    bool isStraightLinePath;
}