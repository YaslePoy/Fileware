using FilewareApi.Models;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace FilewareApi;

[SignalRHub]
public class ChangesHub : Hub
{
    // public async Task AttachToFilespace(string fs)
    // {
    //     await Groups.AddToGroupAsync(this.Context.ConnectionId, fs);
    // }
    //
    // public async Task NotifyFileUpdate(int fileId)
    // {
    //     await Clients.Others.SendCoreAsync("NotifyFileUpdate", [fileId]);
    // }
    //
    // public async Task NotifyFileDelete(int fileId)
    // {
    //     await Clients.Others.SendCoreAsync("NotifyFileDelete", [fileId]);
    // }
    //
    // public async Task NotifyFileCreate(int fileId, string filespace)
    // {
    //     await Clients.OthersInGroup(filespace).SendCoreAsync("NotifyFileCreate", [fileId]);
    // }
    //
    // public async Task NotifyMessageUpdate(int messageId)
    // {
    //     await Clients.Others.SendCoreAsync("NotifyFileUpdate", [messageId]);
    // }
    //
    // public async Task NotifyMessageDelete(int messageId)
    // {
    //     await Clients.Others.SendCoreAsync("NotifyFileDelete", [messageId]);
    // }
    //
    // public async Task NotifyMessageCreate(int messageId, string filespace)
    // {
    //     await Clients.OthersInGroup(filespace).SendCoreAsync("NotifyFileCreate", [messageId]);
    // }
    
}