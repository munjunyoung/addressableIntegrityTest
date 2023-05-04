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
    /// NOTE : �󺧺� �ٿ�ε� ����
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
    /// NOTE : ��� ĳ�� Ŭ����, īŻ�α׵� ����
    /// </summary>
    /// <param name="clearCatalog"></param>
    public void OnClearAllCache(bool clearCatalog = false)
    {
        Debug.Log("Click Clear Cache");
        AssetBundle.UnloadAllAssetBundles(true);

        if (Caching.ClearCache())
        {
            //���忡�� Ȯ�ο뵵�� ���Ͽ� ����
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
                //ĳ�� Ŭ����
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OnIntegrityBundles()
    {
        //�ε�� īŻ�α� ��� ��� Ű�� �˻��Ͽ� bundle version hash ����
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
        //��� ���� �ؽ��� 
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
