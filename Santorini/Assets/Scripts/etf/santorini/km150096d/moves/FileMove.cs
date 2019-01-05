using UnityEngine;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.model.gameobject;

namespace etf.santorini.km150096d.moves
{
    class FileMove : Move
    {
        public FileMove(PlayerID id, IBoard board) : base(id,board) { }
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
                (board as Board).FinishReadingFromFile();
            }
        }
    }
}
