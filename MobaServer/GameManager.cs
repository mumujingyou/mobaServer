using MobaServer.GameModule;
using MobaServer.Net;
using System;

namespace MobaServer
{
    class GameManager
    {
        static void Main(string[] args)
        {
            Console.WriteLine("启动服务器... ...");


            GameModuleInit();//游戏模块的初始化

            NetSystemInit();


            Console.ReadLine();//读取用户的一行输入
        }

        public static USocket uSocket;
        static void NetSystemInit() {
            uSocket = new USocket(DispatchNetEvent);
            Debug.Log("网络系统初始化完成!");
        }

        static void DispatchNetEvent(BufferEntity buffer) {
            //进行报文分发
            NetEvent.Instance.Dispatch(buffer.messageID, buffer);

        }

        /// <summary>
        /// 游戏模块的初始化
        /// </summary>
        static void GameModuleInit() {
            UserModule.Instance.Init();
            RolesModule.Instance.Init();
            LobbyModule.Instance.Init();
            RoomModule.Instance.Init();
            BattleModule.Instance.Init();
        }
    }
}
