using UnityEngine;

using etf.santorini.km150096d.utils;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Roof : MonoBehaviour
    {
        public static void GenerateRoof(int x, int y, Board board, int height)
        {
            GameObject gameObject = Instantiate(board.roofPrefab) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Roof roof = gameObject.GetComponent<Roof>();
            Util.MoveRoof(roof, x, y, height);
        }
    }
}
