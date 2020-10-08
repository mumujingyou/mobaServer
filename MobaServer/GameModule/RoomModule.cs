using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobaServer.Net;
using MobaServer.Player;
using ProtoMsg;

namespace MobaServer.GameModule
{
    class RoomModule : GameModuleBase<RoomModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1400, HandleRoomSelectHeroC2S);
            NetEvent.Instance.AddEventListener(1401, HandleRoomSelectHeroSkillC2S);
            NetEvent.Instance.AddEventListener(1404, HandleRoomSendMsgC2S);
            NetEvent.Instance.AddEventListener(1405, HandleRoomLockHeroC2S);
            NetEvent.Instance.AddEventListener(1406, HandleRoomLoadProgressC2S);
        }

        /// <summary>
        /// 发送了加载进度过来
        /// </summary>
        /// <param name="request"></param>
        private void HandleRoomLoadProgressC2S(BufferEntity request)
        {
            RoomLoadProgressC2S c2sMSG = ProtobufHelper.FromBytes<RoomLoadProgressC2S>(request.proto);
            RoomLoadProgressS2C s2cMSG = new RoomLoadProgressS2C();
            s2cMSG.IsBattleStart = false;

            PlayerEntity p = PlayerManager.GetPlayerEntityFromSession(request.session);
            bool result= p.roomEntity.UpdateLoadProgress(p.rolesInfo.RolesID, c2sMSG.LoadProgress);
            if (result==true)
            {
                //所有玩家都已经加载完成了
            }
            else
            {
                p.roomEntity.GetLoadProgress(ref s2cMSG);
                BufferFactory.CreqateAndSendPackage(request,s2cMSG);
            }
        }

        /// <summary>
        /// 锁定英雄
        /// </summary>
        /// <param name="request"></param>
        private void HandleRoomLockHeroC2S(BufferEntity request)
        {
            RoomLockHeroC2S c2sMSG = ProtobufHelper.FromBytes<RoomLockHeroC2S>(request.proto);
            RoomLockHeroS2C s2cMSG = new RoomLockHeroS2C();
            s2cMSG.HeroID = c2sMSG.HeroID;

            PlayerEntity p = PlayerManager.GetPlayerEntityFromSession(request.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;

            //缓存角色技能
            p.roomEntity.LockHero(s2cMSG.RolesID, s2cMSG.HeroID);

            p.roomEntity.Broadcast(request.messageID, s2cMSG);
        }

        /// <summary>
        /// 发送聊天信息
        /// </summary>
        /// <param name="request"></param>
        private void HandleRoomSendMsgC2S(BufferEntity request)
        {
            RoomSendMsgC2S c2sMSG = ProtobufHelper.FromBytes<RoomSendMsgC2S>(request.proto);
            RoomSendMsgS2C s2cMSG = new RoomSendMsgS2C();
            PlayerEntity p = PlayerManager.GetPlayerEntityFromSession(request.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;
            s2cMSG.Text = c2sMSG.Text;

            //指向广播给同个阵营的玩家 
            //p.roomEntity.Broadcast(p.TeamID, request.messageID, s2cMSG);

            p.roomEntity.Broadcast( request.messageID, s2cMSG);
        }

        /// <summary>
        /// 选择英雄
        /// </summary>
        /// <param name="request"></param>
        private void HandleRoomSelectHeroSkillC2S(BufferEntity request)
        {
            RoomSelectHeroSkillC2S c2sMSG = ProtobufHelper.FromBytes<RoomSelectHeroSkillC2S>(request.proto);
            RoomSelectHeroSkillS2C s2cMSG = new RoomSelectHeroSkillS2C();
            s2cMSG.SkillID = c2sMSG.SkillID;
            s2cMSG.GridID = c2sMSG.GridID;
            PlayerEntity p = PlayerManager.GetPlayerEntityFromSession(request.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;

            //缓存角色技能
            p.roomEntity.UpdateSKill(s2cMSG.RolesID,s2cMSG.SkillID,s2cMSG.GridID);

            p.roomEntity.Broadcast(request.messageID, s2cMSG);
        }

        /// <summary>
        /// 用户选择英雄
        /// </summary>
        /// <param name="request"></param>
        private void HandleRoomSelectHeroC2S(BufferEntity request)
        {
            RoomSelectHeroC2S c2sMSG = ProtobufHelper.FromBytes<RoomSelectHeroC2S>(request.proto);
            RoomSelectHeroS2C s2cMSG = new RoomSelectHeroS2C();
            s2cMSG.HeroID = c2sMSG.HeroID;
            PlayerEntity p= PlayerManager.GetPlayerEntityFromSession(request.session);
            s2cMSG.RolesID = p.rolesInfo.RolesID;

            p.roomEntity.Broadcast(request.messageID,s2cMSG);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Release()
        {
            base.Release();
            NetEvent.Instance.RemoveEventListener(1400, HandleRoomSelectHeroC2S);
            //shift+alt
            NetEvent.Instance.RemoveEventListener(1401, HandleRoomSelectHeroSkillC2S);
            NetEvent.Instance.RemoveEventListener(1404, HandleRoomSendMsgC2S);
            NetEvent.Instance.RemoveEventListener(1405, HandleRoomLockHeroC2S);
            NetEvent.Instance.RemoveEventListener(1406, HandleRoomLoadProgressC2S);
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
        }
    }
}
