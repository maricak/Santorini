using UnityEngine;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.model.gameobject
{


    public class Tile : MonoBehaviour, ITile
    {
        #region Constants
        public static readonly float SIZE = 1.0f;
        public static readonly float DISTANCE = SIZE / 10;
        public static readonly Vector3 OFFSET = new Vector3(SIZE / 2, 0, SIZE / 2);
        #endregion

        #region Game objects
        private static readonly Tile[,] tiles = new Tile[Board.DIM, Board.DIM];
        public static Tile GetTile(int x, int y)
        {
            return tiles[x, y];
        }
        public static Tile[,] GetTiles()
        {
            return tiles;
        }
        #endregion

        #region Object fileds
        public Height Height { get; set; }
        public IPlayer Player { get; set; }
        public bool HasPlayer()
        {
            return Player != null;
        }
        #endregion

        private void Start()
        {
            Height = Height.H0;
            Player = null;
        }

        #region Generate
        public static void GenerateTile(int x, int y, Board board)
        {
            GameObject gameObject = Instantiate(board.tilePrefab) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Tile tile = gameObject.GetComponent<Tile>();
            tiles[x, y] = tile;
            Util.MoveTile(tile, x, y, 0);
        }
        #endregion
    }
}