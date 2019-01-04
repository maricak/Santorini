using etf.santorini.km150096d.model;

namespace etf.santorini.km150096d.moves
{
    public class HumanMove : Move
    {
        public HumanMove(PlayerID id, Board board) : base(id, board) { }
        
        public override bool MouseInputNeeded()
        {
            return true;
        }
    }
}