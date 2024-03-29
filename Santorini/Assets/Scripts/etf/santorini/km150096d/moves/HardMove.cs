﻿using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.utils;
using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public class HardMove : AIMove
    {
        public HardMove(PlayerID id, IBoard board) : base(id, board)
        {
            type = MoveType.HARD;
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

            foreach (HardMove move in moves)
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
                else
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


        // poziva se nakon kreiranja navog stanja, id je id pleyera koji je napravio potez
        protected override float Evaluate(Vector2[] move)
        {
            //Debug.Log("WINNING pos" + winningPlayer.Position.x + winningPlayer.Position.y);
            // win!
            if (board[move[1]].Height == Height.H3)
            {
                //Debug.Log("WIN" + WIN_VALUE);
                return WIN_VALUE;
            }
            if (winningPlayer.Id == id) // ja pobedjujem 
            {
                //Debug.Log("WINNING pos" + winningPlayer.Position.x + winningPlayer.Position.y + "My pos" + move[0].x + move[0].y);
                //if (winningPlayer.Position == move[0]) // igra buider
                //{

                // sto dalje od protivnika
                float destValueBuildig = DistanceFromOpponent(move[1], 1 - id)
                    + ((int)board[move[1]].Height - 1) * 10;
                // sto dalje od protivnika i sto vise
                float buildValueBuilding = (board[move[2]].Height == Height.ROOF) ? -50
                    : (board[move[1]].Height - board[move[2]].Height == 1) ? 50
                    : (board[move[1]].Height - board[move[2]].Height == 0) ? 50 : 0;
                //Debug.Log("POBEDJUJEM - BUILDER: " + (destValueBuildig + buildValueBuilding));

                // winning graditelj ima prednost
                if (winningPlayer.Position == move[0])
                {
                    return destValueBuildig + buildValueBuilding + 50;
                }
                else
                {
                    return destValueBuildig + buildValueBuilding;
                }

                //}
                //else // igra smarac
                //{
                //     Debug.Log("POBEDJUJEM - IGRA DRUGI" + LOSS_VALUE);
                //      return LOSS_VALUE;
                //}
            }
            else // protivnik pobedjuje
            {
                // IPlayer otherPlayer = board[id, 0].Position == move[0] ? board[id, 1] : board[id, 0];
                //if (CanMove(otherPlayer.Position) && DistanceFromOpponent(move[0], 1 - id) > DistanceFromOpponent(otherPlayer.Position, 1 - id))
                //{
                //Debug.Log("GUBIM - IGRA DRUGI" + LOSS_VALUE);
                //igra drugi builder
                //return LOSS_VALUE;
                //}
                // else
                //{
                //sto blize protivniku
                float destValueHunt = 50 / Util.Distance(move[1], winningPlayer.Position);
                //sto blize protivniku i sto vise
                float buildValueHunt = 50 / Util.Distance(move[2], winningPlayer.Position)
                    + (board[move[2]].Height == Height.ROOF ? 50 : 0)
                    + (((Util.Distance(move[2], winningPlayer.Position) < 1.5f)
                    && (board[move[2]].Height - board[winningPlayer.Position].Height > 1)) ? 50 : 0);

                //Debug.Log("GUBIM - SMARAC: " + (destValueHunt + buildValueHunt));
                return destValueHunt + buildValueHunt;
                //  }
            }
        }

        private float DistanceFromOpponent(Vector2 position, PlayerID opponentId)
        {
            return Util.Distance(position, board[opponentId, 0].Position)
                        + Util.Distance(position, board[opponentId, 1].Position);
        }

        protected override void FindWinningPlayer()
        {
            float closestToWinValue = Mathf.NegativeInfinity;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    IPlayer player = board[(PlayerID)i, j];
                    if (CanMove(player.Position))
                    {
                        int localWinValue = WinValue(player);
                        if (localWinValue > closestToWinValue)
                        {
                            closestToWinValue = localWinValue;
                            winningPlayer.Id = player.Id;
                            winningPlayer.Position = player.Position;
                        }
                    }
                }
            }
        }

        private int WinValue(IPlayer player)
        {
            int x = (int)player.Position.x;
            int y = (int)player.Position.y;
            Height height = board[x, y].Height;
            int winValue = 0;
            for (int i = 0; i < Board.DIM; i++)
            {
                for (int j = 0; j < Board.DIM; j++)
                {
                    if (i == x && j == y)
                    {
                        continue;
                    }
                    else if (Mathf.Abs(x - i) <= 1 && Mathf.Abs(j - y) <= 1)
                    {
                        ITile tile = board[i, j];
                        if (!tile.HasPlayer() && tile.Height != Height.ROOF && (Mathf.Abs(height - tile.Height) == 1))
                        {
                            winValue++;
                            if (tile.Height == Height.H3)
                                winValue += 5;
                        }
                    }
                }
            }
            return winValue;
        }
    }
}
