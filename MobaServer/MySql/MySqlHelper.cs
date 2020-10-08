using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.MySql
{
    class MySqlHelper
    {
        //两个数据库
        //其中一个是用于存放帐号 服务器列表 
        //另一个是游戏相关的数据库 每一个区的
        //public static string linkUserDB = "server=127.0.0.1;port=3306;database=loluser;user=root;password=123456;";
        public static string linkGameDB = "server=127.0.0.1;port=3306;database=lolgame;user=root;password=123456;";

        //回收池 已经工作完的实体 
        public static List<MySqlEntity> RecoveryPool = new List<MySqlEntity>();

        /// <summary>
        /// 执行命令 :返回执行结果
        /// </summary>
        /// <param name="connectType"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static MySqlEntity Get(ConnectType connectType, string sql)
        {
            MySqlEntity mysqlEntity = Connect(connectType);//先获取实体 实体里面包含了链接的对象
            mysqlEntity.CreateCMD(sql);//通过实体的创建命令的方法 进行确定命令的sql语句 打开连接
            return mysqlEntity;
        }

        private static MySqlEntity Connect(ConnectType connectType)
        {
            var poolCount = RecoveryPool.Count;
            Debug.Log("连接池的数量:" + poolCount);

            for (int i = poolCount; i > 0; i--)
            {

                //已经完成工作的实体
                if (RecoveryPool[i - 1].state == 1)
                {
                    //连接类型一样的话
                    if (RecoveryPool[i - 1].connectType == connectType)
                    {
                        RecoveryPool[i - 1].Reset();
                        return RecoveryPool[i - 1];//重置后进行返回
                    }
                    else
                    {
                        //如果类型不一致 关闭 然后移除
                        RecoveryPool[i - 1].mySqlConnection.Close();
                        RecoveryPool.RemoveAt(i - 1);
                    }
                }

            }

            //如果池子并没有回收的连接实体
            switch (connectType)
            {
                //优化 TODO 用Task执行
                case ConnectType.USER:
                    return new MySqlEntity(connectType);//.mySqlConnection;
                case ConnectType.GAME:
                    return new MySqlEntity(connectType);//.mySqlConnection;
            }
            return null;
        }

        /// <summary>
        /// 查询的接口
        /// </summary>
        /// <param name="connectType">查询哪个数据库</param>
        /// <param name="sqlCMD">查询的命令</param>
        /// <returns></returns>
        public static MySqlDataReader SelectCMD(ConnectType connectType, string sqlCMD)
        {
            MySqlEntity entity = Get(connectType, sqlCMD);
            entity.ExecuteReader();//执行读取任务

            //回收本次的操作实体
            if (!RecoveryPool.Contains(entity))
            {
                RecoveryPool.Add(entity);
            }
            return entity.mySqlDataReader;

        }

        /// <summary>
        /// 更新的接口
        /// </summary>
        /// <param name="connectType">连接的类型</param>
        /// <param name="sqlCMD">更新语句</param>
        /// <returns></returns>
        public static bool UpdateCMD(ConnectType connectType, string sqlCMD)
        {
            MySqlEntity entity = Get(connectType, sqlCMD);
            //实体进行任务操作
            entity.ExecuteNonQuery();

            //进行回收
            if (!RecoveryPool.Contains(entity))
            {
                RecoveryPool.Add(entity);
            }

            //需要将更新的结果返回出去
            return entity.Result;
        }

        /// <summary>
        /// 增加的命令
        /// </summary>
        /// <param name="connectType">连接的类型</param>
        /// <param name="sqlCMD">操作的语句</param>
        /// <returns></returns>
        public static bool AddCMD(ConnectType connectType, string sqlCMD)
        {
            MySqlEntity entity = Get(connectType, sqlCMD);
            entity.ExecuteNonQuery();

            //回收
            if (!RecoveryPool.Contains(entity))
            {
                RecoveryPool.Add(entity);
            }
            //返回添加操作的结果
            return entity.Result;
        }

        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="connectType">连接类型</param>
        /// <param name="sqlCMD">操作语句</param>
        /// <returns></returns>
        public static bool DeleteCMD(ConnectType connectType, string sqlCMD)
        {
            MySqlEntity entity = Get(connectType, sqlCMD);
            entity.ExecuteNonQuery();
            //回收实体
            if (!RecoveryPool.Contains(entity))
            {
                RecoveryPool.Add(entity);
            }
            return entity.Result;
        }
        
    }

    /// <summary>
    /// 表示连接的类型 分为帐号与游戏数据库
    /// </summary>
    public enum ConnectType
    {
        USER,
        GAME,
    }
}
