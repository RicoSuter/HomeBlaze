namespace HomeBlaze.Services.Tests
{
    public static class Test
    {
        public static void Wait(Action action)
        {
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                }

                Thread.Sleep(10);
            }

            action();
        }
    }
}
