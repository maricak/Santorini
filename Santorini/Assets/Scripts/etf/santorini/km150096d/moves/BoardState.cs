

using System;
using etf.santorini.km150096d.model;
using UnityEngine;

namespace etf.santorini.km150096d.moves
{
    
     public struct BoardState
  {
/*
      public Tile[,] tiles;
      public PlayerID currentPlayer;
      public Vector2[] currentPositions;
      public PlayerID otherPlayer;
      public Vector2[] otherPositions;
      public bool[,] possibleMoves;
      public bool[,] possibleBuilds;
      public int currentDepth;

      public BoardState(Tile[,] tiles,
          PlayerID currentPlayer, Vector2[] currentPositions,
          PlayerID otherPlayer, Vector2[] otherPositions,
          int currentDepth)
      {
          //this.tiles = new Tile[Board.DIM, Board.DIM];
          //Array.Copy(tiles, this.tiles, Board.DIM * Board.DIM);
          CopyTiles(tiles, out this.tiles);
          this.currentPlayer = currentPlayer;
          this.currentPositions = currentPositions;
          this.otherPlayer = otherPlayer;
          this.otherPositions = otherPositions;
          this.possibleMoves = new bool[Board.DIM, Board.DIM];
          this.possibleBuilds = new bool[Board.DIM, Board.DIM];
          this.currentDepth = currentDepth;

          this.tiles[0, 0].Height = Height.ROOF;
      }

      internal float Evaluate()
      {
          // TODO;promeni
          return currentPositions[0].x + currentPositions[0].y;
      }        

      internal void MovePlayer(Vector2 srcPosition, Vector2 dstPosition)
      {
          tiles[(int)dstPosition.x, (int)dstPosition.y].Player = tiles[(int)srcPosition.x, (int)srcPosition.y].Player;
          tiles[(int)srcPosition.x, (int)srcPosition.y].Player = null;
      }

      internal void Build(Vector2 buildPosition)
      {
          tiles[(int)buildPosition.x, (int)buildPosition.y].Height++;
      }

      internal bool CurrentWins()
      {
          // stojim na tri sprata ili drugi ne moze da se pomeri
          if(tiles[(int)currentPositions[0].x, (int)currentPositions[0].y].Height == Height.H3 ||
              tiles[(int)currentPositions[1].x, (int)currentPositions[1].y].Height == Height.H3)
          {
              return true;
          } 
          if(!Move.CanMove(otherPositions, tiles))
          {
              return true;
          }

          return false;
      }

      internal bool OtherWins()
      {
          // stojim na tri sprata ili drugi ne moze da se pomeri
          if (tiles[(int)otherPositions[0].x, (int)otherPositions[0].y].Height == Height.H3 ||
              tiles[(int)otherPositions[1].x, (int)otherPositions[1].y].Height == Height.H3)
          {
              return true;

          }
          if (!Move.CanMove(currentPositions, tiles))
          {
              return true;
          }
          return false;
      }

      private static void CopyTiles(Tile[,] original, out Tile[,] copy)
      {
          copy = new Tile[Board.DIM, Board.DIM];
          for (int i = 0; i < Board.DIM; i++)
          {
              for (int j = 0; j < Board.DIM; j++)
              {
                  copy[i, j] = new Tile()
                  {
                      Height = original[i, j].Height,
                      Player = original[i, j].Player == null ? null : new Player()
                      {
                          Id = original[i, j].Player.Id, 
                          Position = new Vector2(original[i, j].Player.Position.x, original[i, j].Player.Position.y), 
                      }                        
                  };                    
              }
          }
      }
 */ }



}
