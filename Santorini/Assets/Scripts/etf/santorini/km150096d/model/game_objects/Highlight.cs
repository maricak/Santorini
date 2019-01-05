using UnityEngine;

using etf.santorini.km150096d.utils;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Highlight : MonoBehaviour
    {
        public static Highlight GenerateHighlight(int x, int y, Board board)
        {
            GameObject gameObject = Instantiate(board.highlightPrefab) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Highlight highlight = gameObject.GetComponent<Highlight>();
            Util.MoveHighlight(highlight, x, y, height: 0); // na pocetnu je visina 0
            highlight.gameObject.SetActive(false);

            return highlight;
        }       
    }
}