using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    public class HumanMove : Move
    {
        public HumanMove(PlayerType type, Board board) : base(type, board) { }
        
        public override bool MouseInputNeeded()
        {
            return true;
        }
    }
}