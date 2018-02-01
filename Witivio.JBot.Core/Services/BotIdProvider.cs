namespace Witivio.JBot.Core.Services
{
    public interface IBotIdProvider
    {
        string BotId { get; }
    }
    public class BotIdProvider : IBotIdProvider
    {
        public string BotId { get; set; }
    }
}