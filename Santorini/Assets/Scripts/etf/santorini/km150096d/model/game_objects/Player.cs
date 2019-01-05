using UnityEngine;
using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Player : MonoBehaviour, IPlayer
    {

        #region Object fileds
        public PlayerID Id { get; set; }
        public Vector2 Position { get; set; }
        #endregion

        public static IPlayer GeneratePlayer(PlayerID id, Vector2 position, Board board)
        {
            // position
            int x = (int)position.x;
            int y = (int)position.y;

            //Tile tile = Tile.GetTile(x, y);
            // create object
            GameObject gameObject = Instantiate(board.playerPrefabs[(int)id]) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Player player = gameObject.GetComponent<Player>();

            // init object
            player.Id = id;
            player.Position = position;

            // set position
            Util.MovePlayer(player, x, y, 0);

            return player;
        }
    }
}