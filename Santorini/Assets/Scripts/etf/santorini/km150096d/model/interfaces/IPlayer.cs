using etf.santorini.km150096d.moves;
using UnityEngine;

namespace etf.santorini.km150096d.model.interfaces
{
    public enum PlayerID : int { PLAYER0 = 0, PLAYER1 };
 
    public interface IPlayer
    {
        PlayerID Id { get; set; }
        Vector2 Position { get; set; }    
    }
}
