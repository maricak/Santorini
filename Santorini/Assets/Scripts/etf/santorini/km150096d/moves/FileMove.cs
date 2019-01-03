using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    class FileMove : Move
    {
        public FileMove(PlayerType type, Board board) : base(type, board) { }
        public override bool MouseInputNeeded()
        {
            return false;
        }
        public override void MakeMove(Vector2 position)
        {
            if (FileManager.Instance.HasNextPosition())
            {
                position = FileManager.Instance.NextPosition();
                base.MakeMove(position);
            }
            else
            {
                Player.FinishReadingFromFile();
            }
        }
    }
}
