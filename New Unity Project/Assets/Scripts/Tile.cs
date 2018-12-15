using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Height { H0 = 1, H1, H2, H3, ROOF };

public class Tile : MonoBehaviour
{

    public static readonly float SIZE = 1.0f;
    //    public static readonly float OFFSET = SIZE / 2;
    public static readonly float DISTANCE = SIZE / 10;
    public static readonly Vector3 OFFSET = new Vector3(SIZE / 2, 0, SIZE / 2);

    public Height Height { get; set; }
    public Player Player { get; set; }
    public bool HasPlayer() { return Player != null; }

    private void Start()
    {
        Height = Height.H0;
    }
}
