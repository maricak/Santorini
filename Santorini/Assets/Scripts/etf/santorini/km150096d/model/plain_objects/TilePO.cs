using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.model.plain_objects
{
    public class TilePO : ITile
    {
        public Height Height { get; set; }
        public IPlayer Player { get; set; }
        public bool HasPlayer()
        {
            return Player != null;
        }
    }
}
