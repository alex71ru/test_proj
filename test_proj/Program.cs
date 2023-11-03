using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace test_proj
{
    internal class Program
    {
        enum eDirection
        {
            up = 0,
            right,
            down,
            left
        }

        struct VisualEl
        {
            public  Pos          position;
            public  ConsoleColor backgroundColor;
            public  ConsoleColor foregroundColor;
            public  char         value;

            public VisualEl(Pos pos = default , ConsoleColor bg = ConsoleColor.Black, ConsoleColor fg = ConsoleColor.White, char val = (char)0) 
            { position = pos; backgroundColor = bg; foregroundColor = fg; value = val; }


            public static bool operator ==(VisualEl lhs, VisualEl rhs)
            {
                if (lhs.position == rhs.position && lhs.backgroundColor == rhs.backgroundColor 
                    && lhs.foregroundColor == rhs.foregroundColor && lhs.value == rhs.value) { return true; }
                return false;
            }
            public static bool operator !=(VisualEl lhs, VisualEl rhs)
            {
                if (lhs.position != rhs.position || lhs.backgroundColor != rhs.backgroundColor
                    || lhs.foregroundColor != rhs.foregroundColor || lhs.value != rhs.value) { return true; }
                return false;
            }

        }

        struct Pos
        {
            public int x;
            public int y;
            public Pos(int x, int y) { this.x = x; this.y = y; }
            public static bool operator == (Pos lhs, Pos rhs)
            {
                if(lhs.x == rhs.x && lhs.y == rhs.y) { return true; }
                return false;
            }
            public static bool operator != (Pos lhs, Pos rhs)
            {
                if(lhs.x != rhs.x || lhs.y != rhs.y) { return true; }
                return false;
            }
        }

        interface IVisualObject
        {

        }

        class VisualHandler
        {
            List<IVisualObject> list;
            
            ConsoleColor?[,] cur_state;

            uint WIDTH;
            uint HEIGHT;

            public VisualHandler(uint width, uint height, List<IVisualObject> list)
            {
                this.list = list;
                this.WIDTH = width * 2;
                this.HEIGHT = height;
            }

            public void CreateCurState()
            {
                cur_state = new ConsoleColor?[HEIGHT, WIDTH];

                foreach (var item_0 in list)
                {
                    if (item_0 is Snake)
                    {
                        foreach (var item_1 in ((Snake)item_0).GetSnakeBody())
                        {
                            cur_state[item_1.y, (item_1.x * 2)] = new ConsoleColor?(((Snake)item_0).Color);
                            cur_state[item_1.y, (item_1.x * 2) + 1] = new ConsoleColor?(((Snake)item_0).Color);
                        }
                    }
                    if (item_0 is Apple)                    
                    {
                        cur_state[((Apple)item_0).Position.y, ((Apple)item_0).Position.x * 2] = new ConsoleColor?(((Apple)item_0).Color);
                        cur_state[((Apple)item_0).Position.y, (((Apple)item_0).Position.x * 2) + 1] = new ConsoleColor?(((Apple)item_0).Color);
                    }
                }
            }

            public void Print()
            {
                CreateCurState();


                for (int i = 0; i < HEIGHT; i++) 
                {
                    for(int j = 0; j < WIDTH; j++) 
                    {
                        if(cur_state[i, j] == null)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                            Console.SetCursorPosition(j, i);
                            Console.Write(" ");
                        }
                        else
                        {
                            Console.BackgroundColor = cur_state[i, j].Value;
                            Console.SetCursorPosition(j, i);
                            Console.Write(" ");
                            Console.ResetColor();
                        }
                    }                
                }
            }
        }

        class Tick
        {
            ulong Delay = 0;
            ulong last_time = 0;
            ulong cur_time = 0;
            DateTime tmpDateTime;


            public delegate void TickDelegate();
            public event TickDelegate? EvTick;


            public Tick(ulong delay, List<TickDelegate> delegates = null)
            {
                Delay = delay;

                if (delegates != null)
                {
                    foreach (var item in delegates)
                        EvTick += item;
                }
            }

            public bool GenerateTick(ulong delay = 0)
            {
                if (delay == 0) delay = Delay;
                tmpDateTime = DateTime.Now;

                cur_time = (ulong)tmpDateTime.Minute * 60000 + (ulong)tmpDateTime.Second * 1000 + (ulong)tmpDateTime.Millisecond;

                if (cur_time - delay >= last_time)
                {                    
                    last_time = cur_time;
                    EvTick?.Invoke();
                    return true;
                }
                return false;
            }

           
        }

        class Apple: IVisualObject
        {
            public Pos Position { get; set; }
            static Random randGenerate;
            public ConsoleColor Color { get; set; } = ConsoleColor.Red;

            public Apple(List<Pos> listPos)
            {               
                Position = listPos.ElementAt(randGenerate.Next(0, listPos.Count));
                GC.Collect();
            }

            static Apple()
            {
                randGenerate = new Random();
            }

            public void RefreshPosition(List<Pos> listPos)
            {
                Position = listPos.ElementAt(randGenerate.Next(0, listPos.Count));
                GC.Collect();
            }
        }

        class Snake: IVisualObject
        {
            LinkedList<Pos> SnakeBody;
            public eDirection HeadDirection;
            public ConsoleColor Color { get; set; } = ConsoleColor.Green;

            public Snake(Pos startPos, eDirection startDirection)
            {
                SnakeBody = new LinkedList<Pos>();
                SnakeBody.AddFirst(startPos);
                HeadDirection = startDirection;
            }

            public bool Move(bool eat = false)
            {
                if (HeadDirection == eDirection.up)    SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x, SnakeBody.ElementAt(0).y - 1));
                if (HeadDirection == eDirection.right) SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x + 1, SnakeBody.ElementAt(0).y));
                if (HeadDirection == eDirection.down)  SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x, SnakeBody.ElementAt(0).y + 1));
                if (HeadDirection == eDirection.left)  SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x - 1, SnakeBody.ElementAt(0).y ));

                if(!eat) SnakeBody.RemoveLast();

                return true;
            }

            public bool CheckGameOver()
            {
                Pos tmp = SnakeBody.ElementAt(0);

                for(int i = 1; i < SnakeBody.Count; i++) 
                {
                    if (tmp.x == SnakeBody.ElementAt(i).x && tmp.y == SnakeBody.ElementAt(i).y)
                        return true;
                }

                return false;                
            }

            public ref LinkedList<Pos> GetSnakeBody()
            {
                return ref SnakeBody;
            }

            public void SetHead(Pos newPos)
            {
                SnakeBody.RemoveFirst();
                SnakeBody.AddFirst(newPos);
            }

            public void PrintSnake()
            {
                Console.Clear();

                Console.BackgroundColor = ConsoleColor.Green;
                foreach (var item in SnakeBody)
                {
                    Console.SetCursorPosition(item.x, item.y);
                    Console.Write("  ");
                }

                Console.BackgroundColor = ConsoleColor.Black;
            }

        }
        
        class GameField : IVisualObject
        {
            uint HEIGHT;
            uint WIDTH;            

            public Apple apple;
            public Snake snake;

            

            public GameField( uint width, uint height)
            { 
                HEIGHT = height;
                WIDTH = width;
                snake = new Snake(new Pos(0, 0), eDirection.right);
                apple = new Apple(CheckFreePosition());
            }

            private void CheckOutOfRange()
            {
                Pos Head = snake.GetSnakeBody().ElementAt(0);
                if (Head.x < 0) snake.SetHead(new Pos((int)WIDTH - 1, Head.y));
                if (Head.x >= (int)WIDTH) snake.SetHead(new Pos(0, Head.y)); 
                if (Head.y < 0) snake.SetHead(new Pos(Head.x, (int)HEIGHT - 1)); 
                if (Head.y >= (int)HEIGHT) snake.SetHead(new Pos(Head.x, 0));               
            }

            private List<Pos> CheckFreePosition()
            {
                List<Pos> list = new List<Pos>();
                bool check = true;

                for (int i = 0; i < WIDTH; i++)
                {
                    for( int j = 0; j < HEIGHT; j++)
                    {
                        check = true;

                        for( int g = 0; g < snake.GetSnakeBody().Count; g++)
                        {
                            if(i == snake.GetSnakeBody().ElementAt(g).x && j == snake.GetSnakeBody().ElementAt(g).y)
                            {
                                check = false; break;
                            }
                        }

                        if(check) list.Add(new Pos(i, j));
                    }
                }
                return list;
            }

            public bool CheckGameOver()
            {
                LinkedList<Pos> tmpBody = snake.GetSnakeBody();
                Pos head = tmpBody.ElementAt(0);

                for (int i = 1; i < tmpBody.Count; i++)
                {
                    if (head.x == tmpBody.ElementAt(i).x && head.y == tmpBody.ElementAt(i).y)
                        return true;
                }

                return false;
            }



            public void MakeMove()
            {
                if (snake.GetSnakeBody().ElementAt(0) == apple.Position)
                { 
                    snake.Move(true);
                    apple.RefreshPosition(CheckFreePosition());
                }
                else                
                    snake.Move(false);

                CheckOutOfRange();

                if (CheckGameOver())
                {

                }
            }            

            public void GameFieldKeyHandler(ConsoleKey key)
            {
                if (key == ConsoleKey.UpArrow) { if (snake.HeadDirection != eDirection.down) snake.HeadDirection = eDirection.up; }
                if (key == ConsoleKey.RightArrow) { if (snake.HeadDirection != eDirection.left) snake.HeadDirection = eDirection.right; }
                if (key == ConsoleKey.DownArrow) { if (snake.HeadDirection != eDirection.up) snake.HeadDirection = eDirection.down; }
                if (key == ConsoleKey.LeftArrow) { if (snake.HeadDirection != eDirection.right) snake.HeadDirection = eDirection.left; }
            }

        }

        class KeyHandler
        {
            public delegate void KeyHandlerDelegate(ConsoleKey key);

            static public event KeyHandlerDelegate? KeyEventHandler;

            public static bool CheckKeyAvailable()
            {
                if (Console.KeyAvailable)
                {
                    KeyEventHandler?.Invoke(Console.ReadKey(true).Key);
                    return true;
                }
                return false;
            }
        }        

        static void Main(string[] args)
        {           
            GameField field = new GameField(20, 20);
            VisualHandler visualHandler = new VisualHandler(20, 20, new List<IVisualObject> {  field.apple,  field.snake });
            Tick tick = new Tick(50, new List<Tick.TickDelegate> { field.MakeMove, visualHandler.Print });
            KeyHandler.KeyEventHandler += field.GameFieldKeyHandler;

            Console.Title = "Snake";            
            Console.CursorVisible = false;            

            try
            {
                while (!field.CheckGameOver())
                {
                    tick.GenerateTick();
                    KeyHandler.CheckKeyAvailable();
                }
            }
            catch(Exception ex) 
            {
                Console.ResetColor();
                Console.WriteLine(ex.Message);            
            }

            Console.SetCursorPosition(15, 5);
            Console.WriteLine("GAME OVER");
            Console.ReadKey(false);
        }


        
    }
}