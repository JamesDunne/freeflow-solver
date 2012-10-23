using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solver
{
    class Program
    {
        static void Main(string[] args)
        {
#if false
            var initial = Board.Initial.Validate(new Board(
                new Pipe[4, 4] {
                    { 0, 0, 0, 0 },
                    { 0, 1, 2, 0 },
                    { 0, 0, 0, 0 },
                    { 2, 0, 0, 1 }
                }
            ));
#else
            var initial = Board.Initial.Validate(new Board(
                new Pipe[9, 9] {
                    { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 2, 6, 7, 0, 9 },
                    { 0, 2, 0, 0, 5, 0, 8, 0, 0 },
                    { 0, 0, 0, 4, 0, 0, 0, 0, 7 },
                    { 1, 0, 0, 0, 0, 0, 6, 0, 0 },
                    { 0, 0, 3, 0, 0, 0, 0, 0, 0 },
                    { 0, 0, 0, 0, 5, 0, 3, 4, 0 },
                    { 0, 0, 0, 0, 0, 0, 0, 9, 0 },
                    { 1, 0, 0, 0, 0, 0, 0, 8, 0 },
                }
            ));
#endif
            if (!initial.HasValue)
            {
                Console.WriteLine("Invalid board!");
                return;
            }

            Console.WriteLine(initial.Value.Board.ToString());

            var nextBoardStack = new Stack<Board.InProgress>();
            var nextMovesStack = new Stack<IEnumerator<Board.InProgress>>();

            var firstBoard = new Board.InProgress(initial.Value);
            nextBoardStack.Push(firstBoard);
            nextMovesStack.Push(firstBoard.GetMoves().GetEnumerator());

            while (nextBoardStack.Count > 0)
            {
                var board = nextBoardStack.Pop();
                if (board.Board.IsFinal())
                {
                    Console.WriteLine(Environment.NewLine + board.Board.ToString());
                    continue;
                }

                var nextMoves = nextMovesStack.Peek();
                if (nextMoves.MoveNext())
                {
                    var newBoard = nextMoves.Current;
                    Console.WriteLine(Environment.NewLine + newBoard.Board.ToString());
                    nextBoardStack.Push(newBoard);
                    nextMovesStack.Push(newBoard.GetMoves().GetEnumerator());
                }
                else
                {
                    nextMovesStack.Pop();
                }
            }
        }
    }

    public struct Pipe
    {
        public readonly byte Color;
        public static implicit operator Pipe(byte color) { return new Pipe(color); }

        Pipe(byte color)
        {
            Color = color;
        }
    }

    public enum Direction
    {
        North,
        South,
        East,
        West
    }

    public struct Pos
    {
        public readonly int X, Y;

        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int ManhattanDistanceFrom(Pos b)
        {
            return Math.Abs(b.X - X) + Math.Abs(b.Y - Y);
        }

        public int DoubleManhattanDistanceFrom(Pos doubleB)
        {
            return Math.Abs(doubleB.X - X * 2) + Math.Abs(doubleB.Y - Y * 2);
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }

        public static bool operator ==(Pos a, Pos b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Pos a, Pos b)
        {
            return a.X != b.X || a.Y != b.Y;
        }
    }

    public struct Path
    {
        public readonly Board Board;
        public readonly byte Color;
        public readonly Pos Pos;
        public readonly Direction FromDirection;

        public Path(Board board, byte color, Pos pos, Direction dir)
        {
            Board = board;
            Color = color;
            Pos = pos;
            FromDirection = dir;
        }

        public Path ApplyPathTo(Pos dest, Pos final, Direction direction)
        {
            // Apply the current path to a copy of the board:
            var newBoard = Board.Clone();
            switch (direction)
            {
                case Direction.North:
                    for (int y = Pos.Y - 1; y >= dest.Y; --y)
                    {
                        if (new Pos(Pos.X, y) == final) break;
                        newBoard.OverwriteClearPipe(y, Pos.X, Color);
                    }
                    break;
                case Direction.South:
                    for (int y = Pos.Y + 1; y <= dest.Y; ++y)
                    {
                        if (new Pos(Pos.X, y) == final) break;
                        newBoard.OverwriteClearPipe(y, Pos.X, Color);
                    }
                    break;
                case Direction.East:
                    for (int x = Pos.X + 1; x <= dest.X; ++x)
                    {
                        if (new Pos(x, Pos.Y) == final) break;
                        newBoard.OverwriteClearPipe(Pos.Y, x, Color);
                    }
                    break;
                case Direction.West:
                    for (int x = Pos.X - 1; x >= dest.X; --x)
                    {
                        if (new Pos(x, Pos.Y) == final) break;
                        newBoard.OverwriteClearPipe(Pos.Y, x, Color);
                    }
                    break;
            }
            return new Path(newBoard, Color, dest, direction);
        }
    }

    public struct PosPair
    {
        public readonly Pos A, B;

        public PosPair(Pos a, Pos b)
        {
            A = a;
            B = b;
        }
    }

    public struct PipeEndpoints
    {
        readonly Pos[] a, b;

        public PipeEndpoints(Dictionary<byte, PosPair> pairs)
        {
            a = new Pos[pairs.Keys.Max()];
            b = new Pos[pairs.Keys.Max()];
            foreach (var pair in pairs)
            {
                a[pair.Key - 1] = pair.Value.A;
                b[pair.Key - 1] = pair.Value.B;
            }
        }

        PipeEndpoints(Pos[] a_s, Pos[] b_s)
        {
            a = a_s;
            b = b_s;
        }

        public PipeEndpoints Clone()
        {
            return new PipeEndpoints((Pos[])a.Clone(), (Pos[])b.Clone());
        }

        public IList<Pos> As { get { return a; } }
        public IList<Pos> Bs { get { return b; } }

        public Pos A(byte color) { return a[color - 1]; }
        public Pos B(byte color) { return b[color - 1]; }
    }

    public struct Board
    {
        public readonly Pipe[,] Pipe;
        public readonly PipeEndpoints EndPoints;

        public Board(Pipe[,] pipe)
        {
            Pipe = pipe;
            EndPoints = new PipeEndpoints();
        }

        Board(Pipe[,] pipe, PipeEndpoints endPoints)
        {
            Pipe = pipe;
            EndPoints = endPoints;
        }

        public void OverwriteClearPipe(int y, int x, byte color)
        {
            if (this[y, x] != 0) throw new ArgumentException();
            Pipe[y, x] = color;
        }

        public byte this[int y, int x] { get { return Pipe[y /* + Pipe.GetLowerBound(0) */, x /* + Pipe.GetLowerBound(1) */].Color; } }
        public byte this[Pos p] { get { return Pipe[p.Y /* + Pipe.GetLowerBound(0) */, p.X /* + Pipe.GetLowerBound(1) */].Color; } }
        public int Width { get { return Pipe.GetUpperBound(1) - Pipe.GetLowerBound(1) + 1; } }
        public int Height { get { return Pipe.GetUpperBound(0) - Pipe.GetLowerBound(0) + 1; } }

        public Board Clone()
        {
            Pipe[,] newPipe = new Pipe[Height, Width];
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    newPipe[y, x] = Pipe[y, x];
            return new Board(newPipe, EndPoints.Clone());
        }

        Pos GetNorthernMostPoint(Pos src, Pos dest, int min)
        {
            for (int y = src.Y - 1; y >= min; --y)
            {
                if (new Pos(src.X, y) == dest)
                    return dest;
                if (this[y, src.X] != 0)
                    return new Pos(src.X, y + 1);
            }
            return new Pos(src.X, min);
        }

        Pos GetSouthernMostPoint(Pos src, Pos dest, int max)
        {
            for (int y = src.Y + 1; y <= max; ++y)
            {
                if (new Pos(src.X, y) == dest)
                    return dest;
                if (this[y, src.X] != 0)
                    return new Pos(src.X, y - 1);
            }
            return new Pos(src.X, max);
        }

        Pos GetEasternMostPoint(Pos src, Pos dest, int max)
        {
            for (int x = src.X + 1; x <= max; ++x)
            {
                if (new Pos(x, src.Y) == dest)
                    return dest;
                if (this[src.Y, x] != 0)
                    return new Pos(x - 1, src.Y);
            }
            return new Pos(max, src.Y);
        }

        Pos GetWesternMostPoint(Pos src, Pos dest, int min)
        {
            for (int x = src.X - 1; x >= min; --x)
            {
                if (new Pos(x, src.Y) == dest)
                    return dest;
                if (this[src.Y, x] != 0)
                    return new Pos(x + 1, src.Y);
            }
            return new Pos(min, src.Y);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Height * (Width + 2));
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                    sb.Append((char)('0' + this[y, x]));
                if (y < Height - 1) sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Represents the initial state of a board.
        /// </summary>
        public struct Initial
        {
            public readonly Board Board;

            Initial(Board board)
            {
                Board = board;
            }

            public static Initial? Validate(Board board)
            {
                var pipes = new Dictionary<byte, PosPair>(16);

                for (int y = 0; y < board.Height; ++y)
                    for (int x = 0; x < board.Width; ++x)
                    {
                        byte color = board[y, x];
                        if (color == 0) continue;

                        PosPair pair;
                        if (pipes.TryGetValue(color, out pair))
                        {
                            pipes[color] = new PosPair(pair.A, new Pos(x, y));
                        }
                        else
                        {
                            pipes[color] = new PosPair(new Pos(x, y), new Pos(-1, -1));
                        }
                    }

                // Check that each color pipe has 2 endpoints.
                foreach (var pair in pipes)
                    if (pair.Value.B.X == -1)
                        return null;

                return new Initial(new Board(board.Pipe, new PipeEndpoints(pipes)));
            }
        }

        public class InProgress
        {
            public readonly Board Board;
            public readonly HashSet<byte> ColorsMoved;

            public InProgress(Initial initial)
            {
                Board = initial.Board;
                ColorsMoved = new HashSet<byte>();
            }

            InProgress(Board board, byte[] colorsMoved, byte color)
            {
                Board = board;
                ColorsMoved = new HashSet<byte>(colorsMoved);
                ColorsMoved.Add(color);
            }

            public IEnumerable<InProgress> GetMoves()
            {
                // A move is a full connection between two same-colored pipe endpoints.
                // Figure out which moves we can make:
                var midpoint = new Pos(Board.Width - 1, Board.Height - 1);

                // Start from the outside and work our way inwards:
                foreach (var point in Board.EndPoints.As.Concat(Board.EndPoints.Bs).OrderByDescending(p => p.DoubleManhattanDistanceFrom(midpoint)))
                {
                    var color = Board[point];
                    if (ColorsMoved.Contains(color)) continue;

                    var thisColorMoved = new HashSet<byte>(ColorsMoved);
                    thisColorMoved.Add(color);

                    var a = point;
                    var b = Board.EndPoints.B(color);
                    if (a == b)
                        b = Board.EndPoints.A(color);

                    // Enumerate all possible paths from a to b:
                    var paths = new Queue<Path>();
                    paths.Enqueue(new Path(Board, color, a, Direction.East));

                    while (paths.Count > 0)
                    {
                        var path = paths.Dequeue();

                        var fromDir = path.FromDirection;
                        var pos = path.Pos;

                        if (fromDir != Direction.North)
                        {
                            // Try north:
                            for (int min = 0; min <= pos.Y; ++min)
                            {
                                var c = path.Board.GetNorthernMostPoint(pos, b, min);
                                if (c == pos) break;
                                if (c == a) break;
                                min = c.Y;

                                var newpath = path.ApplyPathTo(c, b, Direction.North);
                                if (c == b) yield return new InProgress(newpath.Board, ColorsMoved.ToArray(), color);

                                paths.Enqueue(newpath);
                            }
                        }
                        if (fromDir != Direction.West)
                        {
                            // Try west:
                            for (int min = 0; min <= pos.X; ++min)
                            {
                                var c = path.Board.GetWesternMostPoint(pos, b, min);
                                if (c == pos) break;
                                if (c == a) break;
                                min = c.X;

                                var newpath = path.ApplyPathTo(c, b, Direction.West);
                                if (c == b) yield return new InProgress(newpath.Board, ColorsMoved.ToArray(), color);

                                paths.Enqueue(newpath);
                            }
                        }
                        if (fromDir != Direction.South)
                        {
                            // Try south:
                            for (int max = path.Board.Height - 1; max >= pos.Y; --max)
                            {
                                var c = path.Board.GetSouthernMostPoint(pos, b, max);
                                if (c == pos) break;
                                if (c == a) break;
                                max = c.Y;

                                var newpath = path.ApplyPathTo(c, b, Direction.South);
                                if (c == b) yield return new InProgress(newpath.Board, ColorsMoved.ToArray(), color);

                                paths.Enqueue(newpath);
                            }
                        }
                        if (fromDir != Direction.East)
                        {
                            // Try east:
                            for (int max = path.Board.Width - 1; max >= pos.X; --max)
                            {
                                var c = path.Board.GetEasternMostPoint(pos, b, max);
                                if (c == pos) break;
                                if (c == a) break;
                                max = c.X;

                                var newpath = path.ApplyPathTo(c, b, Direction.East);
                                if (c == b) yield return new InProgress(newpath.Board, ColorsMoved.ToArray(), color);

                                paths.Enqueue(newpath);
                            }
                        }
                    }
                }

                yield break;
            }

            static T[] ConcatArrays<T>(T[] arr, params T[] appended)
            {
                T[] newarr = new T[arr.Length + appended.Length];
                Array.Copy(arr, newarr, arr.Length);
                Array.Copy(appended, 0, newarr, arr.Length, appended.Length);
                return newarr;
            }
        }

        public bool IsFinal()
        {
            // Determine if this board state is a winner.
            for (int y = 0; y < Height; ++y)
                for (int x = 0; x < Width; ++x)
                    if (this[y, x] == 0) return false;
            return true;
        }
    }
}
