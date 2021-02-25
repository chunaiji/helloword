//using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net.WebSockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace HelloWord.Controllers
//{

//    public class CustomWebSocket
//    {
//        public string ConId { get; set; }
//        public WebSocket webSocket { get; set; }
//    }

//    public interface ICustomWebSocketFactory
//    {
//        void Add(CustomWebSocket uws);
//        void Remove(string conId);
//        List<CustomWebSocket> All();
//        List<CustomWebSocket> Others(CustomWebSocket client);
//        CustomWebSocket Client(string conId);
//    }

//    public interface ICustomWebSocketMessageHandler
//    {
//        Task SendInitialMessages(CustomWebSocket userWebSocket);
//        Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory);
//        Task SendMessageInfo(string conId, object data, ICustomWebSocketFactory wsFactory);
//    }



//    public class WebSocketFanoutDto
//    {
//        public string conId { get; set; }

//        public CustomWebSocketMessage data { get; set; }
//    }

//    public interface IMessageConsume
//    {

//    }

//    public class CustomWebSocketMessage
//    {
//        public object DataInfo { get; set; }
//        public WSMessageType Type { get; set; }
//        public DateTime MessagDateTime { get; set; }
//    }

//    public class LoginInfoDto
//    {
//        public int? tenantId { get; set; }
//        public int userId { get; set; }

//        public int? bankId { get; set; }
//        public string fromeType { get; set; }
//    }

//    public enum WSMessageType
//    {
//        任务数量,
//        连接响应,
//        用户信息
//    }

//    public enum SendEnum {
//        订阅模式
//    }

//    public class PushMsg {
//        public WebSocketFanoutDto sendjsonMsg { get; set; }
//        public string exchangeName { get; set; }
//        public SendEnum sendEnum { get; set; }
//    }

//    public class FanoutMesConsume : IMessageConsume
//    {
//        public void Consume(string message)
//        {
//            var condto = JsonConvert.DeserializeObject<WebSocketFanoutDto>(message);
//            var wsFactory = IOCManage.ServiceProvider.GetService<ICustomWebSocketFactory>();
//            var uws = wsFactory.Client(condto.conId);
//            if (uws != null)
//            {
//                //发送消息
//                var mesbuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(condto.data));
//                var mescount = Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(condto.data));
//                uws.WebSocket.SendAsync(new ArraySegment<byte>(mesbuffer, 0, mescount), WebSocketMessageType.Text, true, CancellationToken.None);
//            }
//        }
//    }


//    public class CustomWebSocketMessageHandler : ICustomWebSocketMessageHandler
//    {
//        public async Task SendInitialMessages(CustomWebSocket userWebSocket)
//        {
//            WebSocket webSocket = userWebSocket.webSocket;
//            var msg = new CustomWebSocketMessage
//            {
//                MessagDateTime = DateTime.Now,
//                Type = WSMessageType.连接响应
//            };

//            string serialisedMessage = JsonConvert.SerializeObject(msg);
//            byte[] bytes = Encoding.ASCII.GetBytes(serialisedMessage);
//            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
//        }
//        /// <summary>
//        /// 推送消息到客户端
//        /// </summary>
//        /// <returns></returns>
//        public async Task SendMessageInfo(string conId, object data, ICustomWebSocketFactory wsFactory)
//        {
//            var uws = wsFactory.Client(conId);
//            CustomWebSocketMessage message = new CustomWebSocketMessage();
//            message.DataInfo = data;
//            message.Type = WSMessageType.任务数量;
//            message.MessagDateTime = DateTime.Now;
//            if (uws == null)
//            {
//                //广播到其他集群节点
//                var listpush = new List<PushMsg>();

//                var push = new PushMsg()
//                {
//                    sendjsonMsg = new WebSocketFanoutDto()
//                    {
//                        conId = conId,
//                        data = message
//                    },
//                    exchangeName = "saas.reltimewsmes.exchange",
//                    sendEnum = SendEnum.订阅模式
//                };
//                listpush.Add(push);
//                BTRabbitMQManage.PushMessageAsync(listpush);
//                return;
//            }

//            var mesbuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
//            var mescount = Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(message));
//            await uws.webSocket.SendAsync(new ArraySegment<byte>(mesbuffer, 0, mescount), WebSocketMessageType.Text, true, CancellationToken.None);
//        }

//        /// <summary>
//        /// 处理接收到的客户端信息
//        /// </summary>
//        /// <param name="result"></param>
//        /// <param name="buffer"></param>
//        /// <param name="userWebSocket"></param>
//        /// <param name="wsFactory"></param>
//        /// <returns></returns>
//        public async Task HandleMessage(WebSocketReceiveResult result, byte[] buffer, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory)
//        {
//            string msg = Encoding.UTF8.GetString(buffer);
//            try
//            {
//                var message = JsonConvert.DeserializeObject<CustomWebSocketMessage>(msg);
//                if (message.Type == WSMessageType.用户信息)
//                {
//                    var logdto = JsonConvert.DeserializeObject<LoginInfoDto>(message.DataInfo.ToJsonString());
//                    await InitUserInfo(logdto, userWebSocket, wsFactory);
//                }

//            }
//            catch (Exception e)
//            {
//                var exbuffer = Encoding.UTF8.GetBytes(e.Message);
//                var excount = Encoding.UTF8.GetByteCount(e.Message);
//                await userWebSocket.webSocket.SendAsync(new ArraySegment<byte>(exbuffer, 0, excount), result.MessageType, result.EndOfMessage, CancellationToken.None);
//            }
//        }
//        /// <summary>
//        /// 初始化用户连接关系
//        /// </summary>
//        /// <param name="dto"></param>
//        /// <param name="userWebSocket"></param>
//        /// <param name="wsFactory"></param>
//        /// <returns></returns>
//        private async Task InitUserInfo(LoginInfoDto dto, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory)
//        {
//            if (dto.userId == 0)
//                return;
//            var contectid = userWebSocket.ConId;
//            var key = "";
//            if (dto.tenantId.HasValue)
//                key += "T_" + dto.userId + "_" + dto.tenantId + "_" + "tenant_";
//            if (dto.bankId.HasValue)
//                key += "B_" + dto.userId + "_" + dto.bankId + "_" + "bank_";
//            key += dto.fromeType;
//            //添加缓存
//            CacheInstace<string>.GetRedisInstanceDefaultMemery().AddOrUpdate(key, contectid, r =>
//            {
//                r = contectid;
//                return r;
//            });
//            CacheInstace<string>.GetRedisInstanceDefaultMemery().Expire(key, new TimeSpan(12, 0, 0));

//        }

//    }


//    public class CustomWebSocketFactory : ICustomWebSocketFactory
//    {
//        List<CustomWebSocket> List;
//        public CustomWebSocketFactory()
//        {
//            List = new List<CustomWebSocket>();
//        }
//        public void Add(CustomWebSocket uws)
//        {
//            List.Add(uws);
//        }
//        public void Remove(string conId)
//        {
//            List.Remove(Client(conId));

//        }
//        public List<CustomWebSocket> All()
//        {
//            return List;
//        }

//        public List<CustomWebSocket> Others(CustomWebSocket client)
//        {
//            return List.Where(c => c.ConId != client.ConId).ToList();
//        }
//        public CustomWebSocket Client(string conId)
//        {
//            var uws = List.FirstOrDefault(c => c.ConId == conId);
//            return uws;

//        }
//    }


//    public class CustomWebSocketMiddlewar
//    {
//        private readonly RequestDelegate _next;

//        public CustomWebSocketMiddlewar(RequestDelegate next)
//        {
//            _next = next;
//        }

//        public async Task Invoke(HttpContext context, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
//        {
//            if (context.WebSockets.IsWebSocketRequest)
//            {
//                string ConId = context.Request.Query["sign"];
//                if (!string.IsNullOrEmpty(ConId))
//                {
//                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
//                    CustomWebSocket userWebSocket = new CustomWebSocket()
//                    {
//                        webSocket = webSocket,
//                        ConId = ConId
//                    };
//                    wsFactory.Add(userWebSocket);
//                    //await wsmHandler.SendInitialMessages(userWebSocket);
//                    await Listen(context, userWebSocket, wsFactory, wsmHandler);
//                }
//            }
//            else
//            {
//                context.Response.StatusCode = 400;
//            }

//            await _next(context);
//        }
//        //监听客户端发送过来的消息
//        private async Task Listen(HttpContext context, CustomWebSocket userWebSocket, ICustomWebSocketFactory wsFactory, ICustomWebSocketMessageHandler wsmHandler)
//        {
//            WebSocket webSocket = userWebSocket.webSocket;
//            var buffer = new byte[1024 * 4];
//            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//            while (!result.CloseStatus.HasValue)
//            {
//                await wsmHandler.HandleMessage(result, buffer, userWebSocket, wsFactory);
//                buffer = new byte[1024 * 4];
//                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//            }
//            wsFactory.Remove(userWebSocket.ConId);
//            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
//        }
//    }

//    //public class MsgTemplate
//    //{
//    //    public string SenderID { get; set; }
//    //    public string ReceiverID { get; set; }
//    //    public string MessageType { get; set; }
//    //    public string Content { get; set; }
//    //}
//    //public class ChatWebSocketMiddleware
//    //{
//    //    private static ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _sockets = new ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

//    //    private readonly RequestDelegate _next;

//    //    public ChatWebSocketMiddleware(RequestDelegate next)
//    //    {
//    //        _next = next;
//    //    }

//    //    public async Task Invoke(HttpContext context)
//    //    {
//    //        if (!context.WebSockets.IsWebSocketRequest)
//    //        {
//    //            await _next.Invoke(context);
//    //            return;
//    //        }
//    //        WebSocket dummy;

//    //        CancellationToken ct = context.RequestAborted;
//    //        var currentSocket = await context.WebSockets.AcceptWebSocketAsync();
//    //        //string socketId = Guid.NewGuid().ToString();
//    //        string socketId = context.Request.Query["sid"].ToString();
//    //        if (!_sockets.ContainsKey(socketId))
//    //        {
//    //            _sockets.TryAdd(socketId, currentSocket);
//    //        }
//    //        //_sockets.TryRemove(socketId, out dummy);
//    //        //_sockets.TryAdd(socketId, currentSocket);

//    //        while (true)
//    //        {
//    //            if (ct.IsCancellationRequested)
//    //            {
//    //                break;
//    //            }

//    //            string response = await ReceiveStringAsync(currentSocket, ct);
//    //            MsgTemplate msg = JsonConvert.DeserializeObject<MsgTemplate>(response);

//    //            if (string.IsNullOrEmpty(response))
//    //            {
//    //                if (currentSocket.State != WebSocketState.Open)
//    //                {
//    //                    break;
//    //                }

//    //                continue;
//    //            }

//    //            foreach (var socket in _sockets)
//    //            {
//    //                if (socket.Value.State != WebSocketState.Open)
//    //                {
//    //                    continue;
//    //                }
//    //                if (socket.Key == msg.ReceiverID || socket.Key == socketId)
//    //                {
//    //                    await SendStringAsync(socket.Value, JsonConvert.SerializeObject(msg), ct);
//    //                }
//    //            }
//    //        }

//    //        //_sockets.TryRemove(socketId, out dummy);

//    //        await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
//    //        currentSocket.Dispose();
//    //    }

//    //    private static Task SendStringAsync(System.Net.WebSockets.WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
//    //    {
//    //        var buffer = Encoding.UTF8.GetBytes(data);
//    //        var segment = new ArraySegment<byte>(buffer);
//    //        return socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
//    //    }

//    //    private static async Task<string> ReceiveStringAsync(System.Net.WebSockets.WebSocket socket, CancellationToken ct = default(CancellationToken))
//    //    {
//    //        var buffer = new ArraySegment<byte>(new byte[8192]);
//    //        using (var ms = new MemoryStream())
//    //        {
//    //            WebSocketReceiveResult result;
//    //            do
//    //            {
//    //                ct.ThrowIfCancellationRequested();

//    //                result = await socket.ReceiveAsync(buffer, ct);
//    //                ms.Write(buffer.ToArray(), buffer.Offset, result.Count);
//    //            }
//    //            while (!result.EndOfMessage);

//    //            ms.Seek(0, SeekOrigin.Begin);
//    //            if (result.MessageType != WebSocketMessageType.Text)
//    //            {
//    //                return null;
//    //            }

//    //            using (var reader = new StreamReader(ms, Encoding.UTF8))
//    //            {
//    //                return await reader.ReadToEndAsync();
//    //            }
//    //        }
//    //    }
//    //}
//}
