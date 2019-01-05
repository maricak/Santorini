using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.moves
{
    public class HardMove : Move
    {
        public HardMove(PlayerID id, IBoard board) : base(id, board) { }

        public override bool MouseInputNeeded()
        {
            return false;
        }
    }
}
