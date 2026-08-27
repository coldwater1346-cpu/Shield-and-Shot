using UnityEngine;


namespace Shield_Shot.DataManagement.DataParsing
{
    public interface ITableData
    {
        string ItemID { get; }

        void LinkData(ItemDataParsingManager manager);
    }
}
