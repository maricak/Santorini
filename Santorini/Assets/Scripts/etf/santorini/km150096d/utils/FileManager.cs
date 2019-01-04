
using System;
using System.IO;
using UnityEngine;

namespace etf.santorini.km150096d.utils
{
    public class FileManager
    {
        #region Singleton
        private static FileManager instance = null;
        private FileManager() { }
        public static FileManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new FileManager();
                return instance;
            }
        }
        #endregion

        private StreamWriter output = null;
        private StreamReader input = null;

        private static readonly char[] ROWS = { 'A', 'B', 'C', 'D', 'E' };
        private static readonly int[] COLS = { 1, 2, 3, 4, 5 };

        public Vector2 Src { get; set; }
        public Vector2 Dst { get; set; }
        public Vector2 Build { get; set; }


        #region input
        public bool SetInput(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith("txt"))
            {
                input = null;
                return false;
            }
            try
            {
                input = new StreamReader(fileName);
            }
            catch (Exception)
            {
                input = null;
                return false;
            }
            return true;
        }

        public bool HasNextPosition()
        {
            if (input != null && !input.EndOfStream)
            {
                return true;
            }
            else
            {
                input.Close();
                return false;
            }

        }

        public Vector2 NextPosition()
        {
            int row = input.Read();
            int col = input.Read();

            // skip blank or new line
            input.Read();

            row -= 'A';
            col -= '0';

            // file is corrupted
            if (row < 0 || row >= 5 || col < 0 || col >= 5)
            {
                throw new IOException();
            }

            return new Vector2(row, col);
        }
        #endregion

        #region output
        public void SaveFile()
        {
            output.Close();
        }

        public void SetOutput()
        {
            output = new StreamWriter("game" + DateTime.Now.ToString("yyyy_dd_M_HH_mm_ss") + ".txt");
        }

        public void WritePositions()
        {
            WriteVector(Src);
            WriteChar(' ');
            WriteVector(Dst);
            WriteChar('\n');
        }

        public void WriteMove()
        {
            WriteVector(Src);
            WriteChar(' ');
            WriteVector(Dst);
            WriteChar(' ');
            WriteVector(Build);
            WriteChar('\n');
        }
        private void WriteChar(char c)
        {
            output.Write(c);
        }

        private void WriteVector(Vector2 position)
        {
            int x = (int)position.x;
            int y = (int)position.y;

            output.Write(ROWS[x]);
            output.Write(COLS[y]);
        }
        #endregion

    }
}

