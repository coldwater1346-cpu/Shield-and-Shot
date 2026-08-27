using Shield_Shot.DataManagement.DataParsing;
using System.Collections.Generic;
using UnityEngine;

namespace Shield_Shot.DataManagement.InventorySystem
{
    public class DataTest : MonoBehaviour
    {
        private void Start()
        {
            string csvText = CSVReader.Read("Data/ItemDataTable");

            List<string[]> rows = CSVParser.Parse(csvText);

            foreach (string[] row in rows)
            {
                Debug.Log(string.Join(" | ", row));
            }
        }
    }
}