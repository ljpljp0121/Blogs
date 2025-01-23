using UnityEditor;
using OfficeOpenXml;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class LocalizationConfigImport
{
    [MenuItem("Project/导入全局本地化", priority = 0)]
    public static void Import()
    {
        //获取SO资源并清空当前设置方便导入
        string soPath = "Assets/Config/GlobalLocalizationConfig.asset";
        LocalizationConfig localizationConfig = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(soPath);
        localizationConfig.config.Clear();

        string excelPath = Application.dataPath + "/Config/Excel/本地化全局配置.xlsx";
        FileInfo fileInfo = new FileInfo(excelPath);
        using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            //Excel中数组从1开始，
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[1];
            int maxCol = worksheet.Cells.Columns; // 有可能有空行不能完全相信
            //Key 中文 英文
            for (int x = 2; x < maxCol; x++)//第一行是表头
            {
                string key = worksheet.Cells[x, 1].Text.Trim();
                if (string.IsNullOrEmpty(key)) break;
                string chinese = worksheet.Cells[x, 2].Text.Trim();
                string english = worksheet.Cells[x, 3].Text.Trim();
                localizationConfig.config.Add(key, new Dictionary<LanguageType, LocalizationDataBase>
                {
                    {LanguageType.SimplifiedChinese,new LocalizationStringData{content = chinese} },
                    {LanguageType.English, new LocalizationStringData{content = english}},
                });
            }
        }
        EditorUtility.SetDirty(localizationConfig);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("完成全局本地化Excel转换");
    }
}
