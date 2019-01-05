using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.moves;
using UnityEngine;

namespace etf.santorini.km150096d.model.plain_objects
{
    public class PlayerPO : IPlayer
    {
        public PlayerID Id { get; set; }
        public Vector2 Position { get; set; }
        
    }
}
