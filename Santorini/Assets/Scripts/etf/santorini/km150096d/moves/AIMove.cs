﻿using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.model.plain_objects;
using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public abstract class AIMove : Move
    {
        static protected int cnt = 0;

        protected readonly int maxDepth;
        protected Vector2[] move = new Vector2[3];
        private int moveCount = 0;

        protected MoveType type;
        protected float evaluationValue;

        public AIMove(PlayerID id, IBoard board) : base(id, board)
        {
            maxDepth = board.MaxDepth;
        }

        public sealed override bool MouseInputNeeded()
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

                    if (board is Board)
                    {
                        (board as Board).ResetSimulationLog();
                    }

                    var bestMove = new Vector2[3];
                    var bestValue = Algorithm(bestMove, 0, id, alpha, beta);
                    move = bestMove;

                    if (board is Board && board.Simulation)
                    {
                        (board as Board).AddToSimulationLog("BEST MOVE:\t" + bestValue +
                            "\n\tselect(" + move[0].x + "," + move[0].y + ")" +
                            "\n\tmove(" + move[1].x + "," + move[1].y + ")" +
                            "\n\tbuild(" + move[2].x + "," + move[2].y + ")");
                    }

                    Debug.Log(cnt);
                    cnt = 0;
                }
                base.MakeMove(move[moveCount]); // select
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
            (m as AIMove).moveState = MoveState.SELECT;
            return m as AIMove;
        }

        protected List<AIMove> GetAllPossibleMoves()
        {
            List<AIMove> moves = new List<AIMove>();

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
                            AIMove mMove = CopyBoardAndCreateMove(mSelect, type);
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
                                        AIMove mBuild = CopyBoardAndCreateMove(mMove, type);
                                        mBuild.Build(buildPosition);

                                        mBuild.move = new Vector2[] { srcPosition, dstPosition, buildPosition };
                                        mBuild.evaluationValue = mBuild.Evaluate(mBuild.move);

                                        // change turn
                                        mBuild.board.ChangeTurn();
                                        mBuild.id = 1 - mBuild.id;

                                       /* mBuild.srcPosition = srcPosition;
                                        mBuild.dstPosition = dstPosition;
                                        mBuild.buildPosition = buildPosition;*/
                                        moves.Add(mBuild);
                                    }
                                }
                            }
                            mMove = null;
                        }
                    }
                }
                mSelect = null;
            }
            return moves;
        }
    }
}
