using FilewareApi.Models;

namespace FilewareApi.Services.MessagingService;

public interface IMessagingService
{
    int PostMessage(string text, string fileSpace);
    void DeleteMessage(int id);
    void UpdateMessage(int id, string text);
    Message? GetMessage(int id);
}