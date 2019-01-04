using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public class AIEasyMove : Move
    {
        public static readonly float WIN_VALUE = 25f;
        public static readonly float LOSS_VALUE = -25f;

        private readonly int maxDepth;
        public AIEasyMove(PlayerID id, Board board, int maxDepth) : base(id, board)
        {
            this.maxDepth = maxDepth;
        }
        public override bool MouseInputNeeded()
        {
            return false;
        }
       /* public override void MakeMove(Vector2 position)
        {
            if (moveState == MoveState.POSITION_FIRST || moveState == MoveState.POSITION_SECOND)
            {
                // random positioning of builders
                position = RandomPosition();
                base.MakeMove(position);
            }
            else
            {
                Vector2[] bestMove = new Vector2[3];
                BoardState boardState = new BoardState(tiles:Tile.GetTiles(),
                                                        currentPlayer: id,
                                                        currentPositions: Player.GetPlayerPositions(id),
                                                        otherPlayer: 1 - id,
                                                        otherPositions: Player.GetPlayerPositions(1 - id),
                                                        currentDepth: 0
                                                       );

                Minimax(boardState, bestMove);

                base.MakeMove(bestMove[0]); // select
                base.MakeMove(bestMove[1]); // move
                base.MakeMove(bestMove[2]); // build
            }
        }*/
        private Vector2 RandomPosition()
        {
            return new Vector2(Random.Range(0, 5), Random.Range(0, 5));
        }

     /*   public float Minimax(BoardState board, Vector2[] bestMove)
        {
            if (board.CurrentWins())
            {
                return WIN_VALUE;
            }
            else if (board.OtherWins())
            {
                return LOSS_VALUE;
            }
            else if (board.currentDepth == maxDepth)
            {
                return board.Evaluate();
            }

            float bestScore = board.currentPlayer == id ? Mathf.NegativeInfinity : Mathf.Infinity;

            for (int k = 0; k < 2; k++)
            {
                Vector2 srcPosition = board.currentPositions[k];
                CalculatePossibleMoves(srcPosition, ref board.possibleMoves, board.tiles);

                // for all possible moves
                for (int i = 0; i < Board.DIM; i++)
                {
                    for (int j = 0; j < Board.DIM; j++)
                    {
                        if (board.possibleMoves[i, j])
                        {
                            Vector2 dstPosition = new Vector2(i, j);

                            // make move
                            board.MovePlayer(srcPosition, dstPosition);

                            //bool[,] possibleBuilds = new bool[Board.DIM, Board.DIM];
                            CalculatePossibleBuilds(dstPosition, ref board.possibleBuilds, board.tiles);

                            // for all possible builds after the move
                            for (int m = 0; m < Board.DIM; m++)
                            {
                                for (int n = 0; n < Board.DIM; n++)
                                {
                                    if (board.possibleBuilds[m, n])
                                    {
                                        Vector2 buildPosition = new Vector2(m, n);
                                        // build
                                        board.Build(buildPosition);

                                        Vector2[] currentMove = new Vector2[3];
                                        float currentScore = Minimax(new BoardState(
                                                                        tiles: board.tiles,
                                                                        currentPlayer: board.otherPlayer,
                                                                        currentPositions: board.otherPositions,
                                                                        otherPlayer: board.currentPlayer,
                                                                        otherPositions: board.currentPositions,
                                                                        currentDepth: board.currentDepth + 1),
                                                                    currentMove);

                                        if (board.currentPlayer == id)
                                        {
                                            if (currentScore > bestScore)
                                            {
                                                bestScore = currentScore;
                                                // TODO set bestMoves arrat
                                                bestMove[0] = srcPosition; // select
                                                bestMove[1] = dstPosition; // move
                                                bestMove[2] = buildPosition; // build
                                            }
                                        }
                                        else
                                        {
                                            if (currentScore < bestScore)
                                            {
                                                bestScore = currentScore;
                                                // TODO set bestMoves arrat
                                                bestMove[0] = srcPosition; // select
                                                bestMove[1] = dstPosition; // move
                                                bestMove[2] = buildPosition; // build
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return bestScore;
        }
  */  }
}