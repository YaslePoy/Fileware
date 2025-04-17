using FilewareApi.Models;

namespace FilewareApi.Services.MessagingService;

public class MessagingService(FilewareDbContext dbContext) : IMessagingService
{
    public int PostMessage(string text, string filespace)
    {
        var msg = new Message { Text = text, Time = FileManagerService.FileManagerService.NowWithoutTimezone };
        dbContext.Messages.Add(msg);
        dbContext.SaveChanges();

        dbContext.HistoryPoints.Add(new HistoryPoint
        {
            Time = FileManagerService.FileManagerService.NowWithoutTimezone, Type = (int)HistoryPointType.Message,
            LinkedId = msg.Id, FileSpaceKey = filespace
        });
        dbContext.SaveChanges();

        return msg.Id;
    }

    public void DeleteMessage(int id)
    {
        var msg = FindMessage(id);
        if (msg is null)
            return;

        dbContext.Messages.Remove(msg);
        dbContext.HistoryPoints.Remove(dbContext.HistoryPoints.FirstOrDefault(i => i.LinkedId == msg.Id));

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