using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobaServer.MySql;
using MobaServer.Net;
using MobaServer.Player;
using ProtoMsg;

namespace MobaServer.GameModule
{
    public class RolesModule : GameModuleBase<RolesModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1201, HandleRolesCreateC2S);
        }

        private void HandleRolesCreateC2S(BufferEntity request)
        {
            //去数据库查询下角色表有没有存在相同名称的 

            RolesCreateC2S c2sMSG = ProtobufHelper.FromBytes<RolesCreateC2S>(request.proto);

            RolesCreateS2C s2cMSG = new RolesCreateS2C();

            //数据库查询 结果为空 说明没有存在该角色名称
            if (DBRolesInfo.Instance.Select(MySqlCMD.Where("NickName",c2sMSG.NickName))==null)
            {
                //用户ID
                PlayerEntity player = PlayerManager.GetPlayerEntityFromSession(request.session);

                RolesInfo rolesInfo = new RolesInfo();
                rolesInfo.NickName = c2sMSG.NickName;
                rolesInfo.ID=player.userInfo.ID;
                rolesInfo.RolesID = player.userInfo.ID;

                bool result= DBRolesInfo.Instance.Insert(rolesInfo);
                if (result==true)
                {
                    s2cMSG.Result = 0;
                    s2cMSG.RolesInfo = rolesInfo;
                    //缓存角色的信息到服务器本地
                    player.rolesInfo = rolesInfo;

                }
                else
                {
                    s2cMSG.Result = 2;//未知的异常 等待排查
                    Debug.Log($"插入角色数据存在异常,昵称:{c2sMSG.NickName}!");
                }
            }
            else
            {
                s2cMSG.Result = 1;//创建结果是1
            }
            BufferFactory.CreqateAndSendPackage(request, s2cMSG);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void Release()
        {
            base.Release();
        }

        public override void RemoveListener()
        {
            base.RemoveListener();
            NetEvent.Instance.RemoveEventListener(1201, HandleRolesCreateC2S);
        }
    }
}
