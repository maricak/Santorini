using System.Collections.Generic;
using UnityEngine;
using etf.santorini.km150096d.utils;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Block : MonoBehaviour
    {
        internal static void GenerateBlock(int x, int y, Board board)
        {
            GameObject gameObject = Instantiate(board.blockPrefab) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Block block = gameObject.GetComponent<Block>();
            Util.MoveBlock(block, x, y, (int)Tile.GetTile(x, y).Height - 1);
        }
    }
}

