namespace Witivio.JBot.Core.Models
{
    public enum UserPresence
    {
        Offline = 0,
        Online = 1,
        Away = 2,
        Chat = 3,
        DoNotDisturb = 4,
        ExtendedAway = 5,
        None = 6
    }
    public class UserPresenceClass
    {
        public UserPresence userPresence { get; set; }
    }
}