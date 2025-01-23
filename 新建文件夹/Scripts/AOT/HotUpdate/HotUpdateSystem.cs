
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
/// 热更新系统
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
    /// 更新Addressable的目录资源
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoUpdateAddressables()
    {
        //确定上一次热更新的状态
        HotUpdateSystemState state = SaveSystem.LoadSetting<HotUpdateSystemState>();
        if (state == null || !state.hotUpdateSucceed)
        {
            Debug.Log("断点续传");
            string catalogPath = $"{Application.persistentDataPath}/com.unity.addressables";
            print("xxxxxx  path " + catalogPath);
            if (Directory.Exists(catalogPath)) Directory.Delete(catalogPath, true);
        }

        //初始化
        yield return Addressables.InitializeAsync();
        succeed = true;

        //检测目录更新
        var checkForCatalogUpdateHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkForCatalogUpdateHandle;

        if (checkForCatalogUpdateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            JKLog.Error($"CheckForCatalogUpdate失败:{checkForCatalogUpdateHandle.OperationException.Message}");
            Addressables.Release(checkForCatalogUpdateHandle);
        }
        else
        {
            List<string> catalogResult = checkForCatalogUpdateHandle.Result;
            Addressables.Release(checkForCatalogUpdateHandle);

            Debug.Log("xxxxxx  calogcount  " + catalogResult.Count);
            //下载最新的目录
            if (catalogResult.Count > 0)
            {
                ShowLoadingWindow();
                var updateCatalogsHandle = Addressables.UpdateCatalogs(catalogResult, false);
                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    succeed = false;
                    Addressables.Release(updateCatalogsHandle);
                    JKLog.Error($"UpdateCatalogs失败:{updateCatalogsHandle.OperationException.Message}");
                }
                else
                {
                    JKLog.Log("下载目录更新成功");
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
                JKLog.Log("无需更新");
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
    /// 下载所有热更新资源
    /// </summary>
    private IEnumerator DownLoadAllAssets(List<object> keys)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync((IEnumerable<object>)keys);
        yield return sizeHandle;
        if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            JKLog.Error($"GetDownloadSizeAsync失败:{sizeHandle.OperationException.Message}");
        }
        else
        {
            long downloadSize = sizeHandle.Result;
            if (downloadSize > 0)
            {
                //实际的下载
                AsyncOperationHandle downloadDependenciesHandle = Addressables.DownloadDependenciesAsync((IEnumerable<object>)keys, Addressables.MergeMode.Union, false);
                //循环查看下载进度
                while (!downloadDependenciesHandle.IsDone)
                {
                    if (downloadDependenciesHandle.Status == AsyncOperationStatus.Failed)
                    {
                        succeed = false;
                        JKLog.Error($"DownloadDependenciesAsync失败:{downloadDependenciesHandle.OperationException.Message}");
                        break;
                    }
                    //分发下载进度
                    float percentage = downloadDependenciesHandle.GetDownloadStatus().Percent;
                    onPercentageForEachFile?.Invoke(percentage);
                    UpdateLoadingWindowProgress(downloadSize * percentage, downloadSize);
                    yield return CoroutineTool.WaitForFrame();
                }
                if (downloadDependenciesHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    JKLog.Log($"全部下载完成");
                }
                Addressables.Release(downloadDependenciesHandle);
            }
        }
        Addressables.Release(sizeHandle);
    }

    /// <summary>
    /// 获取版本信息
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
    /// 加载热更新程序集
    /// </summary>
    private void LoadHotUpdateDll()
    {
        for (int i = 0; i < hotUpdateDllNames.Length; i++)
        {
            TextAsset dllTextAsset = Addressables.LoadAssetAsync<TextAsset>(hotUpdateDllNames[i]).WaitForCompletion();
            System.Reflection.Assembly.Load(dllTextAsset.bytes);
            JKLog.Log($"加载{hotUpdateDllNames[i]}程序集");
        }
    }

    /// <summary>
    /// 加载AOT程序集
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
        //根据当前设置确定语言类型
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
