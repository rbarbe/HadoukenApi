namespace Hadouken.HadoukenApi.Models
{
    public class File
    {
        public int Priority;
        public string Name { get; set; }
        public long Size { get; set; }
        public long Downloaded { get; set; }
    }
}