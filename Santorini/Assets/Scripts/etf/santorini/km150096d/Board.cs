using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Collections;
using UnityEngine.UI;


namespace etf.santorini.km150096d
{
    public class Board : MonoBehaviour
    {
        private static Board Instance { get; set; }
        private Board() { }
        public static readonly int DIM = 5;

        // game object prefabs
        public GameObject tilePrefab;
        public GameObject[] playerPrefabs = new GameObject[2];
        public GameObject blockPrefab;
        public GameObject highlightPrefab;
        public GameObject roofPrefab;

        // message
        public Canvas messageCanvas;

        // position of the mouse
        private Vector2 mouseOver;

        private bool gameOver = false;
        private PlayerType winner;

        private float deltaTime = 0.0f;
        public float threshold = 0.5f;



        private void Start()
        {
            Instance = this;

            // initialize files
            FileManager.Instance.SetOutput();

            // TODO if needed! ------------------>  change later
            FileManager.Instance.SetInput();
            // initialize mouse position
            ResetMouseOver();
            // generate board tiles
            GenerateBoard();

            // TODO -> initialize player logic -- human, bot, level....
            Player.Init(Instance);

            // start positioning builders
            Player.PositioningInProgress = true;
            Player.ReadFromFileInProgres = true;

            UpdateMessage("Turn: " + Player.turn);
        }

        private void Update()
        {
            // update mouse postion
            UpdateMouseOver();

            if (!gameOver)
            {
                if (Player.MouseInputNeeded()) // wait for mouse click
                {
                    if (Input.GetMouseButtonDown(0) && MouseInsideBoard())
                    {
                        Player.Move(mouseOver);
                    }
                }
                else
                {
                    // read move from file or ai
                    deltaTime += Time.deltaTime;
                    if (deltaTime > threshold)
                    {
                        deltaTime = 0.0f;
                        try
                        {
                            if (Player.ReadFromFileInProgres)
                                Player.Move(Vector2.zero);
                        }
                        catch (IOException)
                        {
                            UpdateMessage("Input file is corrupted!");
                        }
                    }
                }
                CheckGameOver();
            }

            // TODO ako je kraj citanja fajla postaviti nove igrace, human , ai..
        }

        private void OnApplicationQuit()
        {
            FileManager.Instance.SaveFile();
        }


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
                var x = (hit.point.x / (Tile.DISTANCE + Tile.SIZE));
                var y = (hit.point.z / (Tile.DISTANCE + Tile.SIZE));
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


        private void CheckGameOver()
        {
            // current player has no possible moves
            if (!Player.HasPossibleMoves())
            {
                gameOver = true;
                winner = 1 - Player.turn;
                UpdateMessage("Winner is " + winner + "!");
            }
            // or he is a winner
            else if (Player.IsWinner())
            {
                gameOver = true;
                winner = Player.turn;
                UpdateMessage("Winner is " + winner + "!");
            }
        }

        public static void UpdateMessage(string message)
        {
            Instance.messageCanvas.GetComponentInChildren<Text>().text = message;
        }
    }
}