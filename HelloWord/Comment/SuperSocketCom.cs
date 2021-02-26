using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWord.Comment
{
    //public class SuperSocketCom : AppServer<ChatSession>
    //{
    //    protected override bool Setup(IRootConfig rootConfig, IServerConfig config)
    //    {
    //        Console.WriteLine("准备读取配置文件。。。。");
    //        return base.Setup(rootConfig, config);
    //    }

    //    protected override void OnStarted()
    //    {
    //        Console.WriteLine("Chat服务启动。。。");
    //        base.OnStarted();
    //    }

    //    protected override void OnStopped()
    //    {
    //        Console.WriteLine("Chat服务停止。。。");
    //        base.OnStopped();
    //    }

    //    /// <summary>
    //    /// 新的连接
    //    /// </summary>
    //    /// <param name="session"></param>
    //    protected override void OnNewSessionConnected(ChatSession session)
    //    {
    //        Console.WriteLine($"Chat服务新加入的连接:{session.LocalEndPoint.Address.ToString()}");
    //        base.OnNewSessionConnected(session);
    //    }
    //}

    ///// <summary>
    ///// 表示用户连接
    ///// </summary>
    ////[AuthorisizeFilter]
    //public class ChatSession : AppSession<ChatSession>
    //{
    //    public string Id { get; set; }

    //    public string PassWord { get; set; }

    //    public bool IsLogin { get; set; }

    //    public DateTime LoginTime { get; set; }

    //    public DateTime LastHbTime { get; set; }

    //    public bool IsOnline
    //    {
    //        get
    //        {
    //            return this.LastHbTime.AddSeconds(10) > DateTime.Now;
    //        }
    //    }

    //    /// <summary>
    //    /// 消息发送
    //    /// </summary>
    //    /// <param name="message"></param>
    //    public override void Send(string message)
    //    {
    //        Console.WriteLine($"准备发送给{this.Id}：{message}");
    //        base.Send(message);
    //    }

    //    protected override void OnSessionStarted()
    //    {
    //        this.Send("Welcome to SuperSocket Chat Server");
    //    }

    //    protected override void OnInit()
    //    {
    //        this.Charset = Encoding.GetEncoding("gb2312");
    //        base.OnInit();
    //    }

    //    protected override void HandleUnknownRequest(StringRequestInfo requestInfo)
    //    {
    //        Console.WriteLine("收到命令:" + requestInfo.Key.ToString());
    //        this.Send("不知道如何处理 " + requestInfo.Key.ToString() + " 命令");
    //    }

    //    /// <summary>
    //    /// 异常捕捉
    //    /// </summary>
    //    /// <param name="e"></param>
    //    protected override void HandleException(Exception e)
    //    {
    //        this.Send($"\n\r异常信息：{ e.Message}");
    //        //base.HandleException(e);
    //    }

    //    /// <summary>
    //    /// 连接关闭
    //    /// </summary>
    //    /// <param name="reason"></param>
    //    protected override void OnSessionClosed(CloseReason reason)
    //    {
    //        Console.WriteLine("链接已关闭。。。");
    //        base.OnSessionClosed(reason);
    //    }
    //}


}
