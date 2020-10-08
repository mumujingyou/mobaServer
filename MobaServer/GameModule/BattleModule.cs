using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobaServer.Net;
using MobaServer.Room;
using ProtoMsg;

namespace MobaServer.GameModule
{
    class BattleModule : GameModuleBase<BattleModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1500, HandleBattleUserInputC2S);
        }

        /// <summary>
        /// 处理用户传输过来的输入
        /// </summary>
        /// <param name="obj"></param>
        private void HandleBattleUserInputC2S(BufferEntity request)
        {
            BattleUserInputC2S c2sMSG= ProtobufHelper.FromBytes<BattleUserInputC2S>(request.proto);
            RoomEntity roomEntity= RoomManager.Instance.Get(c2sMSG.RoomID);
            if (roomEntity!=null)
            {
                roomEntity.HandleBattleUserInputC2S(c2sMSG);
            }
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Release()
        {
            base.Release();
            NetEvent.Instance.RemoveEventListener(1500, HandleBattleUserInputC2S);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
        }
    }
}
