using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoMsg;
using MobaServer;
using MobaServer.MySql;

public class DBUserInfo : DBBase<DBUserInfo>
{
    //查询 根据条件返回实体类
    public UserInfo Select(string sql)
    {

        UserInfo UserInfo = new UserInfo();
        string sqlCmd = "select *from userinfo_a " + sql;

        //查的表明一定是类的名称
        var r = MySqlHelper.SelectCMD(ConnectType.GAME, sqlCmd);
        if (r.HasRows == false)
        {
            return null;
        }
        else
        {
            if (r.Read())
            {
                
                UserInfo.ID=r.GetInt32("ID");
                UserInfo.Account=r.GetString("Account");
                UserInfo.Password=r.GetString("Password"); 
                //"UserInfo"."字段名称" = r.Get"数据类型("字段名称")";
            }

        }
        return UserInfo;
    }

    //查询 根据ID -> 条件  查询后返回一个类 Single
    public List<UserInfo> SelectList(string sql)
    {
        List<UserInfo> UserInfoList = new List<UserInfo>();
        
        string sqlCmd= "select *from userinfo_a " + sql;
        //查的表明一定是类的名称
        var r = MySqlHelper.SelectCMD(ConnectType.GAME, sqlCmd);
        if (r.HasRows == false)
        {
            return null;
        }
        else
        {
            while (r.Read())
            {
                UserInfo UserInfo = new UserInfo();

                
                UserInfo.ID=r.GetInt32("ID");
                UserInfo.Account=r.GetString("Account");
                UserInfo.Password=r.GetString("Password"); 
                //"UserInfo"."字段名称" = r.Get"数据类型("字段名称")";

                UserInfoList.Add(UserInfo);
            }
        }
        return UserInfoList;
    }
    
    //删除-key 根据条件删除 
    public bool Delete(string sql)
    {
        string sqlCmd= "delete from userinfo_a " + sql;
        bool b = MySqlHelper.DeleteCMD(ConnectType.GAME, sqlCmd);

        return b;
    }

    //插入->添加的是一整条的数据  如果有主键  先判断主键是否存在了 返回插入结果 isSingle是否保持唯一性
    public bool Insert(UserInfo UserInfo)
    {
        //ikey需要通过运算获得  
        //ivalue需要通过append
        //string sqlCmd = "insert into userinfo_a("+ikey+") values("+ivalues+")";
		string sqlCmd = "insert into userinfo_a(Account,Password)  values("+"\"" + UserInfo.Account + "\""
                        +"," +"\"" + UserInfo.Password + "\""
                        +")";
        bool b = MySqlHelper.AddCMD(ConnectType.GAME, sqlCmd);
        return b;
    }

 
    //改 先进行查询 然后再进行修改  返回更新结果
    public bool Update(string setSql,string whereSql)
    {
        if (!string.IsNullOrEmpty(setSql))
        {
            if (setSql[0] == ',')
            {
                setSql = setSql.Remove(0, 1);
            }
        }
        //UPDATE account  set id=115 where id=1 and name="mm"
        string sqlCmd = "UPDATE userinfo_a set " + setSql + " " + whereSql;
        //UPDATE account  set id=115 where(id=1)
        bool b = MySqlHelper.UpdateCMD(ConnectType.GAME, sqlCmd);
        return b;
    }
}