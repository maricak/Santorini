using UnityEngine;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model;

namespace etf.santorini.km150096d.moves
{
    public class AISimpleMove : Move
    {
      //  private readonly int maxDepth;
        public AISimpleMove(PlayerID id, Board board, int maxDepth) : base(id, board)
        {
          //  this.maxDepth = maxDepth;
        }
        public override bool MouseInputNeeded()
        {
            return false;
        }
        public override void MakeMove(Vector2 position)
        {
            // position = ??
            if (moveState == MoveState.POSITION_FIRST || moveState == MoveState.POSITION_SECOND)
            {
                // random positioning of builders
                position = RandomPosition();
            }
            else
            {
                // minimax                
                MiniMax(
                    Tile.GetTiles(),
                    id,
                    new Vector2[2] { Player.GetPlayer(id, 0).Position, Player.GetPlayer(id, 1).Position },
                    0,
                    ref position);
            }
            position = FileManager.Instance.NextPosition();
            base.MakeMove(position);
        }
        private Vector2 RandomPosition()
        {
            return new Vector2(Random.Range(0, 5), Random.Range(0, 5));
        }
        public float MiniMax(Tile[,] tiles,
            PlayerID currentId, // his move in the minimax tree
            Vector2[] positions,
            int currentDepth,
            ref Vector2 bestMove)
        {
            return 0;
        }
    }
}