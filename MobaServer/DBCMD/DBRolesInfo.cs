using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoMsg;
using MobaServer;
using MobaServer.MySql;

/// <summary>
/// 角色信息的数据库操作类
/// </summary>
public class DBRolesInfo : DBBase<DBRolesInfo>
{
    //查询 根据条件返回实体类
    public RolesInfo Select(string sql)
    {

        RolesInfo RolesInfo = new RolesInfo();
        string sqlCmd = "select *from rolesinfo_a " + sql;

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
                
                RolesInfo.ID=r.GetInt32("ID");
                RolesInfo.RolesID=r.GetInt32("RolesID");
                RolesInfo.NickName=r.GetString("NickName");
                RolesInfo.Level=r.GetInt32("Level");
                RolesInfo.State=r.GetInt32("State");
                RolesInfo.VictoryPoint=r.GetInt32("VictoryPoint");
                RolesInfo.GoldCoin=r.GetInt32("GoldCoin");
                RolesInfo.Diamonds=r.GetInt32("Diamonds");
                RolesInfo.RoomID=r.GetInt32("RoomID");
                RolesInfo.SeatID=r.GetInt32("SeatID"); 
                //"RolesInfo"."字段名称" = r.Get"数据类型("字段名称")";
            }

        }
        return RolesInfo;
    }

    //查询 根据ID -> 条件  查询后返回一个类 Single
    public List<RolesInfo> SelectList(string sql)
    {
        List<RolesInfo> RolesInfoList = new List<RolesInfo>();
        
        string sqlCmd= "select *from rolesinfo_a " + sql;
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
                RolesInfo RolesInfo = new RolesInfo();

                
                RolesInfo.ID=r.GetInt32("ID");
                RolesInfo.RolesID=r.GetInt32("RolesID");
                RolesInfo.NickName=r.GetString("NickName");
                RolesInfo.Level=r.GetInt32("Level");
                RolesInfo.State=r.GetInt32("State");
                RolesInfo.VictoryPoint=r.GetInt32("VictoryPoint");
                RolesInfo.GoldCoin=r.GetInt32("GoldCoin");
                RolesInfo.Diamonds=r.GetInt32("Diamonds");
                RolesInfo.RoomID=r.GetInt32("RoomID");
                RolesInfo.SeatID=r.GetInt32("SeatID"); 
                //"RolesInfo"."字段名称" = r.Get"数据类型("字段名称")";

                RolesInfoList.Add(RolesInfo);
            }
        }
        return RolesInfoList;
    }
    
    //删除-key 根据条件删除 
    public bool Delete(string sql)
    {
        string sqlCmd= "delete from rolesinfo_a " + sql;
        bool b = MySqlHelper.DeleteCMD(ConnectType.GAME, sqlCmd);

        return b;
    }

    //插入->添加的是一整条的数据  如果有主键  先判断主键是否存在了 返回插入结果 isSingle是否保持唯一性
    public bool Insert(RolesInfo RolesInfo)
    {
        //ikey需要通过运算获得  
        //ivalue需要通过append
        //string sqlCmd = "insert into rolesinfo_a("+ikey+") values("+ivalues+")";
		string sqlCmd = "insert into rolesinfo_a(ID,RolesID,NickName,Level,State,VictoryPoint,GoldCoin,Diamonds,RoomID,SeatID)  values("+RolesInfo.ID
                        +"," + RolesInfo.RolesID
                        +"," +"\"" + RolesInfo.NickName + "\""
                        +"," + RolesInfo.Level
                        +"," + RolesInfo.State
                        +"," + RolesInfo.VictoryPoint
                        +"," + RolesInfo.GoldCoin
                        +"," + RolesInfo.Diamonds
                        +"," + RolesInfo.RoomID
                        +"," + RolesInfo.SeatID
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
        string sqlCmd = "UPDATE rolesinfo_a set " + setSql + " " + whereSql;
        //UPDATE account  set id=115 where(id=1)
        bool b = MySqlHelper.UpdateCMD(ConnectType.GAME, sqlCmd);
        return b;
    }
}