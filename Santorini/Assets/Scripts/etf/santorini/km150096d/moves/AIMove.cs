using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.model.plain_objects;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public abstract class AIMove : Move
    {
        static protected int cnt = 0;

        protected readonly int maxDepth;
        protected readonly Vector2[] bestMove = new Vector2[3];
        private int moveCount = 0;

        public AIMove(PlayerID id, IBoard board) : base(id, board)
        {
            maxDepth = board.MaxDepth;
        }

        public override bool MouseInputNeeded()
        {
            return board.Simulation;
            //return false;
        }

        public sealed override void MakeMove(Vector2 position)
        {
            if (moveState == MoveState.POSITION_FIRST || moveState == MoveState.POSITION_SECOND)
            {
                // random positioning of builders
                position = RandomPosition();
                base.MakeMove(position);
            }
            else
            {
                if (moveCount == 0)
                {
                    float alpha = Mathf.NegativeInfinity;
                    float beta = Mathf.Infinity;

                    if(board is Board)
                    {
                        (board as Board).ResetSimulationLog();
                    }
                    
                    var bestValue = Algorithm(bestMove, 0, id, alpha, beta);

                    if (board is Board && board.Simulation)
                    {
                        (board as Board).AddToSimulationLog("BEST MOVE: " + bestValue +
                            "\n\tselect(" + bestMove[0].x + "," + bestMove[0].y + ")" +
                            "\n\tmove(" + bestMove[1].x + "," + bestMove[1].y + ")" +
                            "\n\tbuild(" + bestMove[2].x + "," + bestMove[2].y + ")");
                    }

                    Debug.Log(cnt);
                    cnt = 0;
                }
                base.MakeMove(bestMove[moveCount]); // select
                moveCount = (moveCount + 1) % 3;
            }
        }

        protected abstract float Algorithm(Vector2[] bestMove, int currentDepth, PlayerID player, float alpha, float beta);
        protected abstract float Evaluate(Vector2[] bestMove);
        private Vector2 RandomPosition()
        {
            return new Vector2(Random.Range(0, 5), Random.Range(0, 5));
        }

        protected AIMove CopyBoardAndCreateMove(Move other, MoveType type)
        {
            IBoard currentBoard = new BoardPO(other.board);
            Move m = CreateMove(type, id, currentBoard);
            m.CopyPossibleMoves(other);
            return m as AIMove;
        }
    }
}
