using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MobaServer.Net
{
   public class USocket
    {
        UdpClient socket;//socket通信

        string ip = "127.0.0.1";
        int port = 8899;

        Action<BufferEntity> dispatchNetEvent;
        //初始化的接口
        public USocket(Action<BufferEntity> dispatchNetEvent) {
            this.dispatchNetEvent = dispatchNetEvent;
            socket = new UdpClient(port);
            Receive();

            Task.Run(Handle,ct.Token);
        }

        //发送消息的接口
        public async void Send(byte[] data,IPEndPoint endPoint) {
            if (socket!=null)
            {
                try
                {
                    int length = await socket.SendAsync(data, data.Length, endPoint);
                    if (data.Length == length)
                    {
                        //完整的发送
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"发送异常:{e.Message}");
                    Close();
                }
            }
        
        }

        //发送ACK消息的接口
        public void SendACK(BufferEntity ackPackage,IPEndPoint endPoint) {
            Debug.Log("回复客户端收到消息了!");
            Send(ackPackage.buffer, endPoint);
        }


        ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();

        //接收消息的接口
        public async void Receive() {
            if (socket!=null)
            {
                try
                {
                    UdpReceiveResult result=  await socket.ReceiveAsync();
                    Debug.Log("接收到客户端的消息!");
                   awaitHandle.Enqueue(result);
                    Receive();
                }
                catch (Exception e)
                {

                    Debug.LogError($"接收异常:{e.Message}");
                    Close();
                }
            }
            
        }

        CancellationTokenSource ct = new CancellationTokenSource();
        int sessionID = 1000;
        //处理消息的接口
        async Task Handle() {
            while (!ct.IsCancellationRequested)
            {
                if (awaitHandle.Count>0)
                {
                    UdpReceiveResult data;
                    if (awaitHandle.TryDequeue(out data))
                    {
                        BufferEntity bufferEntity = new BufferEntity(data.RemoteEndPoint, data.Buffer);
                        if (bufferEntity.isFull)
                        {
                            //会话ID来进行查询 
                            if (bufferEntity.session == 0)
                            {
                                //客户端 还未建立连接 分配会话ID
                                sessionID += 1;
                                //创建客户端 给这个客户端分配这个会话ID
                                bufferEntity.session = sessionID;
                                CreateUClient(bufferEntity);
                                Debug.Log($"创建客户端,会话ID是{sessionID}");
                            }

                            UClient targetClient;
                            //获取到客户端
                            if (clients.TryGetValue(bufferEntity.session, out targetClient))
                            {
                                targetClient.Handle(bufferEntity);
                            }
                        }
                    }
                }
            }
        }

        //关闭Socket的接口
        void Close() {
            //取消任务的信号
            ct.Cancel();

            //所有的客户端都清理掉 
            foreach (var client in clients.Values)
            {
                client.Close();
            }
            clients.Clear();

            if (socket!=null)
            {
                socket.Close();
                socket = null;
            }

            if (dispatchNetEvent!=null)
            {
                dispatchNetEvent = null;
            }
        }

        ConcurrentDictionary<int, UClient> clients = new ConcurrentDictionary<int, UClient>();

        //创建客户端建立虚拟连接的接口
        void CreateUClient(BufferEntity buffer)
        {
            UClient client;
            if (!clients.TryGetValue(buffer.session,out client))
            {
                client = new UClient(this,buffer.endPoint,0,0,buffer.session, dispatchNetEvent);
                clients.TryAdd(buffer.session, client);
            }
        }


        //移除掉客户端的接口
        public void RemoveClient(int sessionid) {
            UClient client;
            if (clients.TryRemove(sessionid,out client))
            {
                client.Close();
                client = null;
            }
        
        }

        //查询客户端的接口
        public UClient GetClient(int sessionid) {

            UClient client;
            if (clients.TryGetValue(sessionid, out client))
            {
                return client;
            }
            return null;
        }


    }
}
