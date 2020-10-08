using MobaServer.Match;
using ProtoMsg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Room
{
    class RoomManager:Singleton<RoomManager>
    {
        int roomID = 0;//房间ID 自增
        ConcurrentDictionary<int, RoomEntity> roomDIC = new ConcurrentDictionary<int, RoomEntity>();

        public void Add(List<MatchEntity> teamA, List<MatchEntity> teamB) {
            roomID += 1;

            RoomInfo roomInfo = new RoomInfo();
            roomInfo.ID = roomID;
            for (int i = 0; i < teamA.Count; i++)
            {
                MatchManager.Instance.Remove(teamA[i]);

                //teamA[i].player.rolesInfo.
                roomInfo.TeamA.Add(teamA[i].player.rolesInfo);
            }

            for (int i = 0; i < teamB.Count; i++)
            {
                MatchManager.Instance.Remove(teamB[i]);

                //teamA[i].player.rolesInfo.
                roomInfo.TeamB.Add(teamB[i].player.rolesInfo);
            }

            roomInfo.StartTime = TimeHelper.Now();
            RoomEntity roomEntity = new RoomEntity(roomInfo);
            if (roomDIC.TryAdd(roomInfo.ID, roomEntity))
            {
                //告诉每一个客户端 匹配成功了 然后进入到房间 选择英雄
                LobbyUpdateMatchStateS2C s2cMSG = new LobbyUpdateMatchStateS2C();
                s2cMSG.Result = 0;
                s2cMSG.RoomInfo = roomInfo;

                roomEntity.Broadcast(1301,s2cMSG);
            }

            //用户的队伍ID  房间实体 缓存
            for (int i = 0; i < teamA.Count; i++)
            {
                teamA[i].player.matchEntity = null;
                teamA[i].player.roomEntity = roomEntity;
                teamA[i].player.TeamID = 0;
            }
            for (int i = 0; i < teamB.Count; i++)
            {
                teamB[i].player.matchEntity = null;
                teamB[i].player.roomEntity = roomEntity;
                teamB[i].player.TeamID = 1;
            }
        }


        internal void Remove(int roomID)
        {
            RoomEntity roomEntity;
            if (roomDIC.TryRemove(roomID,out roomEntity))
            {
                roomEntity.Close();
                roomEntity = null;
            }
        }

        public RoomEntity Get(int roomID) {
            RoomEntity roomEntity;
            if (roomDIC.TryGetValue(roomID, out roomEntity))
            {
                return roomEntity;
            }
            return null;
        }

        public void CloseAll() {
            foreach (var roomEntity in roomDIC.Values)
            {
                Remove(roomEntity.roomID);
            }
        }
    }
}
