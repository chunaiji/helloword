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
                        #region ����socket����                      
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
                        //���ӳɹ������ͷ��������������Զ��������ģ�壬ǰ�˸�����Ϣ����������
                        //WebSocket���������������network���Կ������󣬵��ǲ���ajax���󣬸����˷���ֵ��
                        await webSocket.SendAsync(new ArraySegment<byte>(byteArray), WebSocketMessageType.Text, true, CancellationToken.None);
                        //���浽socket����
                        WebSocketHelper.Root.UserList.Add(new WebSocketModel
                        {
                            Id = userId,
                            Sk = webSocket,
                            UserId = userId.ToString()
                        });
                        #endregion
                        //���ӳɹ��󣬶����Զ����շ����ݵĹ�����
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
            //        //LogBuilder.CreateInstance().Info("IMCore :" + t.clientId + "������");
            //        Console.WriteLine(t.clientId + "������");
            //        var onlineUids = ImHelper.GetClientListByOnline();
            //        ImHelper.SendMessage(t.clientId, onlineUids, $"�û�{t.clientId}������");
            //    },
            //    t =>
            //    {
            //        //LogBuilder.CreateInstance().Info("IMCore :" + t.clientId + "������");
            //        Console.WriteLine(t.clientId + "������");
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
        /// ���շ���
        /// </summary>
        /// <param name="webSocket"></param>
        /// <returns></returns>
        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!webSocket.CloseStatus.HasValue)//������
            {
                //���
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
                //������Ϣ����
                buffer = new byte[1024 * 4];
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            WebSocketHelper.Root.UserList.Remove(WebSocketHelper.Root.UserList.Where(c => c.Sk == webSocket).FirstOrDefault());
            await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);
        }

        /// <summary>
        /// ������Ϣ
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
                             target.Sk.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None),//���Է�����Ϣ��ʾ
                             webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None)//���Լ�����Ϣ��ʾ�ҵ���Ϣ���ʹ�
                         },
                         (m) =>
                         {
                             Console.WriteLine("���ͳɹ�");
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
