using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.model.plain_objects;
using etf.santorini.km150096d.utils;
using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public class MediumMove : AIMove
    {

        public static readonly float WIN_VALUE = 150f;
        public static readonly float LOSS_VALUE = -150f;

        public MediumMove(PlayerID id, IBoard board) : base(id, board)
        {
            this.type = MoveType.MEDIUM;
        }
        
        // ALPHA BETA MINIMAX
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
                return evaluationValue;
            }

            float bestScore = ((maxDepth - currentDepth) % 2 == 1) ? Mathf.NegativeInfinity : Mathf.Infinity;

            List<AIMove> moves = GetAllPossibleMoves();

            foreach (MediumMove move in moves)
            {
                Vector2[] currentMove = new Vector2[3];
                currentMove[0] = move.move[0]; // select
                currentMove[1] = move.move[1]; // move
                currentMove[2] = move.move[2]; // build

                //Debug.Log("BOARD STATE select(" + currentMove[0].x + "," + currentMove[0].y + ")" + "move(" + currentMove[1].x + "," + currentMove[1].y + ")" + "build(" + currentMove[2].x + "," + currentMove[2].y + ") -- Val:" + move.evaluationValue);

                float currentScore = move.Algorithm(currentMove, currentDepth + 1, player, alpha, beta);

                //Debug.Log("ALGO RESULT select(" + currentMove[0].x + "," + currentMove[0].y + ")" + "move(" + currentMove[1].x + "," + currentMove[1].y + ")" + "build(" + currentMove[2].x + "," + currentMove[2].y + ") -- Val:" + move.evaluationValue);

                if (currentDepth == 0 && board is Board && board.Simulation)
                {
                    (board as Board).AddToSimulationLog(
                       "(" + currentMove[0].x + "," + currentMove[0].y + ")" +
                       "(" + currentMove[1].x + "," + currentMove[1].y + ")" +
                       "(" + currentMove[2].x + "," + currentMove[2].y + ") \t" + currentScore);
                }
                if ((maxDepth - currentDepth) % 2 == 1) // odd - max 
                {
                    if (currentScore > bestScore)
                    {
                        bestScore = currentScore;
                        bestMove[0] = move.move[0]; // select
                        bestMove[1] = move.move[1]; // move
                        bestMove[2] = move.move[2]; // build

                        //Debug.Log("MAX: select(" + bestMove[0].x + "," + bestMove[0].y + ")" + "move(" + bestMove[1].x + "," + bestMove[1].y + ")" + "build(" + bestMove[2].x + "," + bestMove[2].y + ") -- Val:" + bestScore);
                        alpha = Mathf.Max(alpha, bestScore);
                        if (bestScore >= beta)
                        {
                            Debug.Log("ODSECANJE beta=" + beta + "alpha=" + alpha + "current" + currentScore);
                            return bestScore;
                        }
                    }
                }
                else
                {
                    if (currentScore < bestScore)
                    {
                        bestScore = currentScore;
                        bestMove[0] = move.move[0]; // select
                        bestMove[1] = move.move[1]; // move
                        bestMove[2] = move.move[2]; // build

                        //Debug.Log("MIN: select(" + bestMove[0].x + "," + bestMove[0].y + ")" + "move(" + bestMove[1].x + "," + bestMove[1].y + ")" + "build(" + bestMove[2].x + "," + bestMove[2].y + ") -- Val:" + bestScore);
                        beta = Mathf.Min(beta, bestScore);
                        if (bestScore <= alpha)
                        {
                               Debug.Log("ODSECANJE alpha=" + alpha + "beta=" + beta + "current" + currentScore);
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
