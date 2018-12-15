using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RbChat.Hubs
{
    public class ChatHub : Hub
    {

        private static Queue<string> Users = null;
        private IUserIdProvider _iUserIdProvider;

        static ChatHub()
        {
            List<string> list = new List<string> { "刘秀",
                "陈诚",
                "王旭",
                "张灵",
                "吴敬",
                "王海东"};
            Users = new Queue<string>(list);
        }
        public ChatHub(IUserIdProvider userIdProvider)
        {
            _iUserIdProvider = userIdProvider;
         
        }
        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            // _iUserIdProvider.GetUserId(Context)
            // Clients.All.SendMessage
            Clients.All.SendAsync("broadcastMessage", name, message);

        }
        //发送回调用方
        public Task SendMessageToCaller(string message)
        {
            return Clients.Caller.SendAsync("ReceiveMessage", message + "1");
        }
        //将消息发送到中的所有客户端SignalR Users组。
        public Task SendMessageToGroup(string message)
        {
            return Clients.Group("SignalR Users").SendAsync("ReceiveMessage", message);
        }
        public void GetDate(string name, string message)
        {
            //  return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Clients.Others.SendAsync("GetDate", DateTime.Now, "");
        }

        public async Task SenMsg(string message)
        {
            var data = new {Name=Context.ConnectionId, msg = message, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            var msgBody = Newtonsoft.Json.JsonConvert.SerializeObject(data);
           await  Clients.All.SendAsync("GetMsg", msgBody);
        }

        public override async Task OnConnectedAsync()
        {
           // var httpContext = Context.GetHttpContext();
          //  var id = httpContext.TraceIdentifier;//0HLJ2ANRC37CH:00000003
           await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");//添加到组
        //    await Clients.All.SendAsync("GetOnlineUsers", Newtonsoft.Json.JsonConvert.SerializeObject(new { Name = Users.Dequeue(), IsOnline = true }));
           // await base.OnConnectedAsync();
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
