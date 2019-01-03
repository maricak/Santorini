using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace etf.santorini.km150096d.moves
{

    public abstract class Move
    {
        protected enum MoveState { POSITION_FIRST, POSITION_SECOND, FINISH_POSITIONING, SELECT, MOVE, BUILD, FINISH };

        protected readonly bool[,] possibleMoves = new bool[Board.DIM, Board.DIM];

        public PlayerType Type { get; set; }
        public Board Board { get; set; }

        protected MoveState moveState = MoveState.POSITION_FIRST;


        // TODO ukloniti virtual gde moze
        public abstract bool MouseInputNeeded();
        public virtual void Position(Vector2 position)
        {
            if (PositioningIsPossible(position))
            {
                // generate player
                if (moveState == MoveState.POSITION_FIRST)
                {
                    Player.GeneratePlayer(Type, position, Board, 0);
                    moveState = MoveState.POSITION_SECOND;
                }
                else if (moveState == MoveState.POSITION_SECOND)
                {
                    Player.GeneratePlayer(Type, position, Board, 1);
                    moveState = MoveState.FINISH_POSITIONING;
                }

                // check if player positionin is over
                if (moveState == MoveState.FINISH_POSITIONING)
                {
                    FileManager.Instance.WritePositions(Type);
                    moveState = MoveState.SELECT;
                    // finish positioning 
                    if (Type == PlayerType.PLAYER1)
                    {
                        //Player.FinishPositioningPlayers();
                        Player.PositioningInProgress = false;
                    }
                    // change turn
                    Player.ChangeTurn();
                }
            }
        }
        protected virtual bool PositioningIsPossible(Vector2 position)
        {
            Tile t = Tile.GetTile((int)position.x, (int)position.y);
            // the tile is not occupied
            if (!t.HasPlayer())
            {
                return true;
            }
            return false;
        }
        public virtual void MakeMove(Vector2 position)
        {
            switch (moveState)
            {
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
                        moveState = MoveState.FINISH;
                        Highlight.ResetHighlight();
                        FileManager.Instance.Build = position;
                    }
                    break;
                default: break;
            }
            if (moveState == MoveState.FINISH)
            {
                FileManager.Instance.WriteMove();
                moveState = MoveState.SELECT;
                Player.ChangeTurn();
            }
        }
        protected virtual bool SelectPlayer(Vector2 position)
        {
            int x = (int)position.x;
            int y = (int)position.y;
            Tile selectedTile = Tile.GetTile(x, y);
            if (selectedTile.HasPlayer() && selectedTile.Player.Type == Type)
            {
                Player.selectedPlayer = selectedTile.Player;

                // highlight
                CalculatePossibleMoves(Player.selectedPlayer.Position);
                Highlight.SetHighlight(possibleMoves);

                return true;
            }
            return false;
        }
        protected virtual bool MovePlayer(Vector2 dstPosition)
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

                if (!CanBuild())
                {
                    return false;
                }

                // set new highlight
                CalculatePossibleBuilds(dstPosition);
                Highlight.SetHighlight(possibleMoves);
                return true;
            }
            else if (tileDst.HasPlayer() && tileDst.Player.Type == Type)
            {
                // remove old highlight
                Highlight.ResetHighlight();

                // player wants to select builder again
                Player.selectedPlayer = tileDst.Player;

                // set new highlight
                CalculatePossibleMoves(Player.selectedPlayer.Position);
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
        protected virtual bool Build(Vector2 position)
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
                    Roof.GenerateRoof(x, y, Board);
                }
                else
                {
                    Block.GenerateBlock(x, y, Board);
                }
                return true;
            }
            return false;
        }
        public virtual bool HasPossibleMoves()
        {
            if (moveState == MoveState.SELECT)
            {
                // is there a builder that can be selected and than moved 
                return CanMove();
            }
            else if (moveState == MoveState.BUILD)
            {
                // is there available tile to build on
                return CanBuild();
            }
            return true;
        }
        protected virtual void CalculatePossibleMoves(Vector2 playerPosition)
        {
            int x = (int)playerPosition.x;
            int y = (int)playerPosition.y;

            Tile playerTile = Tile.GetTile(x, y);

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
                        Tile tile = Tile.GetTile(i, j);
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
        protected virtual void CalculatePossibleBuilds(Vector2 playerPosition)
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
                        Tile tile = Tile.GetTile(i, j);
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
        protected virtual bool CanMove()
        {
            for (int k = 0; k < 2; k++)
            {
                Player player = Player.GetPlayer(Type, k);
                int x = (int)player.Position.x;
                int y = (int)player.Position.y;
                Tile playerTile = Tile.GetTile(x, y);

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
                            Tile tile = Tile.GetTile(i, j);
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
        protected virtual bool CanBuild()
        {
            int x = (int)Player.selectedPlayer.Position.x;
            int y = (int)Player.selectedPlayer.Position.y;
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
                        Tile tile = Tile.GetTile(i, j);
                        if (!tile.HasPlayer() && (tile.Height != Height.ROOF))
                            return true;
                    }
                }
            }
            return false;
        }
    }
}