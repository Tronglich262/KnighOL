using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ItemStatsExporter : MonoBehaviour
{
    [ContextMenu("Export All ItemStats to JSON and Upload")]
    public void ExportAndUpload()
    {
        ItemStats[] allItems = Resources.LoadAll<ItemStats>("ItemStats");
        List<ItemStatsDTO> dtoList = new List<ItemStatsDTO>();

        foreach (var item in allItems)
        {
            dtoList.Add(item.ToDTO());
        }

        // Tự tạo JSON dạng mảng bằng cách ghép chuỗi
        List<string> jsonItems = new List<string>();
        foreach (var dto in dtoList)
        {
            jsonItems.Add(JsonUtility.ToJson(dto));
        }

        string json = "[\n" + string.Join(",\n", jsonItems) + "\n]";
        File.WriteAllText(Application.dataPath + "/ItemStatsExport.json", json);
        Debug.Log("Đã xuất file ItemStatsExport.json");

        StartCoroutine(UploadToServer(json));
    }

    IEnumerator UploadToServer(string json)
    {
        string apiUrl = "https://localhost:7124/api/item/upload"; // ← Thay bằng URL thật của bạn

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("🎉 Upload thành công!");
        }
        else
        {
            Debug.LogError("❌ Upload thất bại: " + request.error + "\n" + request.downloadHandler.text);
        }
    }
}
