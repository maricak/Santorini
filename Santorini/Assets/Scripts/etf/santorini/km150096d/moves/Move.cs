using System;
using UnityEngine;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.moves
{
    public enum MoveType : int { HUMAN, EASY, MEDIUM, HARD };
    public abstract class Move
    {
        protected enum MoveState { POSITION_FIRST, POSITION_SECOND, SELECT, MOVE, BUILD };

        protected bool[,] possibleMoves = new bool[Board.DIM, Board.DIM];
      
        public PlayerID id;
        public readonly IBoard board;

        protected MoveState moveState = MoveState.POSITION_FIRST;

        public Move(PlayerID id, IBoard board)
        {
            this.id = id;
            this.board = board;
        }
        public void CopyMove(Move move)
        {
            moveState = move.moveState;
            possibleMoves = move.possibleMoves;
        }
        public void CopyPossibleMoves(Move move)
        {
            for(int i = 0; i < Board.DIM; i++)
            {
                for(int j = 0; j < Board.DIM; j++)
                {
                    possibleMoves[i, j] = move.possibleMoves[i, j];
                }
            }
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
                        IPlayer player = Player.GeneratePlayer(id, position, board as Board);
                        board[position].Player = player;
                        board[id, 0] = player;


                        if (board is Board)
                        {
                            FileManager.Instance.Src = position;
                        }
                        moveState = MoveState.POSITION_SECOND;
                    }
                    break;
                case MoveState.POSITION_SECOND:
                    if (PositioningIsPossible(position))
                    {
                        IPlayer player = Player.GeneratePlayer(id, position, board as Board);
                        board[position].Player = player;
                        board[id, 1] = player;

                        if (board is Board)
                        {
                            FileManager.Instance.Dst = position;
                            FileManager.Instance.WritePositions();
                        }
                        // finish positioning
                        moveState = MoveState.SELECT;

                        board.ChangeTurn();
                    }
                    break;
                case MoveState.SELECT:
                    if (SelectPlayer(position))
                    {
                        moveState = MoveState.MOVE;
                        if (board is Board)
                        {
                            FileManager.Instance.Src = position;
                        }
                    }
                    break;
                case MoveState.MOVE:
                    if (MovePlayer(position)) // move selected
                    {
                        moveState = MoveState.BUILD;
                        if (board is Board)
                        {
                            FileManager.Instance.Dst = position;
                        }
                    }
                    break;
                case MoveState.BUILD:
                    if (Build(position))
                    {
                        if (board is Board)
                        {
                            (board as Board).ResetHighlight();
                            FileManager.Instance.Build = position;

                            FileManager.Instance.WriteMove();
                        }
                        // finish moving and building
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
                board.SelectedPlayer = selectedTile.Player;

                // highlight
                CalculatePossibleMoves(board.SelectedPlayer.Position);
                if (board is Board)
                {
                    (board as Board).SetHighlight(possibleMoves);
                }
                return true;
            }
            return false;
        }
        protected bool MovePlayer(Vector2 dstPosition)
        {
            int x = (int)dstPosition.x;
            int y = (int)dstPosition.y;

            IPlayer player = board.SelectedPlayer;
            ITile tileSrc = board[board.SelectedPlayer.Position];
            ITile tileDst = board[x, y];

            if (possibleMoves[x, y])
            {
                if (board is Board)
                {
                    // remove highlight
                    (board as Board).ResetHighlight();
                }
                // remove player from old tile
                tileSrc.Player = null;

                // move player to new tile
                tileDst.Player = player;
                board.SelectedPlayer.Position = dstPosition;
                if (board is Board)
                {
                    Util.MovePlayer(player as Player, x, y, (int)(tileDst.Height - 1));
                }
                //if (!CanBuild(board.SelectedPlayer.Position))
                //{
                //    return false;
                //}

                // set new highlight
                CalculatePossibleBuilds(dstPosition);
                if (board is Board)
                {
                    (board as Board).SetHighlight(possibleMoves);
                }
                return true;
            }
            else if (tileDst.HasPlayer() && tileDst.Player.Id == id)
            {
                if (board is Board)
                {
                    // remove old highlight
                    (board as Board).ResetHighlight();
                }
                // player wants to select builder again
                board.SelectedPlayer = tileDst.Player;

                // set new highlight
                CalculatePossibleMoves(board.SelectedPlayer.Position);
                if (board is Board)
                {
                    (board as Board).SetHighlight(possibleMoves);
                }
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
                if (board is Board)
                {
                    if (tile.Height == Height.ROOF)
                    {
                        Roof.GenerateRoof(x, y, board as Board, (int)tile.Height - 1);
                    }
                    else
                    {
                        Block.GenerateBlock(x, y, board as Board, (int)tile.Height - 1);
                    }
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
            Debug.Log("Is winner" + moveState);
            if (moveState == MoveState.POSITION_FIRST || moveState == MoveState.POSITION_SECOND)
            {
                return false;
            }
            Vector2[] positions = new Vector2[] { board[id, 0].Position, board[id, 1].Position};
            ITile tile = board[positions[0]];
            if (tile.Height == Height.H3)
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

               // return CanMove(new Vector2[] { board[id, 0].Position, board[id, 1].Position });
                return CanMove(board[id, 0].Position) || CanMove(board[id, 1].Position);
            }
            else if (moveState == MoveState.BUILD)
            {
                // is there available tile to build on
                return CanBuild(board.SelectedPlayer.Position);
            }
            return true;
        }
        public bool CanMove(Vector2/*[]*/ playerPosition/*s*/)
        {
            //for (int k = 0; k < 2; k++)
            //{
                int x = (int)playerPosition/*s[k]*/.x;
                int y = (int)playerPosition/*s[k]*/.y;
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
               // }
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

        public static Move CreateMove(MoveType type, PlayerID id, IBoard board)
        {
            switch (type)
            {
                case MoveType.HUMAN:
                    return new HumanMove(id, board);
                case MoveType.EASY:
                    return new EasyMove(id, board);
                case MoveType.MEDIUM:
                    return new MediumMove(id, board);
                case MoveType.HARD:
                    return new HardMove(id, board);
            }         

            return new HumanMove(id, board);
        }
    }
}