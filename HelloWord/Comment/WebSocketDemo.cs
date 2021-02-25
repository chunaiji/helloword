using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace HelloWord.Comment
{

    public enum SendType
    {
        SystemMsg,
        /// <summary>
        /// 单独发送
        /// </summary>
        Unicast,
        /// <summary>
        /// 
        /// </summary>
        Broadcast
    }
    public enum DataType
    {
        String,
        Json,
    }
    public class WebSocketModel
    {
        public WebSocket Sk { get; set; }
        public Guid Id { get; set; }
        public string UserId { get; set; }
    }
    public class ChatRoom
    {
        public List<WebSocketModel> UserList { get; set; }
        public Guid RoomId { get; set; }
    }
    public class MessageModel
    {
        public DataType DataType { get; set; }
        public SendType SendType { get; set; }
        public object Data { get; set; }
        public string SenderName { get; set; }
        public Guid SenderId { get; set; }
        public Guid TargetId { get; set; }
    }
    public static class WebSocketHelper
    {
        /// <summary>
        /// websocket容器，可以当作缓存使用
        /// </summary>
        public static ChatRoom Root = new ChatRoom
        {
            UserList = new List<WebSocketModel>(),
            RoomId = Guid.NewGuid(),
        };
    }
}
