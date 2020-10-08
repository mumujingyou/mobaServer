using MobaServer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobaServer.Match
{
    /// <summary>
    /// 每一个队伍的实体
    /// </summary>
    public class MatchEntity
    {
        public PlayerEntity player;
        public int TeamID;//队伍ID
    }
}
