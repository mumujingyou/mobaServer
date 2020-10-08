using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.MySql
{
    public class MySqlEntity
    {
        public int state = 0;//正常 1已工作完 释放

        public ConnectType connectType;
        public MySqlConnection mySqlConnection;//连接
        public MySqlCommand mySqlCommand;
        public MySqlDataReader mySqlDataReader;
        public bool Result = false;

        /// <summary>
        /// 实体构造函数:根据传递的类型 构建mysql连接
        /// </summary>
        /// <param name="connectType"></param>
        public MySqlEntity(ConnectType connectType)
        {
            this.connectType = connectType;
            switch (connectType)
            {
                //优化 TODO 用Task执行
                case ConnectType.USER:
                    //mySqlConnection = new MySqlConnection(MySqlHelper.linkUserDB);
                    break;
                case ConnectType.GAME:
                    mySqlConnection = new MySqlConnection(MySqlHelper.linkGameDB);
                    break;
            }
        }

        /// <summary>
        /// 定义操作命令
        /// </summary>
        /// <param name="sql">操作的字符串</param>
        public void CreateCMD(string sql)
        {
            //构建实例操作命令对象的时候
            mySqlCommand = new MySqlCommand(sql, mySqlConnection);
            try
            {
                mySqlConnection.Open();//打开数据库
            }
            catch (Exception e)
            {

                Debug.Log("打开数据库异常:" + e.Message);
            }
        }

        /// <summary>
        /// 重置的方法
        /// </summary>
        internal void Reset()
        {
            Result = false;
            mySqlCommand = null;
            mySqlDataReader = null;
            state = 0;
            mySqlConnection.Close();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        internal void ExecuteNonQuery()
        {
            try
            {
                Result = mySqlCommand.ExecuteNonQuery() > 0 ? true : false;
            }
            catch (Exception e)
            {
                Result = false;
                Debug.Log("执行数据库操作命令出错:" + e.Message);
            }
            state = 1;//已经工作完
        }

        /// <summary>
        /// 查询,读取任务
        /// </summary>
        internal void ExecuteReader()
        {
            try
            {
                //存储了 执行命令之后返回的数据
                mySqlDataReader = mySqlCommand.ExecuteReader();
            }
            catch (Exception e)
            {
                Debug.Log("执行数据库读取命令出错:" + e.Message); ;
            }
            state = 1;
        }
    }
}
