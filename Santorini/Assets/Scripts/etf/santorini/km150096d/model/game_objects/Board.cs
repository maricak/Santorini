using System.IO;
using UnityEngine;
using UnityEngine.UI;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.menu;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Board : MonoBehaviour
    {
        #region Singleton
        public static Board Instance { get; set; }
        private Board() { }
        #endregion

        public static readonly int DIM = 5;

        // game object prefabs
        #region Unity objects
        public GameObject tilePrefab;
        public GameObject[] playerPrefabs = new GameObject[2];
        public GameObject blockPrefab;
        public GameObject highlightPrefab;
        public GameObject roofPrefab;       

        // message
        public Canvas messageCanvas;
        #endregion


        // position of the mouse
        private Vector2 mouseOver;

        #region Game variables
        private bool gameOver = false;
        private PlayerID winner;

        private float deltaTime = 0.0f;
        public float threshold = 0.5f;

        public bool Simulation { set; get; }
        public int MaxDepth { set; get; }
        #endregion

        #region Unity methods
        private void Start()
        {
            Instance = this;

            // initialize files
            FileManager.Instance.SetOutput();

            // initialize mouse position
            ResetMouseOver();
            // generate board tiles
            GenerateBoard();

            Player.Init(Instance,
              (PlayerType)Menu.Instance.player1.value,
              (PlayerType)Menu.Instance.player2.value,
              Menu.Instance.simulation.isOn,
              Menu.Instance.depth.value + 1,
              Menu.Instance.loadFromFile);

            UpdateMessage("Turn: " + Player.turnId);
        }
        private void Update()
        {
            deltaTime += Time.deltaTime;

            // update mouse postion
            UpdateMouseOver();

            if (!gameOver)
            {
                if (Player.MouseInputNeeded()) // wait for mouse click
                {
                    if (Input.GetMouseButtonDown(0) && MouseInsideBoard())
                    {
                        Player.MakeMove(mouseOver);
                    }
                }
                else if (deltaTime > threshold) // read move from file or ai after treshold time
                {
                    deltaTime = 0.0f;
                    try
                    {
                        // position is not important                    
                        Player.MakeMove(Vector2.zero);
                    }
                    catch (IOException)
                    {
                        UpdateMessage("Input file is corrupted!");
                        Player.FinishReadingFromFile();
                    }
                }
                CheckGameOver();
            }
        }
        private void OnApplicationQuit()
        {
            FileManager.Instance.SaveFile();
        }
        #endregion

        #region MouseOver
        // update the position of the mouse
        private void UpdateMouseOver()
        {
            if (!Camera.main)
            {
                Debug.Log("Unable to find main camera");
                return;
            }
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
            {
                // calculate tile index
                var x = hit.point.x / (Tile.DISTANCE + Tile.SIZE);
                var y = hit.point.z / (Tile.DISTANCE + Tile.SIZE);
                if (x % 1 <= 0.9 && y % 1 <= 0.9)
                {
                    mouseOver.x = (int)x;
                    mouseOver.y = (int)y;
                }
                else
                {
                    ResetMouseOver();
                }
            }
            else
            {
                ResetMouseOver();
            }
        }
        private void ResetMouseOver()
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
        // check if mouse position is inside the board
        private bool MouseInsideBoard()
        {
            return mouseOver.x >= 0 && mouseOver.x < DIM && mouseOver.y >= 0 && mouseOver.y < DIM;
        }
        #endregion

        #region GenerateBoard
        private void GenerateBoard()
        {
            for (int x = 0; x < DIM; x++)
            {
                for (int y = 0; y < DIM; y++)
                {
                    Tile.GenerateTile(x, y, Board.Instance);
                    Highlight.GenerateHighlight(x, y, Board.Instance);
                }
            }
        }
        #endregion

        #region Game methods

        private void CheckGameOver()
        {
            // current player has no possible moves
            if(Player.IsGameOver(ref winner))
            {
                gameOver = true;
                UpdateMessage("Winner is " + winner + "!");
            }           
        }
        #endregion

        #region Message
        public static void UpdateMessage(string message)
        {
            Instance.messageCanvas.GetComponentInChildren<Text>().text = message;
        }
        #endregion

        #region Player
        #endregion

        #region Tile
        #endregion
    }
}