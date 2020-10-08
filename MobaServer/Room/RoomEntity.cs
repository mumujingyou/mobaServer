using Google.Protobuf;
using MobaServer.Net;
using MobaServer.Player;
using ProtoMsg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Room
{
   public class RoomEntity
   {
        //属性:房间ID 选择英雄的时间配置 房间的信息 房间中的玩家列表 UClient客户端列表 锁定次数
        //加载进度 是否所有用户都加载完成了 
        public int roomID;
        public int selectHeroTime=20000;
        public RoomInfo roomInfo;
        ConcurrentDictionary<int, PlayerInfo> playerList = new ConcurrentDictionary<int, PlayerInfo>();
        ConcurrentDictionary<int, UClient> clientList = new ConcurrentDictionary<int, UClient>();
        public int lockCount;
        //每个玩家的加载进度
        ConcurrentDictionary<int, int> playerProgress = new ConcurrentDictionary<int, int>();

        bool isLoadComplete = false;

        /// <summary>
        /// 处理用户的输入
        /// </summary>
        /// <param name="c2sMSG"></param>
        internal void HandleBattleUserInputC2S(BattleUserInputC2S c2sMSG)
        {
            BattleUserInputS2C s2cMSG = new BattleUserInputS2C();
            s2cMSG.CMD = c2sMSG;

            Broadcast(1500,s2cMSG);
            //定一个间隔时间 66ms进行广播一次  100ms广播一次 
        }



        //接口:初始化 缓存加载进度 获取加载进度 锁定英雄 选择了召唤师技能 玩家信息的初始化 
        //广播给房间的所有人 广播给自己队伍的玩家 销毁的时候
        public RoomEntity(RoomInfo roomInfo) {
            
            this.roomID = roomInfo.ID;
            this.roomInfo = roomInfo;
            Init();
        }

        /// <summary>
        /// 角色的初始化
        /// </summary>
        void PlayerInit() {

            for (int i = 0; i < roomInfo.TeamA.Count; i++)
            {
                PlayerInfo playerInfo = new PlayerInfo();
                playerInfo.RolesInfo = roomInfo.TeamA[i];
                //默认的值
                playerInfo.SkillA = 103;
                playerInfo.SkillB = 106;
                playerInfo.HeroID = 0;//表示未选择
                playerInfo.TeamID = 0;
                playerInfo.PosID = i;//0-4
                playerList.TryAdd(playerInfo.RolesInfo.RolesID, playerInfo);


                UClient client = GameManager.uSocket.GetClient
                    (PlayerManager.GetPlayerEntityFromRoles(playerInfo.RolesInfo.RolesID).session);
                //缓存每一个客户端 为广播接口服务的
                clientList.TryAdd(playerInfo.RolesInfo.RolesID, client);

                //加载进度
                playerProgress.TryAdd(playerInfo.RolesInfo.RolesID, 0);
            }

            for (int i = 0; i < roomInfo.TeamB.Count; i++)
            {
                PlayerInfo playerInfo = new PlayerInfo();
                playerInfo.RolesInfo = roomInfo.TeamB[i];
                //默认的值
                playerInfo.SkillA = 103;
                playerInfo.SkillB = 106;
                playerInfo.HeroID = 0;//表示未选择
                playerInfo.TeamID = 1;
                playerInfo.PosID = i+5;//5-9
                playerList.TryAdd(playerInfo.RolesInfo.RolesID, playerInfo);

                UClient client = GameManager.uSocket.GetClient
               (PlayerManager.GetPlayerEntityFromRoles(playerInfo.RolesInfo.RolesID).session);
                //缓存每一个客户端 为广播接口服务的
                clientList.TryAdd(playerInfo.RolesInfo.RolesID, client);

                //加载进度
                playerProgress.TryAdd(playerInfo.RolesInfo.RolesID, 0);
            }
        }

        /// <summary>
        /// 整个房间的初始化
        /// </summary>
        private async void Init()
        {
            PlayerInit();
            //选择英雄的时间 
            await Task.Delay(selectHeroTime);
            //是不是所有玩家都锁定了英雄 
            if (lockCount==(roomInfo.TeamA.Count+ roomInfo.TeamB.Count))
            {
                //所有人都锁定了选择的英雄
                //可以加载战斗了
                RoomToBattleS2C s2cMSG = new RoomToBattleS2C();

                foreach (var rolesID in playerList.Keys)
                {
                    UClient client = GameManager.uSocket.GetClient
                        (PlayerManager.GetPlayerEntityFromRoles(rolesID).session);
                    //缓存每一个客户端 为广播接口服务的
                    clientList.TryAdd(rolesID, client);

                    s2cMSG.PlayerList.Add(playerList[rolesID]);
                }
                Broadcast(1407,s2cMSG);
            }
            else
            {
                //解散房间
                RoomCloseS2C s2cMSG = new RoomCloseS2C();
                Broadcast(1403, s2cMSG);
                //通知房间管理器 释放掉这个房间
                RoomManager.Instance.Remove(roomID);
            }
        }

        #region 广播的接口
        public void Broadcast(int messageID, IMessage s2cMSG)
        {
            foreach (var client in clientList.Values)
            {
                BufferFactory.CreqateAndSendPackage(client, messageID, s2cMSG);
            }
        }

        public void Broadcast(int teamID,int messageID, IMessage s2cMSG)
        {
            if (teamID==0)
            {
                //A队伍
                for (int i = 0; i < roomInfo.TeamA.Count; i++)
                {
                    UClient client;
                    if (clientList.TryGetValue(roomInfo.TeamA[i].RolesID, out client))
                    {
                        BufferFactory.CreqateAndSendPackage(client, messageID, s2cMSG);
                    }
                }
            }
            else
            {
                //B队伍
                for (int i = 0; i < roomInfo.TeamB.Count; i++)
                {
                    UClient client;
                    if (clientList.TryGetValue(roomInfo.TeamB[i].RolesID, out client))
                    {
                        BufferFactory.CreqateAndSendPackage(client, messageID, s2cMSG);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 锁定英雄
        /// </summary>
        /// <param name="rolesID"></param>
        /// <param name="heroID"></param>
        public void LockHero(int rolesID,int heroID) {
            lockCount += 1;
            playerList[rolesID].HeroID = heroID;
        }

        /// <summary>
        /// 更新召唤师技能
        /// </summary>
        /// <param name="rolesID"></param>
        /// <param name="skillID"></param>
        /// <param name="gridID"></param>
        public void UpdateSKill(int rolesID,int skillID,int gridID) {
            if (gridID==0)
            {
                playerList[rolesID].SkillA = skillID;
            }
            else
            {
                playerList[rolesID].SkillB = skillID;
            }
        }

        /// <summary>
        /// 更新所有用户的进度
        /// </summary>
        /// <param name="rolesID"></param>
        /// <param name="progress"></param>
        public bool UpdateLoadProgress(int rolesID,int progress) {
            if (isLoadComplete==true)
            {
                return true;
            }
            playerProgress[rolesID] = progress;
            if (isLoadComplete==false)
            {
                foreach (var value in playerProgress.Values)
                {
                    //100 实际的进度 90客户端异步加载场景 0-0.9
                    if (value<100)
                    {
                        isLoadComplete = false;
                        return false;
                    }
                }
                //所有玩家都加载完成了
                isLoadComplete = true;
                //告诉所有客户端 都加载完成了
                RoomLoadProgressS2C s2cMSG = new RoomLoadProgressS2C();
                s2cMSG.IsBattleStart = true;
                foreach (var item in playerProgress.Keys)
                {
                    s2cMSG.RolesID.Add(item);
                    s2cMSG.LoadProgress.Add(playerProgress[item]);
                }
                Broadcast(1406, s2cMSG);
            }
            return true;
        }

       
        /// <summary>
        /// 获取所有用户的加载进度
        /// </summary>
        /// <param name="s2cMSG"></param>
        public void GetLoadProgress(ref RoomLoadProgressS2C s2cMSG) {
            foreach (var item in playerProgress.Keys)
            {
                s2cMSG.RolesID.Add(item);
                s2cMSG.LoadProgress.Add(playerProgress[item]);
            }
        }

        //房间关闭销毁
        public void Close() { 
        
        }
    }
}
