using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model;


namespace etf.santorini.km150096d.menu
{

    public class Menu : MonoBehaviour
    {

        #region Singlton

        public static Menu Instance { set; get; }
        private Menu() { }

        #endregion

        public Dropdown player1;
        public Dropdown player2;
        public Toggle simulation;

        public Button loadFile;
        public InputField fileName;
        public bool loadFromFile;

        public Dropdown depth;

        public Button play;

        private void Start()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Instance.Init();
        }

        private void Init()
        {
            player1.onValueChanged.AddListener(ChangePlayer);
            player2.onValueChanged.AddListener(ChangePlayer);

            simulation.interactable = false;
            simulation.isOn = false;

            loadFile.onClick.AddListener(LoadFile);

            play.onClick.AddListener(StartGame);
        }

        private void ChangePlayer(int value)
        {
            if (player1.value != 0 || player2.value != 0)
            {
                simulation.interactable = true;
            }
            else
            {
                simulation.interactable = false;
            }
        }
        private void LoadFile()
        {
            if (FileManager.Instance.SetInput(fileName.text))
            {
                loadFromFile = true;
                StartGame();
            }
            else
            {
                fileName.text = "";
                fileName.placeholder.GetComponent<Text>().text = "Error while opening file!";
            }
        }

        private void StartGame()
        {
            SceneManager.LoadScene("board");
        }
    }
}