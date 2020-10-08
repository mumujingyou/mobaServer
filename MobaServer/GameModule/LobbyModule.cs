using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobaServer.Match;
using MobaServer.Net;
using MobaServer.Player;
using ProtoMsg;

namespace MobaServer.GameModule
{
    class LobbyModule : GameModuleBase<LobbyModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1300, HandleLobbyToMatchC2S);
            NetEvent.Instance.AddEventListener(1302, HandleLobbyQuitMatchC2S);
        }

        //退出匹配
        private void HandleLobbyQuitMatchC2S(BufferEntity request)
        {
            LobbyQuitMatchC2S c2sMSG = ProtobufHelper.FromBytes<LobbyQuitMatchC2S>(request.proto);
            LobbyQuitMatchS2C s2cMSG = new LobbyQuitMatchS2C();

            PlayerEntity player = PlayerManager.GetPlayerEntityFromSession(request.session);
            if (player!=null)
            {
               bool result= MatchManager.Instance.Remove(player.matchEntity);
                if (result==true)
                {
                    player.matchEntity = null;
                    s2cMSG.Result = 0;//移除成功
                }
                else
                {
                    s2cMSG.Result = 1;//不在匹配状态
                }
            }
            BufferFactory.CreqateAndSendPackage(request, s2cMSG);
        }

        //进入匹配
        private void HandleLobbyToMatchC2S(BufferEntity request)
        {
            LobbyToMatchC2S c2sMSG = ProtobufHelper.FromBytes<LobbyToMatchC2S>(request.proto);
            LobbyToMatchS2C s2cMSG = new LobbyToMatchS2C();
            s2cMSG.Result = 0;

            MatchEntity matchEntity = new MatchEntity();
            PlayerEntity player= PlayerManager.GetPlayerEntityFromSession(request.session);
            //缓存匹配信息
            player.matchEntity = matchEntity;

            matchEntity.TeamID = player.rolesInfo.RolesID;
            matchEntity.player = player;

            BufferFactory.CreqateAndSendPackage(request, s2cMSG);

            //让角色进入匹配状态 
            MatchManager.Instance.Add(matchEntity);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Release()
        {
            base.Release();
            NetEvent.Instance.RemoveEventListener(1300, HandleLobbyToMatchC2S);
            NetEvent.Instance.RemoveEventListener(1302, HandleLobbyQuitMatchC2S);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
        }
    }
}
