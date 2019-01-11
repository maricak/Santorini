using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.utils;
using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public class MediumMove : AIMove
    {
        public MediumMove(PlayerID id, IBoard board) : base(id, board)
        {
            this.type = MoveType.MEDIUM;
        }

        // ALPHA BETA MINIMAX
        protected override float Algorithm(Vector2[] bestMove, int currentDepth, float alpha, float beta)
        {
            if (currentDepth == maxDepth)
            {
                return evaluationValue;
            }

            float bestScore = ((maxDepth - currentDepth) % 2 == 1) ? Mathf.NegativeInfinity : Mathf.Infinity;

            List<AIMove> moves = GetAllPossibleMoves();

            if (moves.Count == 0) // nema poteza
            {
                bestScore = ((maxDepth - currentDepth) % 2 == 1) ? LOSS_VALUE * (maxDepth - currentDepth + 1) : WIN_VALUE * (maxDepth - currentDepth + 1);
            }

            foreach (MediumMove move in moves)
            {
                //Debug.Log("hard Moves");
                Vector2[] currentMove = new Vector2[3];
                currentMove[0] = move.move[0]; // select
                currentMove[1] = move.move[1]; // move
                currentMove[2] = move.move[2]; // build

                float currentScore;
                if (move.isWinner) // pobedio je
                {
                    currentScore = ((maxDepth - currentDepth) % 2 == 1) ? WIN_VALUE * (maxDepth - currentDepth + 1) : LOSS_VALUE * (maxDepth - currentDepth + 1);
                }
                else
                {
                    currentScore = move.Algorithm(currentMove, currentDepth + 1, alpha, beta);
                }

                if (currentDepth == 0 && board is Board && board.Simulation)
                {
                    (board as Board).AddToSimulationLog(
                       "(" + move.move[0].x + "," + move.move[0].y + ")" +
                       "(" + move.move[1].x + "," + move.move[1].y + ")" +
                       "(" + move.move[2].x + "," + move.move[2].y + ") \t" + currentScore);
                }

                if ((maxDepth - currentDepth) % 2 == 1) // odd - max 
                {
                    if (currentScore > bestScore)
                    {
                        bestScore = currentScore;
                        bestMove[0] = move.move[0]; // select
                        bestMove[1] = move.move[1]; // move
                        bestMove[2] = move.move[2]; // build

                        alpha = Mathf.Max(alpha, bestScore);
                        if (bestScore >= beta)
                        {
                            //Debug.Log("ODSECANJE beta=" + beta + "alpha=" + alpha + "current" + currentScore);
                            return bestScore;
                        }
                    }
                }
                else // min
                {
                    if (currentScore < bestScore)
                    {
                        bestScore = currentScore;
                        bestMove[0] = move.move[0]; // select
                        bestMove[1] = move.move[1]; // move
                        bestMove[2] = move.move[2]; // build

                        beta = Mathf.Min(beta, bestScore);
                        if (bestScore <= alpha)
                        {
                            //Debug.Log("ODSECANJE alpha=" + alpha + "beta=" + beta + "current" + currentScore);
                            return bestScore;
                        }
                    }
                }
            }
            moves = null;

            return bestScore;
        }
        protected override float Evaluate(Vector2[] move)
        {
            return (int)board[move[1]].Height - 1 + ((int)board[move[2]].Height - 1) * PlayerDistance(move[2]);
        }

        private float PlayerDistance(Vector2 position)
        {
            return +Util.Distance(board[id, 0].Position, position)
                 + Util.Distance(board[id, 1].Position, position)
                 - Util.Distance(board[1 - id, 0].Position, position)
                 - Util.Distance(board[1 - id, 1].Position, position);
        }
    }
}
