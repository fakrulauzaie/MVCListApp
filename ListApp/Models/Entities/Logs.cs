namespace ListApp.Models.Entities
{
    public class Logs
    {
        public DateTime TimeStamp { get; set; }
        public string Level { get; set; }
        public string RenderedMessage { get; set; }
    }
}
