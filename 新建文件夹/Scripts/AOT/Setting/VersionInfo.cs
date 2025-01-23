using JKFrame;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/VersionInfo")]
public class VersionInfo : ConfigBase
{
    public class VersionData
    {
        [Multiline]
        public string info;
    }
    public Dictionary<LanguageType, VersionData> versionInfoDic = new Dictionary<LanguageType, VersionData>();
    
    /// <summary>
    /// 获取版本信息(分语言)
    /// </summary>
    /// <param name="type">语言类型</param>
    /// <returns></returns>
    public VersionData GetVerisonData(LanguageType type)
    {
        return versionInfoDic[type];
    }
}
