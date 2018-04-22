using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    class Item
    {
        public string Url { get; set; }
        public int Id { get; set; }

        public string Name { get; set; }
        public string Title { get; set; }
        public string Supplier { get; set; }
        public string SupplierWeb { get; set; }
        public string Description { get; set; }

        public TableGroup<MainTableRow> MainTables = new TableGroup<MainTableRow>();
        public List<TableGroup<PropertyTableRow>> PropertyTablesList = new List<TableGroup<PropertyTableRow>>();
    }

    
    class NameUrl
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    class MainTableRow
    {
        public string Key { get; set; }
        public List<NameUrl> Values { get; set; }
        public MainTableRow()
        {
            Values = new List<NameUrl>();
        }
    }

    class PropertyTableRow
    {
        public string Name { get; set; }
        /// <summary>
        /// 额定值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 测试方法
        /// </summary>
        public string TestMethod { get; set; }
    }

    class Table<TRow>
    {
        public string Caption { get; set; }
        public List<TRow> Rows { get; set; }
        public Table()
        {
            Rows = new List<TRow>();
        }
    }

    class TableGroup<TRow>
    {
        public string GroupName { get; set; }
        public List<Table<TRow>> Tables { get; set; }
        public TableGroup()
        {
            Tables = new List<Table<TRow>>();
        }
    }
}
