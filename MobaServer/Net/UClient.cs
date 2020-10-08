using MobaServer.Player;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Net
{
   public class UClient
    {
        private USocket uSocket;
        public IPEndPoint endPoint;
        private int sendSN;//发送的序号
        private int handleSN;//处理的序号
        public int session;
        private Action<BufferEntity> dispatchNetEvent;

        public UClient(USocket uSocket, IPEndPoint endPoint, int sendSN, int handleSN, int session, Action<BufferEntity> dispatchNetEvent)
        {
            this.uSocket = uSocket;
            this.endPoint = endPoint;
            this.sendSN = sendSN;
            this.handleSN = handleSN;
            this.session = session;
            this.dispatchNetEvent = dispatchNetEvent;

            //超时检测
            CheckOutTime();
        }

        public bool isConnect = true;//是否处于连接的状态

        int overtime = 150;//超时的时间
        //对已发送的消息进行超时检测
        private async void CheckOutTime()
        {
            await Task.Delay(overtime);
            foreach (var package in sendPackage.Values)
            {
                if (package.recurCount >= 10)
                {
                    Debug.LogError($"重发十次还是失败!,协议ID:{package.messageID}");
                    uSocket.RemoveClient(session);
                    return;
                }

                if (TimeHelper.Now() - package.time >= (package.recurCount + 1) * overtime)
                {
                    //重发次数+1
                    package.recurCount += 1;
                    Debug.Log($"超时重发,序号是:{package.sn}");
                    uSocket.Send(package.buffer, endPoint);
                }
            }
            CheckOutTime();
        }

        internal void Handle(BufferEntity buffer)
        {
            //要移除掉已经发送的BufferEntity
            //int sn = buffer.sn;

            switch (buffer.messageType)
            {
                case 0://ACK确认报文
                    BufferEntity buff;
                    if (sendPackage.TryRemove(buffer.sn,out buff))
                    {
                        Debug.Log($"报文已确认,序号:{buffer.sn}");
                    }
                    else
                    {
                        Debug.Log($"要确认的报文不存在,序号:{buffer.sn}");
                    }
                    break;
                case 1://业务报文

                    //if (buffer.sn!=1)
                    //{
                    //    return;//超时重发的测试代码 
                    //}
                    BufferEntity ackPackage = new BufferEntity(buffer);
                    uSocket.SendACK(ackPackage, endPoint);

                    Debug.Log("收到的是业务报文!");
                    //再进行处理业务报文
                    HandleLogincPackage(buffer);
                    break;
                default:
                    break;
            }

        }

        

        ConcurrentDictionary<int, BufferEntity> sendPackage = new ConcurrentDictionary<int, BufferEntity>();

        //发送的接口
        public void Send(BufferEntity package)
        {
            if (isConnect ==false)
            {
                return;
            }

            package.time = TimeHelper.Now();
            sendSN += 1;//发送序号+1
            package.sn = sendSN;

            //序列化
            package.Encoder(false);
            uSocket.Send(package.buffer, endPoint);
            if (session!=0)
            {
                //已经发送的数据
                sendPackage.TryAdd(package.sn, package);
            }

        }

        //存储错序的报文 等待后面处理
        ConcurrentDictionary<int, BufferEntity> waitHandle = new ConcurrentDictionary<int, BufferEntity>();

        //处理业务逻辑的接口
        private void HandleLogincPackage(BufferEntity buffer)
        {
            if (buffer.sn<=handleSN)
            {
                Debug.Log($"已经处理过的消息了,序号:{buffer.sn}");
                return;
            }

            if (buffer.sn-handleSN>1)
            {
                if (waitHandle.TryAdd(buffer.sn, buffer))
                {
                    Debug.Log($"错序的报文,进行缓存,序号是:{buffer.sn}");
                }
                return;
            }

            handleSN = buffer.sn;
            if (dispatchNetEvent!=null)
            {
                Debug.Log("分发消息给游戏模块...");
                dispatchNetEvent(buffer);
            }
            BufferEntity nextBuffer;
            if (waitHandle.TryRemove(handleSN+1,out nextBuffer))
            {
                HandleLogincPackage(nextBuffer);
            }
        }

        internal void Close()
        {
            isConnect = false;
            //客户端断开的时候 清理掉缓存 避免游戏模块在获取的时候 获取到错误的数据
            if (PlayerManager.GetPlayerEntityFromSession(session)!=null)
            {
                int rolesID = PlayerManager.GetPlayerEntityFromSession(session).rolesInfo.RolesID;
                PlayerManager.RemoveFromRolesID(rolesID);
            }
            PlayerManager.RemoveFromSession(session);
        }


    }
}
