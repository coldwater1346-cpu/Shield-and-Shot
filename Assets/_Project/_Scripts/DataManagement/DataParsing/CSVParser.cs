using UnityEngine;
using System.Collections.Generic;

namespace Shield_Shot.DataManagement.DataParsing
{
    public static class CSVParser
    {
        public static List<string[]> Parse(string csvText)
        {
            List<string[]> rows = new List<string[]>();

            if (string.IsNullOrEmpty(csvText))
            {
                Debug.Log("CSV 텍스트가 비어있습니다.");
                return rows;
            }

            string[] lines = csvText.Split(new[] {'\n', '\r'}, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] columns = line.Split(',');

                for (int i=0; i<columns.Length; i++)
                {
                    columns[i] = columns[i].Trim();
                }

                rows.Add(columns);
            }

            return rows;
        }
    }
}