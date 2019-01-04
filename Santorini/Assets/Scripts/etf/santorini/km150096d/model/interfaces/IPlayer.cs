using UnityEngine;

namespace etf.santorini.km150096d.model.interfaces
{
    public enum PlayerID : int { PLAYER0 = 0, PLAYER1 };
    public enum PlayerType : int { HUMAN, EASY, MEDIUM, HARD };
    public interface IPlayer
    {
        PlayerID Id { get; set; }
        Vector2 Position { get; set; }
    }
}
