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

// MazeGenerator.cs
/*

Sector Connection Directions
Bit:  3  2    1    0
Name: UP DOWN LEFT RIGHT

Sector Data:
Bit:  13-10 9-5  4
Name: YLen  XLen IsRoom

Sector Door Positions:
Bit:  30-26 25-23 21-18 17-14
Name: Up    Down  Left  Right


*/

using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGeneration
{
    public class MazeGeneration
    {
        ///<summary>
        ///Generates a maze using Prim's Algorithm.
        ///</summary>
        ///<param name="Width">How many cells wide the maze will be.</param>
        ///<param name="Height">How many cells high the maze will be.</param>
        ///<param name="Seed">The seed for generation. If null, the seed will be random.</param>
        ///<return>Returns a visual representation of the generated maze. 'X' is a wall, ' ' is floor.</return>
        public static char[,] Test(int Width, int Height, int? Seed)
        {
            uint[,] maze = Generarion.Generate(Width, Height, Seed);
            char[,] display = new char[3 * Height, 3 * Width];
            for (int y = 0; y < maze.GetLength(0); y++)
            {
                for(int x = 0; x < maze.GetLength(1); x++)
                {
                    int X = (3 * x) + 1;
                    int Y = (3 * y) + 1;
                    display[Y - 1, X - 1] = 'X';
                    display[Y - 1, X + 1] = 'X';
                    display[Y + 1, X - 1] = 'X';
                    display[Y + 1, X + 1] = 'X';
                    display[Y, X] = ' ';
                    display[Y - 1, X] = ((maze[y, x] & (uint)Direction.UP) == (uint)Direction.UP) ? ' ' : 'X';
                    display[Y + 1, X] = ((maze[y, x] & (uint)Direction.DOWN) == (uint)Direction.DOWN) ? ' ' : 'X';
                    display[Y, X - 1] = ((maze[y, x] & (uint)Direction.LEFT) == (uint)Direction.LEFT) ? ' ' : 'X';
                    display[Y, X + 1] = ((maze[y, x] & (uint)Direction.RIGHT) == (uint)Direction.RIGHT) ? ' ' : 'X';
                }
            }
            return display;
        }
    }

    public enum Direction
    {
        NULL = 0, UP = 8, DOWN = 4, LEFT = 2, RIGHT = 1
    }

    internal enum CellStatus
    {
        Painted, Frontier, Unreached, NULL
    }

    public class Coords
    {
        public int X;
        public int Y;

        public Coords(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static Direction GetDirection(Coords c1, Coords c2)
        {
            int dy = c2.Y - c1.Y;
            int dx = c2.X - c1.X;

            if (dy < 0) return Direction.UP;
            else if (dy > 0) return Direction.DOWN;
            else if (dx < 0) return Direction.LEFT;
            else if (dx > 0) return Direction.RIGHT;
            else return Direction.NULL;
        }

        public static Direction GetOppositeDirection(Coords c1, Coords c2)
        {
            int dy = c2.Y - c1.Y;
            int dx = c2.X - c1.X;

            if (dy < 0) return Direction.DOWN;
            else if (dy > 0) return Direction.UP;
            else if (dx < 0) return Direction.RIGHT;
            else if (dx > 0) return Direction.LEFT;
            else return Direction.NULL;
        }
    }

    public class Generation
    {
        private List<Coords> GetSurroundingCells(Coords selected, int Width, int Height)
        {
            int x = selected.X;
            int y = selected.Y;
            List<Coords> n = new List<Coords>();

            if (x > 0)
            {
                n.Add(new Coords(x - 1, y));
            }

            if (x < (Width - 1))
            {
                n.Add(new Coords(x + 1, y));
            }

            if (y > 0)
            {
                n.Add(new Coords(x, y - 1));
            }

            if (y < (Height - 1))
            {
                n.Add(new Coords(x, y + 1));
            }

            return n;
        }

        #region Prim's Algorithm --> Complete.
        /// <summary>
        /// Generates a maze based around Prim's Algorithm.
        /// </summary>
        /// <param name="Seed"></param>
        /// <returns>Returns an encoded 2D unsigned integer array that describes the connections between rooms.
        /// bin(0) = no door, bin(1) = door. <br/>
        /// Position: 3      2       1       0 <br/>
        /// Diection: UP     DOWN    RIGHT   LEFT
        /// </returns>
        /// 
        public static uint[,] Generate(int Width, int Height, int? Seed)
        {
            Random Rand = (Seed == null)? new Random() : new Random((int)Seed);
            uint[,] MapInt = new uint[Height, Width];
            CellStatus[,] PaintedCells = new CellStatus[Height, Width];
            Coords Start = new Coords(Rand.Next(Width), Rand.Next(Height));

            /// TODO: Ensure MapInt returns the MetaData in reference to the spreadsheet "%userprofile%\Desktop\MapInt Metadata.xlsx"
            /// 
            //byte coordsMeta = 0x00;
            for (byte i = 0; i < Height; i++)
            {
                for (byte j = 0; j < Width; j++)
                {
                    /*
                    coordsMeta |= (byte)(i << 16);
                    coordsMeta |= (byte)(i << 24);
                    */

                    MapInt[i, j] = 0;
                    PaintedCells[i, j] = CellStatus.Unreached;
                }
            }

            return Algorithm(MapInt, PaintedCells, Start, Width, Height, Rand);
        }

        private static uint[,] Algorithm(uint[,] MapInt, CellStatus[,] PaintedCells, Coords Start, int Width, int Height, Random Rand)
        {
            List<Coords> adj = new List<Coords>();
            List<Coords> frontier = new List<Coords>();
            do
            {
                adj.Clear();
                frontier.Clear();

                int x = Start.X;
                int y = Start.Y;

                PaintedCells[y, x] = CellStatus.Painted;

                adj = GetAdjacentPaintedCells(Start, PaintedCells, Width, Height);

                if (adj.Count > 0)
                {
                    Coords chosen = (adj.Count == 1) ? adj[0] : adj[Rand.Next(adj.Count)];
                    //Direction d = Coords.GetDirection(Start, n);
                    
                    MapInt[y, x] |= (uint)Coords.GetDirection(Start, chosen);
                    MapInt[chosen.Y, chosen.X] |= (uint)Coords.GetOppositeDirection(Start, chosen);
                    
                    /*
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
                    */
                }

                PaintedCells = PaintAdjacentCellsFrontier(Start, PaintedCells, Width, Height);

                frontier = GetFrontierCells(PaintedCells, Width, Height);

                if (frontier.Count == 1) Start = frontier[0];
                else if (frontier.Count > 1) Start = frontier[Rand.Next(frontier.Count)];
            } while (frontier.Count > 0);
            return MapInt;
        }

        private static List<Coords> GetAdjacentPaintedCells(Coords c, CellStatus[,] PaintedCells, int Width, int Height)
        {
            int x = c.X;
            int y = c.Y;
            List<Coords> n = new List<Coords>();

            if (x > 0)
            {
                if (PaintedCells[y, x - 1] == CellStatus.Painted)
                    n.Add(new Coords(x - 1, y));
            }

            if (x < (Width - 1))
            {
                if (PaintedCells[y, x + 1] == CellStatus.Painted)
                    n.Add(new Coords(x + 1, y));
            }

            if (y > 0)
            {
                if (PaintedCells[y - 1, x] == CellStatus.Painted)
                    n.Add(new Coords(x, y - 1));
            }

            if (y < (Height - 1))
            {
                if (PaintedCells[y + 1, x] == CellStatus.Painted)
                    n.Add(new Coords(x, y + 1));
            }

            return n;
        }

        private static CellStatus[,] PaintAdjacentCellsFrontier(Coords c, CellStatus[,] PaintedCells, int Width, int Height)
        {
            int x = c.X;
            int y = c.Y;

            if (x > 0)
            {
                if (PaintedCells[y, x - 1] == CellStatus.Unreached)
                    PaintedCells[y, x - 1] = CellStatus.Frontier;
            }

            if (x < (Width - 1))
            {
                if (PaintedCells[y, x + 1] == CellStatus.Unreached)
                    PaintedCells[y, x + 1] = CellStatus.Frontier;
            }

            if (y > 0)
            {
                if (PaintedCells[y - 1, x] == CellStatus.Unreached)
                    PaintedCells[y - 1, x] = CellStatus.Frontier;
            }

            if (y < (Height - 1))
            {
                if (PaintedCells[y + 1, x] == CellStatus.Unreached)
                    PaintedCells[y + 1, x] = CellStatus.Frontier;
            }

            return PaintedCells;
        }

        private static List<Coords> GetFrontierCells(CellStatus[,] PaintedCells, int Width, int Height)
        {
            List<Coords> n = new List<Coords>();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (PaintedCells[y, x] == CellStatus.Frontier)
                    {
                        n.Add(new Coords(x, y));
                    }
                }
            }

            return n;
        }
        #endregion
    }

    public class PostGeneration
    {
        ///<summary> Generates multiple inaccessible holes in the map.</summary>
        ///<param Name='MapInt'>The generated map data.</param>
        ///<param Name='HoleLocs'>The list of locations within the map to generate hole.</param>
        ///<return>Returns the generated map data with the holes in it.</return>
        public static uint[,] GenerateMultipleHoles(uint[,] MapInt, List<Coords> HoleLocs)
        {
            var output = MapInt;
            foreach(Coords c in HoleLocs)
            {
                output = GenerateHole(output, c);
            }
            return output;
        }

        ///<summary> Generates an inaccessible hole in the map.</summary>
        ///<param Name='MapInt'>The generated map data.</param>
        ///<param Name='HoleLoc'>The location within the map to generate a hole.</param>
        ///<return>Returns the generated map data with the holes in it.</return>
        public static uint[,] GenerateHole(uint[,] MapInt, Coords HoleLoc)
        {
            uint[,] output = MapInt;
            output[HoleLoc.Y, HoleLoc.X] = 0;
            output[HoleLoc.Y, HoleLoc.X + 1] &= ~(1);
            output[HoleLoc.Y, HoleLoc.X - 1] &= ~(1 << 1);
            output[HoleLoc.Y - 1, HoleLoc.X] &= ~(1 << 2);
            output[HoleLoc.Y + 1, HoleLoc.X] &= ~(1 << 3);
            return output;
        }
    }

}
