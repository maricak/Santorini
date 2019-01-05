using System;
using UnityEngine;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.moves
{
    public abstract class Move
    {
        protected enum MoveState { POSITION_FIRST, POSITION_SECOND, SELECT, MOVE, BUILD };

        protected bool[,] possibleMoves = new bool[Board.DIM, Board.DIM];

        public readonly PlayerID id;
        public readonly Board board;

        protected MoveState moveState = MoveState.POSITION_FIRST;

        public Move(PlayerID id, Board board)
        {
            this.id = id;
            this.board = board;
        }
        internal void CopyMove(Move move)
        {
            moveState = move.moveState;
            possibleMoves = move.possibleMoves;
        }

        // TODO ukloniti virtual gde moze, internal, public, private, modifikatori???
        public abstract bool MouseInputNeeded();
        
        #region Move
        public virtual void MakeMove(Vector2 position)
        {
            switch (moveState)
            {
                case MoveState.POSITION_FIRST:
                    if (PositioningIsPossible(position))
                    {
                        IPlayer player = Player.GeneratePlayer(id, position, board);
                        board[position].Player = player;
                        board.players[(int)id, 0] = player;

                        FileManager.Instance.Src = position;

                        moveState = MoveState.POSITION_SECOND;
                    }
                    break;
                case MoveState.POSITION_SECOND:
                    if (PositioningIsPossible(position))
                    {
                        IPlayer player = Player.GeneratePlayer(id, position, board);
                        board[position].Player = player;
                        board.players[(int)id, 1] = player;

                        FileManager.Instance.Dst = position;

                        moveState = MoveState.SELECT;

                        // finish positioning
                        FileManager.Instance.WritePositions();                        
                        board.ChangeTurn();
                    }
                    break;
                case MoveState.SELECT:
                    if (SelectPlayer(position))
                    {
                        moveState = MoveState.MOVE;
                        FileManager.Instance.Src = position;
                    }
                    break;
                case MoveState.MOVE:
                    if (MovePlayer(position)) // move selected
                    {
                        moveState = MoveState.BUILD;
                        FileManager.Instance.Dst = position;
                    }
                    break;
                case MoveState.BUILD:
                    if (Build(position))
                    {
                        board.ResetHighlight();
                        FileManager.Instance.Build = position;

                        // finish moving and building
                        FileManager.Instance.WriteMove();
                        moveState = MoveState.SELECT;
                        board.ChangeTurn();
                    }
                    break;
                default: break;
            }

        }
        protected bool SelectPlayer(Vector2 position)
        {
            int x = (int)position.x;
            int y = (int)position.y;
            ITile selectedTile = board[x, y];
            if (selectedTile.HasPlayer() && selectedTile.Player.Id == id)
            {
                board.selectedPlayer = selectedTile.Player;

                // highlight
                CalculatePossibleMoves(board.selectedPlayer.Position);
                board.SetHighlight(possibleMoves);

                return true;
            }
            return false;
        }
        protected bool MovePlayer(Vector2 dstPosition)
        {
            int x = (int)dstPosition.x;
            int y = (int)dstPosition.y;

            IPlayer player = board.selectedPlayer;
            ITile tileSrc = board[board.selectedPlayer.Position];
            ITile tileDst = board[x, y];

            if (possibleMoves[x, y])
            {
                // remove highlight
                board.ResetHighlight();

                // remove player from old tile
                tileSrc.Player = null;

                // move player to new tile
                tileDst.Player = player;
                board.selectedPlayer.Position = dstPosition;
                Util.MovePlayer(player as Player, x, y, (int)(tileDst.Height - 1));

                if (!CanBuild(board.selectedPlayer.Position))
                {
                    return false;
                }

                // set new highlight
                CalculatePossibleBuilds(dstPosition);
                board.SetHighlight(possibleMoves);
                return true;
            }
            else if (tileDst.HasPlayer() && tileDst.Player.Id == id)
            {
                // remove old highlight
                board.ResetHighlight();

                // player wants to select builder again
                board.selectedPlayer = tileDst.Player;

                // set new highlight
                CalculatePossibleMoves(board.selectedPlayer.Position);
                board.SetHighlight(possibleMoves);

                // move is not over
                return false;
            }
            else
            {
                // move is not possible, try again
                return false;
            }
        }
        protected bool Build(Vector2 position)
        {
            int x = (int)position.x;
            int y = (int)position.y;

            ITile tile = board[x, y];

            if (possibleMoves[x, y])
            {
                // generate block or roof
                tile.Height++;
                if (tile.Height == Height.ROOF)
                {
                    Roof.GenerateRoof(x, y, board, (int)tile.Height);
                }
                else
                {
                    Block.GenerateBlock(x, y, board, (int)tile.Height - 1);
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Calculations
        protected void CalculatePossibleMoves(Vector2 playerPosition)
        {
            int x = (int)playerPosition.x;
            int y = (int)playerPosition.y;

            //Tile playerTile = Tile.GetTile(x, y);
            ITile playerTile = board[x, y];

            for (int i = 0; i < Board.DIM; i++)
            {
                for (int j = 0; j < Board.DIM; j++)
                {
                    if (i == x && j == y)
                    {
                        possibleMoves[i, j] = false;
                    }
                    else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                    {
                        //Tile tile = Tile.GetTile(i, j);
                        ITile tile = board[i, j];
                        if (tile.HasPlayer() || tile.Height == Height.ROOF || (playerTile.Height + 1).CompareTo(tile.Height) < 0)
                        {
                            possibleMoves[i, j] = false;
                        }
                        else
                        {
                            possibleMoves[i, j] = true;
                        }
                    }
                    else
                    {
                        possibleMoves[i, j] = false;
                    }
                }
            }
        }
        protected void CalculatePossibleBuilds(Vector2 playerPosition)
        {
            int x = (int)playerPosition.x;
            int y = (int)playerPosition.y;

            for (int i = 0; i < Board.DIM; i++)
            {
                for (int j = 0; j < Board.DIM; j++)
                {
                    if (i == x && j == y)
                    {
                        possibleMoves[i, j] = false;
                    }
                    else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                    {
                        // Tile tile = Tile.GetTile(i, j);
                        ITile tile = board[i, j];
                        if (!tile.HasPlayer() && (tile.Height != Height.ROOF))
                            possibleMoves[i, j] = true;
                    }
                    else
                    {
                        possibleMoves[i, j] = false;
                    }
                }
            }
        }
        #endregion

        public bool IsWinner()
        {
            if(moveState == MoveState.POSITION_FIRST || moveState == MoveState.POSITION_SECOND)
            {
                return false;
            }
            Vector2[] positions = board.GetPlayerPositions(id);
            ITile tile = board[positions[0]];
            if(tile.Height == Height.H3)
            {
                return true;
            }
            tile = board[positions[1]];
            if (tile.Height == Height.H3)
            {
                return true;
            }
            return false;
        }

        #region HasMoves
        protected bool PositioningIsPossible(Vector2 position)
        {
            ITile tile = board[position];

            // the tile is not occupied
            if (!tile.HasPlayer())
            {
                return true;
            }
            return false;
        }
        public bool HasPossibleMoves()
        {
            if (moveState == MoveState.SELECT)
            {
                // is there a builder that can be selected and than moved 
                return CanMove(board.GetPlayerPositions(id));
            }
            else if (moveState == MoveState.BUILD)
            {
                // is there available tile to build on
                return CanBuild(board.selectedPlayer.Position);
            }
            return true;
        }
        public bool CanMove(Vector2[] playerPositions)
        {
            for (int k = 0; k < 2; k++)
            {
               // Player player = Player.GetPlayer(id, k);
                int x = (int)playerPositions[k].x;
                int y = (int)playerPositions[k].y;
                //Tile playerTile = Tile.GetTile(x, y);
                ITile playerTile = board[x, y];
                for (int i = 0; i < Board.DIM; i++)
                {
                    for (int j = 0; j < Board.DIM; j++)
                    {
                        if (i == x && j == y)
                        {
                            continue;
                        }
                        else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                        {
                            // Tile tile = Tile.GetTile(i, j);
                            ITile tile = board[i, j];
                            if (tile.HasPlayer() || tile.Height == Height.ROOF || (playerTile.Height + 1).CompareTo(tile.Height) < 0)
                            {
                                continue;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        public bool CanBuild(Vector2 builderPosition)
        {
            int x = (int)builderPosition.x;
            int y = (int)builderPosition.y;

            for (int i = 0; i < Board.DIM; i++)
            {
                for (int j = 0; j < Board.DIM; j++)
                {
                    if (i == x && j == y)
                    {
                        continue;
                    }
                    else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                    {
                        ITile tile = board[i, j];
                        if (!tile.HasPlayer() && (tile.Height != Height.ROOF))
                            return true;
                    }
                }
            }
            return false;
        } 
        #endregion
    }
}