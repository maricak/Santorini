using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etf.santorini.km150096d
{
    public class Util
    {
        public static void MoveTile(Tile tile, int x, int y, int height)
        {
            tile.transform.position =
                Vector3.right * (x + x * Tile.DISTANCE)
                + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
        }

        public static void MoveHighlight(GameObject highlight, int x, int y, int height)
        {
            highlight.transform.position =
                Vector3.right * (x + x * Tile.DISTANCE)
               - Vector3.down * height * 0.10f
               + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
        }

        public static void MovePlayer(Player player, int x, int y, int height)
        {
            player.transform.position =
                Vector3.right * (x + x * Tile.DISTANCE)
                - Vector3.down * 0.45f
                - Vector3.down * height * 0.10f
                + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
        }

        public static void MoveBlock(Block block, int x, int y, int height)
        {
            block.transform.position =
                Vector3.right * (x + x * Tile.DISTANCE)
                - Vector3.down * height * 0.10f
                + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
        }

        public static void MoveRoof(Roof roof, int x, int y, int height)
        {
            roof.transform.position =
                Vector3.right * (x + x * Tile.DISTANCE)
                - Vector3.down * height * 0.10f
                + Vector3.down * 0.2f
                + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
        }
    }
}