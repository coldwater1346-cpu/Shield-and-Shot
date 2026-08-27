using UnityEngine;

namespace Shield_Shot.DataManagement.DataParsing
{
    public interface ITableDataFactory<T>
    {
        
        T Create(string[] header, string[] row);
    }
}