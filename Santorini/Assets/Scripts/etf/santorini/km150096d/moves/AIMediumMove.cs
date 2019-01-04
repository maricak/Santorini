﻿using etf.santorini.km150096d.model.gameobject;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.moves
{
    public class AIMediumMove : Move
    {
        public AIMediumMove(PlayerID id, Board board, int maxDepth) : base(id, board)
        {
            //  this.maxDepth = maxDepth;
        }

        public override bool MouseInputNeeded()
        {
            return false;
        }
    }
}
