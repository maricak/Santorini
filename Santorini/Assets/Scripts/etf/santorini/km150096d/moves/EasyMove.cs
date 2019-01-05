using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.model.plain_objects;
using etf.santorini.km150096d.utils;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public class EasyMove : AIMove
    {        
        public static readonly float WIN_VALUE = 25f;
        public static readonly float LOSS_VALUE = -25f;

        public EasyMove(PlayerID id, IBoard board) : base(id, board) { }    
       

        // MINIMAX
        protected override float Algorithm(Vector2[] bestMove, int currentDepth, PlayerID player, float alpha, float beta)
        {
            if (IsWinner())
            {
                return WIN_VALUE;
            }
            else if (!HasPossibleMoves())
            {
                return LOSS_VALUE;
            }
            else if (currentDepth == maxDepth)
            {
                var val = Evaluate(bestMove);
                //Debug.Log("EVAL: select(" + bestMove[0].x + "," + bestMove[0].y + ")" + "move(" + bestMove[1].x + "," + bestMove[1].y + ")" + "build(" + bestMove[2].x + "," + bestMove[2].y + ") -- Val:" + val);
                cnt++;
                return val;
            }

            float bestScore = (id == player) ? Mathf.NegativeInfinity : Mathf.Infinity;

            // za svakog graditelja
            for (int k = 0; k < 2; k++)
            {
                //select player
                Vector2 srcPosition = board[id, k].Position;
                EasyMove mSelect = CopyBoardAndCreateMove(this, MoveType.EASY) as EasyMove;
                mSelect.SelectPlayer(srcPosition);

                // for all possible moves
                for (int i = 0; i < BoardPO.DIM; i++)
                {
                    for (int j = 0; j < BoardPO.DIM; j++)
                    {
                        if (mSelect.possibleMoves[i, j])
                        {
                            // make move
                            Vector2 dstPosition = new Vector2(i, j);
                            EasyMove mMove = CopyBoardAndCreateMove(mSelect, MoveType.EASY) as EasyMove;
                            mMove.MovePlayer(dstPosition);

                            // for all possible builds after the move
                            for (int m = 0; m < BoardPO.DIM; m++)
                            {
                                for (int n = 0; n < BoardPO.DIM; n++)
                                {
                                    if (mMove.possibleMoves[m, n])
                                    {
                                        // build
                                        Vector2 buildPosition = new Vector2(m, n);
                                        EasyMove mBuild = CopyBoardAndCreateMove(mMove, MoveType.EASY) as EasyMove;
                                        mBuild.Build(buildPosition);

                                        //mBuild.board.ChangeTurn();
                                        mBuild.id = 1 - mBuild.id;

                                        Vector2[] currentMove = new Vector2[3];
                                        currentMove[0] = srcPosition; // select
                                        currentMove[1] = dstPosition; // move
                                        currentMove[2] = buildPosition;


                                        float currentScore = mBuild.Algorithm(currentMove, currentDepth + 1, player, alpha, beta);

                                        mBuild = null;

                                        if (id == player)
                                        {
                                            if (currentScore > bestScore)
                                            {
                                                bestScore = currentScore;
                                                bestMove[0] = srcPosition; // select
                                                bestMove[1] = dstPosition; // move
                                                bestMove[2] = buildPosition; // build
                                               // Debug.Log("MAX: select(" + bestMove[0].x + "," + bestMove[0].y + ")" + "move(" + bestMove[1].x + "," + bestMove[1].y + ")" + "build(" + bestMove[2].x + "," + bestMove[2].y + ") -- Val:" + bestScore);
                                            }
                                        }
                                        else
                                        {
                                            if (currentScore < bestScore)
                                            {
                                                bestScore = currentScore;
                                                bestMove[0] = srcPosition; // select
                                                bestMove[1] = dstPosition; // move
                                                bestMove[2] = buildPosition; // build

                                                //Debug.Log("MIN: select(" + bestMove[0].x + "," + bestMove[0].y + ")" + "move(" + bestMove[1].x + "," + bestMove[1].y + ")" + "build(" + bestMove[2].x + "," + bestMove[2].y + ") -- Val:" + bestScore);

                                            }
                                        }
                                    }
                                }
                            }

                            mMove = null;
                        }
                    }
                }

                mSelect = null;
            }
            return bestScore;
        }
        protected override float Evaluate(Vector2[] move)
        {
            return (int)board[move[1]].Height - 1 + ((int)board[move[2]].Height - 1) * PlayerDistance(move[2]);
        }

        private float PlayerDistance(Vector2 position)
        {
            return Util.Distance(board[id, 0].Position, position)
                 + Util.Distance(board[id, 1].Position, position)
                 - Util.Distance(board[1 - id, 0].Position, position)
                 - Util.Distance(board[1 - id, 1].Position, position);
        }      
    }
}