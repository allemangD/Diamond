namespace hexworld
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            using (var gw = new HexRender(1280, 720)) gw.Run();
        }
    }
}