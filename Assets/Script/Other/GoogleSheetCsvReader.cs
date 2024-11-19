using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GoogleSheetCsvReader : MonoBehaviour
{
    // URL CSV với GID của sheet bạn muốn lấy dữ liệu
    string baseUrl = "https://docs.google.com/spreadsheets/d/12u7YGB4HeKG5tHKzYGyROAPCT-qMkaJg7b3FmpSI9nw/export?format=csv&gid=";

    void Start()
    {
        // Lấy dữ liệu từ sheet với GID cụ thể
        StartCoroutine(ReadGoogleSheetCsv("514089300"));  // GID cho sheet đầu tiên thường là 0, thay đổi nếu cần
    }

    IEnumerator ReadGoogleSheetCsv(string gid)
    {
        string csvUrl = baseUrl + gid;

        UnityWebRequest www = UnityWebRequest.Get(csvUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string csvData = www.downloadHandler.text;
            ProcessCsvData(csvData);
        }
    }

    void ProcessCsvData(string csvData)
    {
        string[] lines = csvData.Split('\n');

        foreach (string line in lines)
        {
            Debug.Log(line);
        }
    }
}
