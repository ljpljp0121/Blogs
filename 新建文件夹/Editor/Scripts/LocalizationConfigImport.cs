using UnityEditor;
using OfficeOpenXml;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class LocalizationConfigImport
{
    [MenuItem("Project/����ȫ�ֱ��ػ�", priority = 0)]
    public static void Import()
    {
        //��ȡSO��Դ����յ�ǰ���÷��㵼��
        string soPath = "Assets/Config/GlobalLocalizationConfig.asset";
        LocalizationConfig localizationConfig = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(soPath);
        localizationConfig.config.Clear();

        string excelPath = Application.dataPath + "/Config/Excel/���ػ�ȫ������.xlsx";
        FileInfo fileInfo = new FileInfo(excelPath);
        using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            //Excel�������1��ʼ��
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[1];
            int maxCol = worksheet.Cells.Columns; // �п����п��в�����ȫ����
            //Key ���� Ӣ��
            for (int x = 2; x < maxCol; x++)//��һ���Ǳ�ͷ
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
        Debug.Log("���ȫ�ֱ��ػ�Excelת��");
    }
}
