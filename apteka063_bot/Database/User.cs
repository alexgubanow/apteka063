namespace apteka063.dbc;

public class User : Telegram.Bot.Types.User
{
    public long ContactId { get; set; }
    public long LocationId { get; set; }
    public OrderStatus Status { get; set; }
    public string State { get; set; }
    //DO NOT REMOVE DEFAULT CTOR
    public User()
    { }
    public User(Telegram.Bot.Types.User tgUser)
    {
        Id = tgUser.Id;
        IsBot = tgUser.IsBot;
        FirstName = tgUser.FirstName;
        LastName = tgUser.LastName;
        Username = tgUser.Username;
        LanguageCode = tgUser.LanguageCode;
        State = "";
    }
    public static async Task<User> GetUserAsync(Apteka063Context db, Telegram.Bot.Types.User tgUser)
    {
        var user = await db.Users!.FindAsync(tgUser?.Id);
        if (user == null)
        {
            user = new(tgUser!);
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
        }
        return user;
    }
}


