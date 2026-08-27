using UnityEngine;


namespace Shield_Shot.DataManagement.DataParsing
{
    public  static class CSVReader 
    {
       public static string Read(string filePath)
        {
            TextAsset csvFile = Resources.Load<TextAsset>(filePath);


            if ( csvFile ==null)
            {
                Debug.Log($"csv파일을 찾을 수 없습니다. {filePath}");
                return null;
            }

            return csvFile.text;

        }
    }
}