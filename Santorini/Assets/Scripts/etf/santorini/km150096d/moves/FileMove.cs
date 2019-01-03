using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    class FileMove : Move
    {
        public override bool MouseInputNeeded()
        {
            return false;
        }
        public override void Position(Vector2 position)
        {
            if (FileManager.Instance.HasNextPosition())
            {
                position = FileManager.Instance.NextPosition();
                base.Position(position);
            }
            else
            {
                Player.ReadFromFileInProgres = false;
            }
        }

        // protected virtual bool PositioningIsPossible(Vector2 position)
        // public override void MakeMove(Vector2 position)

        protected override bool SelectPlayer(Vector2 position)
        {
            if (FileManager.Instance.HasNextPosition())
            {
                position = FileManager.Instance.NextPosition();
                return base.SelectPlayer(position);
            }
            else
            {
                Player.ReadFromFileInProgres = false;
                return false;
            }
        }
        protected override bool MovePlayer(Vector2 dstPosition)
        {
            if (FileManager.Instance.HasNextPosition())
            {
                dstPosition = FileManager.Instance.NextPosition();
                return base.MovePlayer(dstPosition);
            }
            else
            {
                Player.ReadFromFileInProgres = false;
                return false;
            }
        }
        protected override bool Build(Vector2 position)
        {
            if (FileManager.Instance.HasNextPosition())
            {
                position = FileManager.Instance.NextPosition();
                return base.Build(position);
            }
            else
            {
                Player.ReadFromFileInProgres = false;
                return false;
            }
        }

        //public override bool HasPossibleMoves()  
        //protected override void CalculatePossibleBuilds(Vector2 playerPosition)
        //protected override void CalculatePossibleMoves(Vector2 /
        //protected override bool CanBuild()
        //protected override bool CanMove()
    }
}
