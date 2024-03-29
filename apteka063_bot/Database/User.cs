﻿namespace apteka063.Database;

public class User : Telegram.Bot.Types.User
{
    public long ContactId { get; set; }
    public long LocationId { get; set; }
    public OrderStatus Status { get; set; }
    public int LastMessageSentId { get; set; }
    public string PhoneNumber { get; set; } = "";
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
    }
}


