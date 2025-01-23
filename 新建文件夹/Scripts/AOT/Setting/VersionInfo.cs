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
    /// ��ȡ�汾��Ϣ(������)
    /// </summary>
    /// <param name="type">��������</param>
    /// <returns></returns>
    public VersionData GetVerisonData(LanguageType type)
    {
        return versionInfoDic[type];
    }
}
