using System;
using System.Collections.Generic;
using System.Text;
using MobaServer.MySql;
using MobaServer.Net;
using MobaServer.Player;
using ProtoMsg;

namespace MobaServer.GameModule
{
    public class UserModule : GameModuleBase<UserModule>
    {
        public override void AddListener()
        {
            base.AddListener();
            NetEvent.Instance.AddEventListener(1000, HandleUserRegisterC2S);
            NetEvent.Instance.AddEventListener(1001, HandleUserLoginC2S);
        }

        /// <summary>
        /// 登录功能
        /// </summary>
        /// <param name="request"></param>
        private void HandleUserLoginC2S(BufferEntity request)
        {
            //反序列化 得到客户端 发送的数据

            UserLoginC2S c2sMSG= ProtobufHelper.FromBytes<UserLoginC2S>(request.proto);
            //主要是看反序列化的功能 是否正常
            //Debug.Log("登录:"+ JsonHelper.SerializeObject(c2sMSG));

            //匹配记录:相同的账号 相同的密码
            string sqlCMD = MySqlCMD.Where("Account", c2sMSG.UserInfo.Account) +
                MySqlCMD.And("Password", c2sMSG.UserInfo.Password);

            UserLoginS2C s2cMSG = new UserLoginS2C();
            UserInfo userInfo= DBUserInfo.Instance.Select(sqlCMD);

            if (userInfo!=null)
            {
                s2cMSG.UserInfo = userInfo;
                s2cMSG.Result = 0;//登录成功

                //保存角色信息到服务器本地
                PlayerManager.Add(request.session, s2cMSG.UserInfo.ID, new PlayerEntity()
                {
                    userInfo = s2cMSG.UserInfo,
                    session=request.session,
                }) ;

                RolesInfo rolesInfo= DBRolesInfo.Instance.Select(MySqlCMD.Where("ID", s2cMSG.UserInfo.ID));

                if (rolesInfo!=null)
                {
                    s2cMSG.RolesInfo = rolesInfo;
                    //获取到了角色信息 缓存起来
                    PlayerEntity playerEntity= PlayerManager.GetPlayerEntityFromSession(request.session);
                    playerEntity.rolesInfo = rolesInfo;
                }

            }
            else
            {
                s2cMSG.Result = 2;//帐号和密码不匹配
            }

            //返回结果
            BufferFactory.CreqateAndSendPackage(request, s2cMSG);

        }

        /// <summary>
        /// 注册功能
        /// </summary>
        /// <param name="request"></param>
        private void HandleUserRegisterC2S(BufferEntity request)
        {
            UserRegisterC2S c2sMSG= ProtobufHelper.FromBytes<UserRegisterC2S>(request.proto);

            UserRegisterS2C s2cMSG = new UserRegisterS2C();
            if (DBUserInfo.Instance.Select(MySqlCMD.Where("Account",c2sMSG.UserInfo.Account))!=null)
            {
                Debug.Log("帐号已被注册");
                s2cMSG.Result = 3;
            }
            else
            {
               bool result= DBUserInfo.Instance.Insert(c2sMSG.UserInfo);
                if (result==true)
                {
                    s2cMSG.Result = 0;//注册成功
                }
                else
                {
                    s2cMSG.Result = 4;//未知原因导致的失败
                }
            }

            //返回结果
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
            NetEvent.Instance.RemoveEventListener(1000, HandleUserRegisterC2S);
            NetEvent.Instance.RemoveEventListener(1001, HandleUserLoginC2S);
        }
    }
}
