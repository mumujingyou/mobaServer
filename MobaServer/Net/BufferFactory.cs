using Google.Protobuf;
using ProtoMsg;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobaServer.Net
{
   public class BufferFactory
    {
        enum MessageType
        {
            ACK = 0,//确认报文
            Login = 1,//业务逻辑的报文
        }

        /// <summary>
        /// 创建并且发送报文
        /// </summary>
        /// <param name="uClient"></param>
        /// <param name="messageID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static BufferEntity CreqateAndSendPackage(UClient uClient,int messageID,IMessage message) {
            //如果处于连接状态
            if (uClient.isConnect)
            {
                //打印protobuf 按json格式
                Debug.Log(messageID, message);

                BufferEntity bufferEntity = new BufferEntity(uClient.endPoint,uClient.session,0,0, MessageType.Login.GetHashCode(),
                    messageID,ProtobufHelper.ToBytes(message));

                uClient.Send(bufferEntity);
                return bufferEntity;
            }
            return null;
        }

        internal static BufferEntity CreqateAndSendPackage(BufferEntity request, IMessage message)
        {
            Debug.Log("会话ID:"+request.session);
            UClient client = GameManager.uSocket.GetClient(request.session);
            return CreqateAndSendPackage(client, request.messageID, message);
        }
    }
}
