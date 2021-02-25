using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyApm.Utilities.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using HelloWord.Comment;
using System.Text;

namespace HelloWord
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSkyApmExtensions();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            };
            //webSocketOptions.AllowedOrigins.Add("https://client.com");
            //webSocketOptions.AllowedOrigins.Add("https://www.client.com");

            app.UseWebSockets(webSocketOptions);
            //app.UseMiddleware<Controllers.ChatWebSocketMiddleware>();

            //Controllers.ChatWebSocketMiddleware chatWebSocket = new Controllers.ChatWebSocketMiddleware();
            //chatWebSocket. Invoke(HttpContext context)

            //app.Use(async (context, next) =>
            //{
            //    if (context.Request.Path == "/ws")
            //    {
            //        if (context.WebSockets.IsWebSocketRequest)
            //        {
            //            using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
            //            {
            //                await Echo(context, webSocket);
            //            }
            //        }
            //        else
            //        {
            //            context.Response.StatusCode = 400;
            //        }
            //    }
            //    else
            //    {
            //        await next();
            //    }

            //});

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        #region 创建socket连接                      
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        Guid userId = Guid.NewGuid();
                        var msg = JsonConvert.SerializeObject(new MessageModel
                        {
                            DataType = DataType.Json,
                            SendType = SendType.SystemMsg,
                            SenderName = "Server",
                            Data = new { Id = Guid.NewGuid(), UserName = "NolenJ", UserId = userId.ToString() }
                        });
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(msg);
                        //连接成功，发送反馈，这里用了自定义的数据模板，前端根据消息类型做处理。
                        //WebSocket创建连接在浏览器network可以看到请求，但是不像ajax请求，给不了返回值。
                        await webSocket.SendAsync(new ArraySegment<byte>(byteArray), WebSocketMessageType.Text, true, CancellationToken.None);
                        //保存到socket容器
                        WebSocketHelper.Root.UserList.Add(new WebSocketModel
                        {
                            Id = userId,
                            Sk = webSocket,
                            UserId = userId.ToString()
                        });
                        #endregion
                        //连接成功后，丢给自定义收发数据的管理工具
                        await Echo(webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });



            //ImHelper.Initialization(new ImServerOptions
            //{
            //    Redis = new FreeRedis.RedisClient("192.168.192.128:6379,poolsize=10"),
            //    Servers = new[] { "127.0.0.1:6001" },
            //    Server = "127.0.0.1:6001"
            //});

            //ImHelper.Instance.OnSend += (s, e) =>
            //{
            //    //LogBuilder.CreateInstance().Info($"ImClient.SendMessage(server={e.Server},data={JsonConvert.SerializeObject(e.Message)})");
            //    Console.WriteLine($"ImClient.SendMessage(server={e.Server},data={JsonConvert.SerializeObject(e.Message)})");
            //};

            //ImHelper.EventBus(
            //    t =>
            //    {
            //        //LogBuilder.CreateInstance().Info("IMCore :" + t.clientId + "上线了");
            //        Console.WriteLine(t.clientId + "上线了");
            //        var onlineUids = ImHelper.GetClientListByOnline();
            //        ImHelper.SendMessage(t.clientId, onlineUids, $"用户{t.clientId}上线了");
            //    },
            //    t =>
            //    {
            //        //LogBuilder.CreateInstance().Info("IMCore :" + t.clientId + "下线了");
            //        Console.WriteLine(t.clientId + "下线了");
            //    });

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});

            //string serverName = Configuration.GetSection("Consul:ServiceName").Value;
            //string clientUrl = Configuration.GetSection("Consul:ConsulAddress").Value;
            //string agentIP = Configuration.GetSection("Consul:ServiceIP").Value;
            //string port = Configuration.GetSection("Consul:ServicePort").Value;
            //string heartbeatUrl = Configuration.GetSection("Consul:ServiceHealthCheck").Value;
            //string tags = Configuration.GetSection("Consul:Tags").Value;
            //var agentTags = new string[] { };
            //if (!string.IsNullOrEmpty(tags))
            //{
            //    agentTags = tags.Split(',');
            //}

            //ConsulClient client = new ConsulClient(obj =>
            //{
            //    obj.Address = new Uri(clientUrl);
            //    obj.Datacenter = serverName;
            //});

            //client.Agent.ServiceRegister(new AgentServiceRegistration()
            //{
            //    ID = $"{serverName}_{Guid.NewGuid()}",
            //    Name = serverName,
            //    Address = agentIP,
            //    Port = Convert.ToInt32(port),
            //    Tags = agentTags,
            //    Check = new AgentServiceCheck()
            //    {
            //        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
            //        Interval = TimeSpan.FromSeconds(15),
            //        HTTP = heartbeatUrl,
            //        Timeout = TimeSpan.FromSeconds(5)
            //    }
            //}).Wait();
            app.UseFileServer();
        }

        /// <summary>
        /// 接收服务
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!webSocket.CloseStatus.HasValue)//连接中
            {
                //拆包
                var msg = JsonConvert.DeserializeObject<MessageModel>(Encoding.Default.GetString(buffer));
                if (WebSocketHelper.Root.UserList.Any(c => c.Id == msg.TargetId))
                {
                    switch (msg.SendType)
                    {
                        case SendType.Broadcast:
                            List<Task> tasks = new List<Task>();
                            WebSocketHelper.Root.UserList.ForEach(c =>
                            {
                                tasks.Add(c.Sk.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None));
                            });
                            Task.WaitAll(tasks.ToArray());
                            break;
                        case SendType.Unicast:
                            await SendMsg(webSocket, msg, result, buffer);
                            break;
                    }
                }
                //重置消息容器
                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            WebSocketHelper.Root.UserList.Remove(WebSocketHelper.Root.UserList.Where(c => c.Sk == webSocket).FirstOrDefault());
            await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="webSocket"></param>
        /// <param name="msg"></param>
        /// <param name="result"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private async Task<bool> SendMsg(WebSocket webSocket, MessageModel msg, WebSocketReceiveResult result, byte[] buffer)
        {
            var actionResult = false;
            var target = WebSocketHelper.Root.UserList.Where(c => c.Id == msg.TargetId).FirstOrDefault();
            if (target != null)
            {
                try
                {
                    await Task.Factory.ContinueWhenAll(new Task[2]{
                             target.Sk.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None),//给对方发消息表示
                             webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None)//给自己发消息表示我的消息已送达
                         },
                         (m) =>
                         {
                             Console.WriteLine("发送成功");
                         });
                    actionResult = true;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return actionResult;
        }
    }
}
