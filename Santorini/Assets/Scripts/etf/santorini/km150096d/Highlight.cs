using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.km150096d
{
    public class Highlight : MonoBehaviour
    {

        public static GameObject[,] highlights = new GameObject[Board.DIM, Board.DIM];
        public static void GenerateHighlight(int x, int y, Board board)
        {
            GameObject highlight = Instantiate(board.highlightPrefab) as GameObject;
            highlight.transform.SetParent(board.transform);
            highlights[x, y] = highlight;
            Util.MoveHighlight(highlight, x, y, (int)Tile.GetTile(x, y).Height);
            highlight.SetActive(false);
        }

        public static void SetHighlight(bool[,] possibleMoves)
        {
            for (int x = 0; x < Board.DIM; x++)
            {
                for (int y = 0; y < Board.DIM; y++)
                {
                    if (possibleMoves[x, y])
                    {
                        Util.MoveHighlight(highlights[x, y], x, y, (int)Tile.GetTile(x, y).Height);
                        highlights[x, y].SetActive(true);
                    }
                }
            }
        }

        public static void ResetHighlight()
        {
            for (int x = 0; x < Board.DIM; x++)
            {
                for (int y = 0; y < Board.DIM; y++)
                {
                    highlights[x, y].SetActive(false);
                }
            }
        }
    }
}