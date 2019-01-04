using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.moves
{
    public class AIHardMove : Move
    {
        public AIHardMove(PlayerID id, Board board, int maxDepth) : base(id, board)
        {
            //  this.maxDepth = maxDepth;
        }

        public override bool MouseInputNeeded()
        {
            return false;
        }
    }
}
