using UnityEngine;

namespace etf.santorini.km150096d
{
    public enum Height { H0 = 1, H1, H2, H3, ROOF };

    public class Tile : MonoBehaviour
    {
        public static readonly float SIZE = 1.0f;
        //    public static readonly float OFFSET = SIZE / 2;
        public static readonly float DISTANCE = SIZE / 10;
        public static readonly Vector3 OFFSET = new Vector3(SIZE / 2, 0, SIZE / 2);

        private static readonly Tile[,] tiles = new Tile[Board.DIM, Board.DIM];

        public static Tile GetTile(int x, int y)
        {
            return tiles[x, y];
        }

        public static Tile[,] GetTiles()
        {
            return tiles;
        }

        public Height Height { get; set; }
        public Player Player { get; set; }
        public bool HasPlayer()
        {
            return Player != null;
        }

        private void Start()
        {
            Height = Height.H0;
            Player = null;
        }
        public static void GenerateTile(int x, int y, Board board)
        {
            GameObject gameObject = Instantiate(board.tilePrefab) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Tile tile = gameObject.GetComponent<Tile>();
            tiles[x, y] = tile;
            Util.MoveTile(tile, x, y, 0);
        }
    }
}