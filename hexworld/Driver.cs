namespace hexworld
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            using (var gw = new HexRender(1280, 720)) gw.Run();

//            var tile = JsonConvert.DeserializeObject<TileInfo>("{\"mesh\":\"RightColumn\", \"pos\": {\"x\": 1, \"y\": 2, \"z\": 3}}");
//
//            Console.Out.WriteLine("tile = {0}", tile);
//            Console.ReadKey();
        }
    }
}