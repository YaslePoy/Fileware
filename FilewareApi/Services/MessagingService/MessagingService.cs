using FilewareApi.Models;

namespace FilewareApi.Services.MessagingService;

public class MessagingService(FilewareDbContext dbContext, ChangesHub hub) : IMessagingService
{
    public int PostMessage(string text, string fileSpace)
    {
        var msg = new Message { Text = text, Time = FileManagerService.FileManagerService.NowWithoutTimezone };
        dbContext.Messages.Add(msg);
        dbContext.SaveChanges();

        dbContext.HistoryPoints.Add(new HistoryPoint
        {
            Time = FileManagerService.FileManagerService.NowWithoutTimezone, Type = (int)HistoryPointType.Message,
            LinkedId = msg.Id, FileSpaceKey = fileSpace
        });
        dbContext.SaveChanges();

        // _ = hub.NotifyMessageCreate(msg.Id, fileSpace);
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
        // _ = hub.NotifyMessageDelete(id);
    }

    public void UpdateMessage(int id, string text)
    {
        var msg = FindMessage(id);
        if (msg is null)
            return;

        msg.Text = text;
        dbContext.SaveChanges();
        // _ = hub.NotifyMessageUpdate(id);
    }

    public Message? GetMessage(int id)
    {
        return FindMessage(id);
    }

    private Message? FindMessage(int id) => dbContext.Messages.FirstOrDefault(i => i.Id == id);
}