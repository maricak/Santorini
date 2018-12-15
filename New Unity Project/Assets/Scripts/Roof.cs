﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roof : MonoBehaviour {


    private static readonly List<Roof> roofs = new List<Roof>();
    internal static void GenerateRoof(int x, int y, Board board)
    {
        GameObject gameObject = Instantiate(board.roofPrefab) as GameObject;
        gameObject.transform.SetParent(board.transform);
        Roof roof = gameObject.GetComponent<Roof>();
        roofs.Add(roof);
        Util.MoveRoof(roof, x, y, (int)Tile.GetTile(x, y).Height);
    }
}
