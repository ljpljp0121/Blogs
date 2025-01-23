using JKFrame;
using MongoDB.Driver;
using UnityEngine;

/// <summary>
/// ���ݿ������
/// </summary>
public class DatabaseManager : SingletonMono<DatabaseManager>
{
    [SerializeField] private string connstr = "mongodb://localhost:27017/";
    private MongoClient mongoClient;
    private IMongoDatabase mmoDatabase;
    private IMongoCollection<PlayerData> playerDataCollection;

    /// <summary>
    /// ��ʼ��DatabaseManager
    /// </summary>
    public void Init()
    {
        mongoClient = new MongoClient(connstr);
        mmoDatabase = mongoClient.GetDatabase("MMONetCode");
        playerDataCollection = mmoDatabase.GetCollection<PlayerData>("PlayerData");
    }

    /// <summary>
    /// �����������ȡ���ݿ�������
    /// </summary>
    /// <param name="name">���ID</param>
    /// <returns>PlayerData����</returns>
    public PlayerData GetPlayerData(string name)
    {
        PlayerData playerData = playerDataCollection.Find(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name), name)).FirstOrDefault();
        return playerData;
    }

    /// <summary>
    /// �½�һ��Player����
    /// </summary>
    /// <param name="playerData"></param>
    public void CreatePlayerData(PlayerData playerData)
    {
        playerDataCollection.InsertOne(playerData);
    }

    /// <summary>
    /// ���²��������ݿ��Player����
    /// </summary>
    /// <param name="playerData">Player����</param>
    public void SavePlayerData(PlayerData playerData)
    {
        playerDataCollection.ReplaceOne(Builders<PlayerData>.Filter.Eq(nameof(PlayerData.name), playerData.name), playerData);
    }


}
