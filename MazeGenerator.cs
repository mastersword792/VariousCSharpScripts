/* * * * * * * * * * * * * * * *  * * * * * * * * * * *\
 *  Name: MazeGen.cs                                  *
 * Auth: mastersword792                               *
 * Date: 23 MAY 2022                                  *
 * About:                                             *
 * Uses Prim's Algorithm to generate a maze with      *
 * 4 vertices.                                        *
 *                                                    *
 * Encodes the "door" data into bit shift 0 thru 4    *
 * within an Signed 32-bit Integer:                   *
 * Shift 0 = Left                                     *
 * Shift 1 = Right                                    *
 * Shift 2 = Down                                     *
 * Shift 3 = Up                                       *
 *                                                    *
 * Door Data:                                         *
 * 1 = Door                                           *
 * 0 = No Door                                        *
 *                                                    *
 *                                                    *
\* * * * * * * * * * * * * * * *  * * * * * * * * * * */

using System;
using System.Collections.Generic;

namespace MazeGeneration
{
	public enum CellStatus { Unreached, Frontier, Painted, Final }
	
	public struct Coords
	{
		public int X;
		public int Y;
		public Coords(int X, int Y)
		{
			this.X = X;
			this.Y = Y;
		}
	}
	
	public class Maze
    {
        public int Height;
        public int Width;
        private Random Rand;

        public Maze(int Width, int Height)
        {
            this.Height = Height;
            this.Width = Width;
            this.Rand = new Random();
        }
        
        public int[,] Generate()
        {
            int[,] MapInt = new int[this.Height, this.Width];
            CellStatus[,] PaintedCells = new CellStatus[this.Height, this.Width];
            Coords Start = new Coords(this.Rand.Next(this.Width), this.Rand.Next(this.Height));

            for (int i = 0; i < this.Height; i++)
            {
                for (int j = 0; j < this.Width; j++)
                {
                    MapInt[i, j] = 0;
                    PaintedCells[i, j] = CellStatus.Unreached;
                }
            }

            return PrimsAlgorithm(MapInt, PaintedCells, Start);
        }

        #region int[,] Generate.PrimsAlgorithm() --> Generates a Maze using Prim's Algorithm.
        private int[,] PrimsAlgorithm(int[,] MapInt, CellStatus[,] PaintedCells, Coords Start)
        {
            List<Coords> c1 = new List<Coords>();
            List<Coords> c2 = new List<Coords>();
            do
            {
                c1.Clear();
                c2.Clear();

                int x = Start.X;
                int y = Start.Y;

                PaintedCells[y, x] = CellStatus.Painted;

                c1 = this.GetAdjacentPaintedCells(Start, PaintedCells);

                if (c1.Count > 0)
                {
                    Coords n;
                    if (c1.Count == 1)
                    {
                        n = c1[0];
                    }
                    else
                    {
                        n = c1[this.Rand.Next(c1.Count)];
                    }

                    Direction d = GetDirection(Start, n);
                    if (d == Direction.UP)
                    {
                        MapInt[y, x] |= (1 << 3);
                        MapInt[n.Y, n.X] |= (1 << 2);
                    }
                    else if (d == Direction.DOWN)
                    {
                        MapInt[y, x] |= (1 << 2);
                        MapInt[n.Y, n.X] |= (1 << 3);
                    }
                    else if (d == Direction.LEFT)
                    {
                        MapInt[y, x] |= (1 << 1);
                        MapInt[n.Y, n.X] |= (1 << 0);
                    }
                    else if (d == Direction.RIGHT)
                    {
                        MapInt[y, x] |= (1 << 0);
                        MapInt[n.Y, n.X] |= (1 << 1);
                    }
                }

                PaintedCells = this.PaintAdjacentCellsFrontier(Start, PaintedCells);

                c2 = this.GetFrontierCells(PaintedCells);
                
                if (c2.Count == 1) Start = c2[0];
                else if(c2.Count > 1) Start = c2[this.Rand.Next(c2.Count)];
            } while (c2.Count > 0);

            return MapInt;
        }

        private List<Coords> GetAdjacentPaintedCells(Coords c, CellStatus[,] PaintedCells)
        {
            int x = c.X;
            int y = c.Y;
            List<Coords> n = new List<Coords>();

            if(x > 0)
            {
                if(PaintedCells[y, x - 1] == CellStatus.Painted)
                    n.Add(new Coords(x - 1, y));
            }

            if(x < (this.Width - 1))
            {
                if (PaintedCells[y, x + 1] == CellStatus.Painted)
                    n.Add(new Coords(x + 1, y));
            }

            if(y > 0)
            {
                if (PaintedCells[y - 1, x] == CellStatus.Painted)
                    n.Add(new Coords(x, y - 1));
            }

            if(y < (this.Height - 1))
            {
                if (PaintedCells[y + 1, x] == CellStatus.Painted)
                    n.Add(new Coords(x, y + 1));
            }

            return n;
        }

        private CellStatus[,] PaintAdjacentCellsFrontier(Coords c, CellStatus[,] PaintedCells)
        {
            int x = c.X;
            int y = c.Y;

            if (x > 0)
            {
                if (PaintedCells[y, x - 1] == CellStatus.Unreached)
                    PaintedCells[y, x - 1] = CellStatus.Frontier;
            }

            if (x < (this.Width - 1))
            {
                if (PaintedCells[y, x + 1] == CellStatus.Unreached)
                    PaintedCells[y, x + 1] = CellStatus.Frontier;
            }

            if (y > 0)
            {
                if (PaintedCells[y - 1, x] == CellStatus.Unreached)
                    PaintedCells[y - 1, x] = CellStatus.Frontier;
            }

            if (y < (this.Height - 1))
            {
                if (PaintedCells[y + 1, x] == CellStatus.Unreached)
                    PaintedCells[y + 1, x] = CellStatus.Frontier;
            }

            return PaintedCells;
        }

        private List<Coords> GetFrontierCells(CellStatus[,] PaintedCells)
        {
            List<Coords> n = new List<Coords>();
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    if (PaintedCells[y, x] == CellStatus.Frontier)
                    {
                        n.Add(new Coords(x, y));
                    }
                }
            }
            
            return n;
        }

        private static Direction GetDirection(Coords c, Coords n)
        {
            int dx = n.X - c.X;
            int dy = n.Y - c.Y;

            if (dx == 1) return Direction.RIGHT;
            if (dx == -1) return Direction.LEFT;
            if (dy == 1) return Direction.DOWN;
            if (dy == -1) return Direction.UP;

            return Direction.NULL;
        }
        #endregion
    }
}
