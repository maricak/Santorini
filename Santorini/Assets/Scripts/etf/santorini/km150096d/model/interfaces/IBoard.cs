

using UnityEngine;

namespace etf.santorini.km150096d.model.interfaces
{
    public interface IBoard
    {
        ITile this[int x, int y] { get; }
        ITile this[Vector2 position] { get; }
        IPlayer this[PlayerID id, int num] { get; set; }
        IPlayer SelectedPlayer { get; set; }
        PlayerID TurnId { get; set; }

        bool Simulation { set; get; }
        int MaxDepth { set; get; }

        void ChangeTurn();
    }
}
