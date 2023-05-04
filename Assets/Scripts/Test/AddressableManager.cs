using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using System.IO;
using UnityEngine.ResourceManagement.ResourceLocations;

using System;
using UnityEngine.UI;

using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Linq;

public class AddressableManager : MonoBehaviour
{
    string[] downLoadLabels = { "default" };
    private Coroutine _downLoadAsyncCor = null;

    public Text viewText;

    public Button donwLoadButton;
    public Button integrityButton;
    public Button loadButton;
    public Button releaseButton;
    public Button clearBundleButton;

    public Image loadedImage;



    private void Awake()
    {
        donwLoadButton.onClick.RemoveAllListeners();
        donwLoadButton.onClick.AddListener(() => OnStartDownLoadBundle());
        clearBundleButton.onClick.RemoveAllListeners();
        clearBundleButton.onClick.AddListener(() => OnClearAllCache());

        loadButton.onClick.RemoveAllListeners();
        loadButton.onClick.AddListener(() => OnLoadImage());

        releaseButton.onClick.RemoveAllListeners();
        releaseButton.onClick.AddListener(() => OnReleaseImage());
    }
    /// <summary>
    /// NOTE : 라벨별 다운로드 시작
    /// </summary>
    /// <param name="key"></param>
    public void OnStartDownLoadBundle()
    {
        if (_downLoadAsyncCor != null)
        {
            StopCoroutine(_downLoadAsyncCor);
            _downLoadAsyncCor = null;
        }
        _downLoadAsyncCor = StartCoroutine(DownloadProcess(downLoadLabels));
    }

    IEnumerator DownloadProcess(string[] labels)
    {
        int count = 0;
        while (count < labels.Length)
        {
            bool isSucced = false;
            var label = labels[count];

            var downHandle = Addressables.DownloadDependenciesAsync(label.ToString());
            downHandle.Completed += (AsyncOperationHandle Handle) =>
            {
                if (Handle.Status == AsyncOperationStatus.Succeeded)
                {
                    viewText.text = downHandle.GetDownloadStatus().Percent * 100 + "%";
                    //popupUI.SetDownloading(label.ToString(), downHandle.GetDownloadStatus().Percent * 100, count, labels.Count);
                    isSucced = true;
                }
            };
            while (!downHandle.IsDone)
            {
                viewText.text = downHandle.GetDownloadStatus().Percent * 100 + "%";
                yield return new WaitForFixedUpdate();
            }

            yield return isSucced;
            Addressables.Release(downHandle);
            count++;
        }
        viewText.text = 100 + "%";
    }


    /// <summary>
    /// NOTE : 모든 캐시 클리어, 카탈로그도 삭제
    /// </summary>
    /// <param name="clearCatalog"></param>
    public void OnClearAllCache(bool clearCatalog = false)
    {
        Debug.Log("Click Clear Cache");
        AssetBundle.UnloadAllAssetBundles(true);

        if (Caching.ClearCache())
        {
            //빌드에서 확인용도를 위하여 변경
            Debug.Log("Success Clear Cache");
        }
        else
        {
            Debug.Log("Fail Clear Cache");
        }

        try
        {
            if (clearCatalog)
            {
                //Catalog clear
                Debug.Log("clear cache at " + Application.persistentDataPath);
                var path = $"{Application.persistentDataPath}/com.unity.addressables";
                //foreach (var item in list)
                //{
                Debug.Log("Delete" + " " + path);
                Directory.Delete(path, true);
                //}
                //캐싱 클리어
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OnIntegrityBundles()
    {
        //로드된 카탈로그 기반 모든 키를 검색하여 bundle version hash 저장
        var bundlesHash = new HashSet<string>();
        foreach (var loc in Addressables.ResourceLocators)
        {
            foreach (var key in loc.Keys)
            {
                if (!loc.Locate(key, typeof(object), out var resourceLocations))
                    continue;

                foreach (var d in resourceLocations.Where(l => l.HasDependencies).SelectMany(l => l.Dependencies))
                {
                    if (d.Data is AssetBundleRequestOptions dependencyBundle)
                        bundlesHash.Add(dependencyBundle.Hash);
                }
            }
        }
        //모든 번들 해쉬셋 
    }

    public void OnLoadImage()
    {

        Debug.Log("Start Load");
        AsyncOperationHandle handle = default;

        handle = Addressables.LoadAssetAsync<Sprite>("Assets/Resources_moved/Test/100001.png");

        Sprite output = handle.WaitForCompletion() as Sprite;
        loadedImage.sprite = output;
        loadedImage.SetNativeSize();
        
    }

    public void OnReleaseImage()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }
}
