using JKFrame;
using MongoDB.Driver;
using UnityEngine;

/// <summary>
/// 数据库管理类
/// </summary>
public class DatabaseManager : SingletonMono<DatabaseManager>
{
    [SerializeField] private string connstr = "mongodb://localhost:27017/";
    private MongoClient mongoClient;
    private IMongoDatabase mmoDatabase;
    private IMongoCollection<PlayerData> playerDataCollection;

    /// <summary>
    /// 初始化DatabaseManager
    /// </summary>
    public void Init()
    {
        mongoClient = new MongoClient(connstr);
        mmoDatabase = mongoClient.GetDatabase("MMONetCode");
        playerDataCollection = mmoDatabase.GetCollection<PlayerData>("PlayerData");
    }

    /// <summary>
    /// 根据输入键获取数据库中数据
    /// </summary>
    /// <param name="name">玩家ID</param>
    /// <returns>PlayerData数据</returns>
    public PlayerData GetPlayerData(string name)
    {
        PlayerData playerData = playerDataCollection.Find(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name), name)).FirstOrDefault();
        return playerData;
    }

    /// <summary>
    /// 新建一条Player数据
    /// </summary>
    /// <param name="playerData"></param>
    public void CreatePlayerData(PlayerData playerData)
    {
        playerDataCollection.InsertOne(playerData);
    }

    /// <summary>
    /// 更新并保存数据库的Player数据
    /// </summary>
    /// <param name="playerData">Player数据</param>
    public void SavePlayerData(PlayerData playerData)
    {
        playerDataCollection.ReplaceOne(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name), playerData.name), playerData);
    }


}
