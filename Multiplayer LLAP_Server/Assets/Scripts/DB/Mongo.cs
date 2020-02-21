using MongoDB.Driver;
using UnityEngine;

public class Mongo
{
    private MongoClient client;
    private MongoServer server;
    private MongoDatabase db;

    private MongoCollection accounts;

    public void Init()
    {
        client = new MongoClient("mongodb://cxz5309:p%40ssw0rd%279%27%21@cluster0-shard-00-00-cspni.mongodb.net:27017,cluster0-shard-00-01-cspni.mongodb.net:27017,cluster0-shard-00-02-cspni.mongodb.net:27017/?ssl=true&replicaSet=Cluster0-shard-0&authSource=admin&w=majority");
        server = client.GetServer();
        db = server.GetDatabase("db");

        accounts = db.GetCollection<Model_Account>("account");

        Debug.Log("Database has been initialized");
    }
    public void Shutdown()
    {
        client = null;
        server.Shutdown();
        db = null;
    }

    #region Fetch
    public bool InsertAccount(string username, string password, string email)
    {
        Model_Account newAccount = new Model_Account();
        newAccount.Username = username;
        newAccount.ShaPassword = password;
        newAccount.Email = email;
        newAccount.Discriminator = "0000";

        accounts.Insert(newAccount);

        return true;
    }
    #endregion

    #region Update
    #endregion

    #region Delete
    #endregion
}
