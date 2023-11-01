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
        struct Pos
        {
            public int x;
            public int y;
            public Pos(int x, int y) { this.x = x; this.y = y; }
        }
        class Tick
        {
            ulong last_time = 0;
            ulong cur_time = 0;
            DateTime tmpDateTime;
            public bool GenerateTick(ulong delay)
            {
                tmpDateTime = DateTime.Now;
                cur_time = (ulong)tmpDateTime.Minute * 60000 + (ulong)tmpDateTime.Second * 1000 + (ulong)tmpDateTime.Millisecond;

                if (cur_time - delay >= last_time)
                {
                    //Console.WriteLine("Tick");
                    last_time = cur_time;
                    return true;
                }
                return false;
            }

        }

        class Snake
        {
            LinkedList<Pos> SnakeBody;
            public eDirection HeadDirection;

            public Snake(Pos startPos, eDirection startDirection)
            {
                SnakeBody = new LinkedList<Pos>();
                SnakeBody.AddFirst(startPos);
                HeadDirection = startDirection;
            }

            public bool Move(bool eat = false)
            {
                if (HeadDirection == eDirection.up)    SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x, SnakeBody.ElementAt(0).y - 1));
                if (HeadDirection == eDirection.right) SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x + 2, SnakeBody.ElementAt(0).y));
                if (HeadDirection == eDirection.down)  SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x, SnakeBody.ElementAt(0).y + 1));
                if (HeadDirection == eDirection.left)  SnakeBody.AddFirst(new Pos(SnakeBody.ElementAt(0).x - 2, SnakeBody.ElementAt(0).y ));

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



        static void Main(string[] args)
        {
            Tick tick = new Tick();
            ConsoleKey key;
            Console.Title = "Snake";
            Snake snake = new Snake(new Pos(10, 10), eDirection.down);
            Console.CursorVisible = false;
            int count_tick = 0;

            try
            {
                while (true)
                {
                    if (tick.GenerateTick(150))
                    {

                        count_tick++;
                        if (count_tick % 10 == 0)

                            snake.Move(true);
                        else
                            snake.Move(false);

                        if (snake.CheckGameOver())
                        {

                            break;
                        }


                        snake.PrintSnake();

                    }

                    if (Console.KeyAvailable)
                    {
                        key = Console.ReadKey(true).Key;

                        if (key == ConsoleKey.UpArrow) { if (snake.HeadDirection != eDirection.down) snake.HeadDirection = eDirection.up; }
                        if (key == ConsoleKey.RightArrow) { if (snake.HeadDirection != eDirection.left) snake.HeadDirection = eDirection.right; }
                        if (key == ConsoleKey.DownArrow) { if (snake.HeadDirection != eDirection.up) snake.HeadDirection = eDirection.down; }
                        if (key == ConsoleKey.LeftArrow) { if (snake.HeadDirection != eDirection.right) snake.HeadDirection = eDirection.left; }

                        //Console.WriteLine(Console.ReadKey(true).Key);
                    }
                }
            }
            catch { }

            Console.SetCursorPosition(20, 10);
            Console.WriteLine("GAME OVER");
            
        }
    }
}