using UnityEngine;
using etf.santorini.km150096d.utils;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Block : MonoBehaviour
    {
        private static readonly Color[] colors = new Color[3] 
        {
            new Color(0, 0, 0),
            new Color(0.15f, 0.15f, 0.15f),
            new Color(0.3f, 0.3f, 0.3f)
        };
        public static void GenerateBlock(int x, int y, Board board, int height)
        {
            GameObject gameObject = Instantiate(board.blockPrefab) as GameObject;
            gameObject.transform.SetParent(board.transform);
            gameObject.GetComponent<Renderer>().material.color = colors[height - 1];
            Block block = gameObject.GetComponent<Block>();
            Util.MoveBlock(block, x, y, height - 1);
        }
    }
}

