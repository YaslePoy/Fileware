using FilewareApi.Models;

namespace FilewareApi.Services.MessagingService;

public class MessagingService(FilewareDbContext dbContext) : IMessagingService
{
    public int PostMessage(string text)
    {
        var msg = new Message { Text = text, Time = FileManagerService.FileManagerService.NowWithoutTimezone };
        dbContext.Messages.Add(msg);
        dbContext.SaveChanges();

        dbContext.HistoryPoints.Add(new HistoryPoint
        {
            Time = FileManagerService.FileManagerService.NowWithoutTimezone, Type = (int)HistoryPointType.Message,
            LinkedId = msg.Id
        });
        dbContext.SaveChanges();

        return msg.Id;
    }

    public void DeleteMassage(int id)
    {
        var msg = FindMessage(id);
        if (msg is null)
            return;

        dbContext.Messages.Remove(msg);
        dbContext.SaveChanges();
    }

    public void UpdateMessage(int id, string text)
    {
        var msg = FindMessage(id);
        if (msg is null)
            return;

        msg.Text = text;
        dbContext.SaveChanges();
    }

    public Message? GetMessage(int id)
    {
        return FindMessage(id);
    }

    private Message? FindMessage(int id) => dbContext.Messages.FirstOrDefault(i => i.Id == id);
}