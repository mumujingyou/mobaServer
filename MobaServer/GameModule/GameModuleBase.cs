using System;
using System.Collections.Generic;
using System.Text;

namespace MobaServer.GameModule
{
   public class GameModuleBase<T> where T:new()
    {
        static T instance;
        public static T Instance {
            get
            {
                if (instance==null)
                {
                    instance = new T();
                }
                return instance;
            }
        }

        //初始化
        public virtual void Init() {
            AddListener();
        }

        //释放
        public virtual void Release()
        {
            RemoveListener();
        }

        //添加监听的事件
        public virtual void AddListener()
        {


        }


        //移除监听的事件 
        public virtual void RemoveListener()
        {


        }
    }
}
