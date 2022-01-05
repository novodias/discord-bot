namespace DiscordBot
{
    public class RandomClass
    {
        public TimeSpan Today {get; set;}
        public TimeSpan Date {get; set;}
        public ulong UserId {get; set;}
        public RandomClass(TimeSpan today, TimeSpan date, ulong userid = 0)
        {
            this.Today = today;
            this.Date = date;
            this.UserId = userid;
        }

    }
}