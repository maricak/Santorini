using System.Collections.Generic;
using UnityEngine;
using etf.santorini.km150096d.utils;

namespace etf.santorini.km150096d.model
{
    public class Block : MonoBehaviour
    {
        private static readonly List<Block> blocks = new List<Block>();
        internal static void GenerateBlock(int x, int y, Board board)
        {
            GameObject gameObject = Instantiate(board.blockPrefab) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Block block = gameObject.GetComponent<Block>();
            blocks.Add(block);
            Util.MoveBlock(block, x, y, (int)Tile.GetTile(x, y).Height - 1);
        }
    }
}

