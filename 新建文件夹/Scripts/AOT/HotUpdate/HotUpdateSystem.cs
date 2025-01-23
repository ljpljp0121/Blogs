
using HybridCLR;
using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// �ȸ���ϵͳ
/// </summary>
public class HotUpdateSystem : MonoBehaviour
{
    [Serializable]
    public class HotUpdateSystemState
    {
        public bool hotUpdateSucceed;
    }

    [SerializeField] private TextAsset[] aotDllAssets;
    [SerializeField] private string[] hotUpdateDllNames;
    [SerializeField] private string versionInfoAddressableKey;

    private Action<float> onPercentageForEachFile;
    private Action<bool> onEnd;

    public void StartHotUpdate(Action<float> onPercentageForEachFile, Action<bool> onEnd)
    {
        this.onPercentageForEachFile = onPercentageForEachFile;
        this.onEnd = onEnd;
        StartCoroutine(DoUpdateAddressables());
    }

    bool succeed;

    /// <summary>
    /// ����Addressable��Ŀ¼��Դ
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoUpdateAddressables()
    {
        //ȷ����һ���ȸ��µ�״̬
        HotUpdateSystemState state = SaveSystem.LoadSetting<HotUpdateSystemState>();
        if (state == null || !state.hotUpdateSucceed)
        {
            Debug.Log("�ϵ�����");
            string catalogPath = $"{Application.persistentDataPath}/com.unity.addressables";
            print("xxxxxx  path " + catalogPath);
            if (Directory.Exists(catalogPath)) Directory.Delete(catalogPath, true);
        }

        //��ʼ��
        yield return Addressables.InitializeAsync();
        succeed = true;

        //���Ŀ¼����
        var checkForCatalogUpdateHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkForCatalogUpdateHandle;

        if (checkForCatalogUpdateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            JKLog.Error($"CheckForCatalogUpdateʧ��:{checkForCatalogUpdateHandle.OperationException.Message}");
            Addressables.Release(checkForCatalogUpdateHandle);
        }
        else
        {
            List<string> catalogResult = checkForCatalogUpdateHandle.Result;
            Addressables.Release(checkForCatalogUpdateHandle);

            Debug.Log("xxxxxx  calogcount  " + catalogResult.Count);
            //�������µ�Ŀ¼
            if (catalogResult.Count > 0)
            {
                ShowLoadingWindow();
                var updateCatalogsHandle = Addressables.UpdateCatalogs(catalogResult, false);
                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    succeed = false;
                    Addressables.Release(updateCatalogsHandle);
                    JKLog.Error($"UpdateCatalogsʧ��:{updateCatalogsHandle.OperationException.Message}");
                }
                else
                {
                    JKLog.Log("����Ŀ¼���³ɹ�");
                    List<IResourceLocator> locatorList = updateCatalogsHandle.Result;
                    Addressables.Release(updateCatalogsHandle);
                    List<object> downloadKeys = new List<object>(1000);
                    foreach (IResourceLocator locator in locatorList)
                    {
                        downloadKeys.AddRange(locator.Keys);
                    }
                    SetLoadingWindow();
                    yield return DownLoadAllAssets(downloadKeys);
                    CloseLoadingWindow();
                }
            }
            else
            {
                JKLog.Log("�������");
            }
        }


        if (state == null) state = new HotUpdateSystemState();
        state.hotUpdateSucceed = succeed;
        SaveSystem.SaveSetting(state);

        if (succeed)
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssetmblies();
        }
        onEnd?.Invoke(succeed);
    }

    /// <summary>
    /// ���������ȸ�����Դ
    /// </summary>
    private IEnumerator DownLoadAllAssets(List<object> keys)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync((IEnumerable<object>)keys);
        yield return sizeHandle;
        if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            JKLog.Error($"GetDownloadSizeAsyncʧ��:{sizeHandle.OperationException.Message}");
        }
        else
        {
            long downloadSize = sizeHandle.Result;
            if (downloadSize > 0)
            {
                //ʵ�ʵ�����
                AsyncOperationHandle downloadDependenciesHandle = Addressables.DownloadDependenciesAsync((IEnumerable<object>)keys, Addressables.MergeMode.Union, false);
                //ѭ���鿴���ؽ���
                while (!downloadDependenciesHandle.IsDone)
                {
                    if (downloadDependenciesHandle.Status == AsyncOperationStatus.Failed)
                    {
                        succeed = false;
                        JKLog.Error($"DownloadDependenciesAsyncʧ��:{downloadDependenciesHandle.OperationException.Message}");
                        break;
                    }
                    //�ַ����ؽ���
                    float percentage = downloadDependenciesHandle.GetDownloadStatus().Percent;
                    onPercentageForEachFile?.Invoke(percentage);
                    UpdateLoadingWindowProgress(downloadSize * percentage, downloadSize);
                    yield return CoroutineTool.WaitForFrame();
                }
                if (downloadDependenciesHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    JKLog.Log($"ȫ���������");
                }
                Addressables.Release(downloadDependenciesHandle);
            }
        }
        Addressables.Release(sizeHandle);
    }

    /// <summary>
    /// ��ȡ�汾��Ϣ
    /// </summary>
    private string GetVersionInfo(LanguageType languageType)
    {
        Addressables.DownloadDependenciesAsync(versionInfoAddressableKey, true).WaitForCompletion();
        VersionInfo versionInfo = Addressables.LoadAssetAsync<VersionInfo>(versionInfoAddressableKey).WaitForCompletion();
        string info = versionInfo.GetVerisonData(languageType).info;
        Addressables.Release(versionInfo);
        return info;
    }

    /// <summary>
    /// �����ȸ��³���
    /// </summary>
    private void LoadHotUpdateDll()
    {
        for (int i = 0; i < hotUpdateDllNames.Length; i++)
        {
            TextAsset dllTextAsset = Addressables.LoadAssetAsync<TextAsset>(hotUpdateDllNames[i]).WaitForCompletion();
            System.Reflection.Assembly.Load(dllTextAsset.bytes);
            JKLog.Log($"����{hotUpdateDllNames[i]}����");
        }
    }

    /// <summary>
    /// ����AOT����
    /// </summary>
    private void LoadMetaForAOTAssetmblies()
    {
        for (int i = 0; i < aotDllAssets.Length; i++)
        {
            byte[] dllBytes = aotDllAssets[i].bytes;
            LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            JKLog.Log($"LoadMetaForAOTAssetmblies:{aotDllAssets[i].name}, {errorCode}");
        }
    }

    private LoadingWindow loadingWindow;
    private void ShowLoadingWindow()
    {
        loadingWindow = UISystem.Show<LoadingWindow>();
        loadingWindow.Set("Loading......");
    }

    private void SetLoadingWindow()
    {
        //���ݵ�ǰ����ȷ����������
        LanguageType languageType;
        GameBaseSetting baseSetting = SaveSystem.LoadSetting<GameBaseSetting>();
        if (baseSetting == null)
        {
            languageType = Application.systemLanguage == SystemLanguage.ChineseSimplified ? LanguageType.SimplifiedChinese : LanguageType.English;
        }
        else
        {
            languageType = baseSetting.LanguageType;
        }
        loadingWindow.Set(GetVersionInfo(languageType));
    }

    private void CloseLoadingWindow()
    {
        UISystem.Close<LoadingWindow>();
        loadingWindow = null;
    }

    private void UpdateLoadingWindowProgress(float current, float max)
    {
        loadingWindow.UpdateDownloadProgres(current, max);
    }
}
