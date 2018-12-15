using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileManager
{
    private static FileManager instance = null;
    private FileManager() { }
    public static FileManager Instance
    {
        get { if (instance == null)
                instance = new FileManager();
            return instance; }
    }

    private StreamWriter file;

    private static readonly char[] ROWS = { 'A', 'B', 'C', 'D', 'E' };
    private static readonly int[] COLS = { 1, 2, 3, 4, 5 };

    public Vector2 Src { get; set; }
    public Vector2 Dst { get; set; }
    public Vector2 Build { get; set; }

    public void SaveFile()
    {
        file.Close();
    }

    public void CreateFile()
    {
        file = new StreamWriter("game" + DateTime.Now.ToString("yyyy_dd_M_HH_mm_ss") + ".txt");
    }

    public void WritePositions(PlayerType type)
    {
        Player player = Player.GetPlayer(type, 0);
        WriteVector(player.Position);
        WriteChar(' ');
        player = Player.GetPlayer(type, 1);
        WriteVector(player.Position);
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
        file.Write(c);
    }

    private void WriteVector(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        file.Write(ROWS[x]);
        file.Write(COLS[y]);
    }
}
