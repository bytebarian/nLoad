using System;
using System.Collections.Generic;
using System.Text;

namespace nLoad
{
    public class ConnectionPool
    {
        public string Name { get; set; }
        public Func<object> OpenConnection { get; set; }
        public Action<object> CloseConnection { get; set; }

        public static ConnectionPool Create(string name, Func<object> openConnection, Action<object> closeConnection) => new ConnectionPool
        {
            Name = name,
            OpenConnection = openConnection,
            CloseConnection = closeConnection
        };
    }
}
