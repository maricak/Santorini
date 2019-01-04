using System;
using UnityEngine;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model;

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
                    if (PositioningIsPossible(position, Tile.GetTiles()))
                    {
                        Player.GeneratePlayer(id, position, board, 0);
                        FileManager.Instance.Src = position;
                        moveState = MoveState.POSITION_SECOND;
                    }
                    break;
                case MoveState.POSITION_SECOND:
                    if (PositioningIsPossible(position, Tile.GetTiles()))
                    {
                        Player.GeneratePlayer(id, position, board, 1);
                        FileManager.Instance.Dst = position;

                        // finish positioning
                        FileManager.Instance.WritePositions();
                        moveState = MoveState.SELECT;
                        Player.ChangeTurn();
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
                        Highlight.ResetHighlight();
                        FileManager.Instance.Build = position;

                        // finish moving and building
                        FileManager.Instance.WriteMove();
                        moveState = MoveState.SELECT;
                        Player.ChangeTurn();
                    }
                    break;
                default: break;
            }

        }
        protected bool SelectPlayer(Vector2 position)
        {
            int x = (int)position.x;
            int y = (int)position.y;
            Tile selectedTile = Tile.GetTile(x, y);
            if (selectedTile.HasPlayer() && selectedTile.Player.Id == id)
            {
                Player.selectedPlayer = selectedTile.Player;

                // highlight
                CalculatePossibleMoves(Player.selectedPlayer.Position, ref possibleMoves, Tile.GetTiles());
                Highlight.SetHighlight(possibleMoves);

                return true;
            }
            return false;
        }
        protected bool MovePlayer(Vector2 dstPosition)
        {
            int x = (int)dstPosition.x;
            int y = (int)dstPosition.y;

            Player player = Player.selectedPlayer;
            Tile tileSrc = Tile.GetTile((int)Player.selectedPlayer.Position.x, (int)Player.selectedPlayer.Position.y);
            Tile tileDst = Tile.GetTile(x, y);

            if (possibleMoves[x, y])
            {
                // remove highlight
                Highlight.ResetHighlight();

                // remove player from old tile
                tileSrc.Player = null;

                // move player to new tile
                tileDst.Player = player;
                Player.selectedPlayer.Position = dstPosition;
                Util.MovePlayer(player, x, y, (int)(tileDst.Height - 1));

                if (!CanBuild(Player.selectedPlayer.Position, Tile.GetTiles()))
                {
                    return false;
                }

                // set new highlight
                CalculatePossibleBuilds(dstPosition, ref possibleMoves, Tile.GetTiles());
                Highlight.SetHighlight(possibleMoves);
                return true;
            }
            else if (tileDst.HasPlayer() && tileDst.Player.Id == id)
            {
                // remove old highlight
                Highlight.ResetHighlight();

                // player wants to select builder again
                Player.selectedPlayer = tileDst.Player;

                // set new highlight
                CalculatePossibleMoves(Player.selectedPlayer.Position, ref possibleMoves, Tile.GetTiles());
                Highlight.SetHighlight(possibleMoves);

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

            Tile tile = Tile.GetTile(x, y);

            if (possibleMoves[x, y])
            {
                // generate block or roof
                tile.Height++;
                if (tile.Height == Height.ROOF)
                {
                    Roof.GenerateRoof(x, y, board);
                }
                else
                {
                    Block.GenerateBlock(x, y, board);
                }
                return true;
            }
            return false;
        }
        #endregion

        #region Calculations
        protected static void CalculatePossibleMoves(Vector2 playerPosition, ref bool[,] possibleMoves, Tile[,] tiles)
        {
            int x = (int)playerPosition.x;
            int y = (int)playerPosition.y;

            //Tile playerTile = Tile.GetTile(x, y);
            Tile playerTile = tiles[x, y];

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
                        Tile tile = tiles[i, j];
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
        protected static void CalculatePossibleBuilds(Vector2 playerPosition, ref bool[,] possibleMoves, Tile[,] tiles)
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
                        Tile tile = tiles[i, j];
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
            Vector2[] positions = Player.GetPlayerPositions(id);
            Tile tile = Tile.GetTile((int)positions[0].x, (int)positions[0].y);
            if(tile.Height == Height.H3)
            {
                return true;
            }
            tile = Tile.GetTile((int)positions[1].x, (int)positions[1].y);
            if (tile.Height == Height.H3)
            {
                return true;
            }
            return false;
        }

        #region HasMoves
        protected static bool PositioningIsPossible(Vector2 position, Tile[,] tiles)
        {
            //Tile tile = Tile.GetTile((int)position.x, (int)position.y);
            Tile tile = tiles[(int)position.x, (int)position.y];
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
                return CanMove(Player.GetPlayerPositions(id), Tile.GetTiles());
            }
            else if (moveState == MoveState.BUILD)
            {
                // is there available tile to build on
                return CanBuild(Player.selectedPlayer.Position, Tile.GetTiles());
            }
            return true;
        }
        public static bool CanMove(/*PlayerID id, */Vector2[] playerPositions, Tile[,] tiles)
        {
            for (int k = 0; k < 2; k++)
            {
               // Player player = Player.GetPlayer(id, k);
                int x = (int)playerPositions[k].x;
                int y = (int)playerPositions[k].y;
                //Tile playerTile = Tile.GetTile(x, y);
                Tile playerTile = tiles[x, y];
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
                            Tile tile = tiles[i, j];
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
        public static bool CanBuild(Vector2 builderPosition, Tile[,] tiles)
        {
            //int x = (int)Player.selectedPlayer.Position.x;
            //int y = (int)Player.selectedPlayer.Position.y;
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
                        //Tile tile = Tile.GetTile(i, j);
                        Tile tile = tiles[i, j];
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