using MobaServer.Match;
using MobaServer.Room;
using ProtoMsg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Player
{
    public class PlayerEntity
    {
        public int session;//会话ID
        public UserInfo userInfo;
        public RolesInfo rolesInfo;

        //匹配的信息
        public MatchEntity matchEntity;
        //房间的信息
        internal RoomEntity roomEntity;

        //阵营ID
        internal int TeamID;

     

        /// <summary>
        /// 用户销毁的时候
        /// </summary>
        public void Destroy()
        {
            Debug.Log("用户断开连接");


        }


    }
}
