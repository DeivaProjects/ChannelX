using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

namespace ChannelX.Redis
{ 

    public class RedisConnection
    {
        private readonly ConnectionMultiplexer _connection;
        private bool IsConnected{get;}
        public RedisConnection()
        {
            try{
                this._connection = ConnectionMultiplexer.Connect("localhost:6379"); // change connection String
                IsConnected = true;
            }
            catch{
                // will handle in the future
                IsConnected = false;
            }
            
        }

        private IDatabase GetDatabase()
        { 
            return this._connection.GetDatabase();
        }
    
        public bool HashSet(string key, HashEntry[] arr )
        {
            if(this.IsConnected)
            {
                var db = this.GetDatabase();
                db.HashSet(key,arr);
                return true; // check this at unit tests
            }
            else
            {
                return false;
            } 
        }
        public RedisValue HashGet(string key, string hashField)
        {
            RedisValue ret = new RedisValue();
            if(this.IsConnected)
            {
                var db = this.GetDatabase();
                ret = db.HashGet(key,hashField);
            }
            return ret;
        }
        public HashEntry[] HashGetAll(string key)
        {
            HashEntry[] ret = new HashEntry[0];
            if(this.IsConnected)
            {
                var db = this.GetDatabase();
                ret = db.HashGetAll(key);
            }
            return ret;
        }

        public RedisValue[] ListRange(string key, int start, int stop)
        {
            RedisValue[] ret = new RedisValue[0];
            if(this.IsConnected)
            {
                var db = this.GetDatabase();
                ret = db.ListRange(key,start,stop);
            }
            return ret;
        }

        public bool ListRightPush(string key, string val)
        {
            if(this.IsConnected)
            {
                var db = this.GetDatabase();
                db.ListRightPush(key,val);
                return true; // check this at unit tests
            }
            else
            {
                return false;
            } 
        }

    }
}
