using JKFrame;
using MongoDB.Bson.Serialization.Attributes;
using UnityEngine;

/// <summary>
/// 玩家数据
/// </summary>
public class PlayerData
{
    [BsonId]
    public string name;
    public string password;
    public CharacterData characterData = new CharacterData();

    public void Init(string name, string password, Vector3 position)
    {
        this.name = name;
        this.password = password;
        this.characterData.position = position;
    }
}

/// <summary>
/// 玩家角色数据
/// </summary>
public class CharacterData
{
    public Vector3 position;
    public float rotation_Y;
}