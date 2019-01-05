using etf.santorini.km150096d.model.interfaces;
using UnityEngine;

namespace etf.santorini.km150096d.model.plain_objects
{
    public class BoardPO : IBoard
    {
        public static readonly int DIM = 5;

        #region Tiles
        private readonly ITile[,] tiles = new TilePO[DIM, DIM];
        public ITile this[int x, int y]
        {
            get { return tiles[x, y]; }
        }
        public ITile this[Vector2 position]
        {
            get { return tiles[(int)position.x, (int)position.y]; }
        }
        #endregion

        #region Players
        public readonly IPlayer[,] players = new PlayerPO[2, 2];
        public IPlayer this[PlayerID id, int num]
        {
            get { return players[(int)id, num]; }
            set { players[(int)id, num] = value; }
        }
        public IPlayer SelectedPlayer { get; set; }
        public PlayerID TurnId { get; set; }
        #endregion     

        public bool Simulation { set; get; }
        public int MaxDepth { set; get; }

        #region Constructor
        public BoardPO(IBoard board)
        {
            CopyTiles(board);
            CopyPlayers(board);
            TurnId = board.TurnId;
            MaxDepth = board.MaxDepth;
            Simulation = board.Simulation;           
        }
        private void CopyTiles(IBoard board)
        {
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    tiles[i, j] = new TilePO()
                    {
                        Height = board[i, j].Height,
                        Player = null
                    };
                }
            }
        }

        private void CopyPlayers(IBoard board)
        {
            for (int i = 0; i < 2; i++)
            {
                //var positions = new Vector2[] { board[(PlayerID)i, 0].Position, board[(PlayerID)i, 1].Position };3
                for (int j = 0; j < 2; j++)
                {
                    IPlayer originalPlayer = board[(PlayerID)i, j];
                    var copyPlayer = new PlayerPO()
                    {
                        Id = (PlayerID)i,
                        Position = originalPlayer.Position
                    };
                    this[copyPlayer.Position].Player = copyPlayer;
                    players[i, j] = copyPlayer;
                    if(originalPlayer == board.SelectedPlayer)
                    {
                        SelectedPlayer = copyPlayer;
                    }
                }
            }
        }
        #endregion

        #region Game methods
        public void ChangeTurn()
        {
            TurnId = 1 - TurnId;
        }
        #endregion     
    }
}
