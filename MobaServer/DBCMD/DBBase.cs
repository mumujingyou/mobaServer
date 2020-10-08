using System;
using System.Collections.Generic;
using System.Text;


public class DBBase<T> where T : new()
{
    static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }
}
