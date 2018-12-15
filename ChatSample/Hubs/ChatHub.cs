using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatSample.Hubs
{
    public class ChatHub : Hub
    {
        private  IUserIdProvider _iUserIdProvider;

        public ChatHub(IUserIdProvider userIdProvider)
        {
            _iUserIdProvider = userIdProvider;
        }
        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            // _iUserIdProvider.GetUserId(Context)
           // Clients.All.SendMessage
            Clients.Others.SendAsync("broadcastMessage", name, message);
           
        }
        //发送回调用方
        public Task SendMessageToCaller(string message)
        {
            return Clients.Caller.SendAsync("ReceiveMessage", message+"1");
        }
        //将消息发送到中的所有客户端SignalR Users组。
        public Task SendMessageToGroup(string message)
        {
            return Clients.Group("SignalR Users").SendAsync("ReceiveMessage", message);
        }
        public void GetDate(string name,string message)
        {
            //  return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Clients.Others.SendAsync("GetDate", DateTime.Now, "");
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var id = httpContext.TraceIdentifier;//0HLJ2ANRC37CH:00000003
            await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");//添加到组
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SignalR Users");
            await base.OnDisconnectedAsync(exception);
        }
    }
    public class CustomUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}