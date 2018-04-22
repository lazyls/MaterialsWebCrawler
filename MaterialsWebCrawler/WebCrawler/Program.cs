using Ivony.Html;
using Ivony.Html.Parser;
using ls.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WebCrawler
{
    class Program
    {
        public static Browser browser { get; set; }
        public static ConcurrentBag<Item> Items { get; set; }

        static void Main(string[] args)
        {
            login();
            get_items();
            get_detail();

            Console.WriteLine("Success");
        }

        static void get_items()
        {
            var msg_prefix = "-分析列表:";
            var y = Encoding.Default.GetByteCount(msg_prefix);
            Console.Write(msg_prefix);
            var charArr = @"-\|/".ToArray();

            var resp = browser.Get("https://materials.ulprospector.com/zh/results?pageSize=100");
            JumonyParser parser = new JumonyParser();
            var document = parser.Parse(resp);
            var total = document.FindFirst(".pagination.pull-right").InnerText().Split(' ').Last().ToInt();
            for (int i = 1; i < 2; i++)
            {
                resp = browser.Get("https://materials.ulprospector.com/zh/results?pageNum=" + i + "pageSize=100");
                document = parser.Parse(resp);
                var eles = document.Find("table.results tbody tr. ");
                foreach (var v in eles)
                {
                    var item = new Item();
                    item.Url = v.FindFirst(".entry a").Attribute("href").Value();
                    item.Id = v.FindFirst("input").Attribute("value").Value().ToInt();
                    Items.Add(item);
                }
                Console.CursorLeft = 0;
                Console.Write(charArr[i % charArr.Length]);
                Console.CursorLeft = y;
                Console.Write((int)(i * 100 / total) + "%");
            }
            Console.WriteLine();
        }

        static void get_detail()
        {
            var msg_prefix = "-分析详情:";
            var y = Encoding.Default.GetByteCount(msg_prefix);
            Console.Write(msg_prefix);
            var charArr = @"-\|/".ToArray();
            JumonyParser parser = new JumonyParser();

            var ItemArr = Items.ToArray();
            for (int i = 0; i < ItemArr.Length; i++)
            {
                var item = ItemArr[i];
                var resp = browser.Get("https://materials.ulprospector.com/zh/profile/default?e=" + item.Id);
                var document = parser.Parse(resp);
                item.Name = document.FindFirst(".pHdrName").InnerHtml();
                item.Title = document.FindFirst(".pHdrTitle").InnerHtml();
                item.Supplier = document.FindFirst(".supplierWeb").InnerHtml();
                item.SupplierWeb = document.FindFirst(".supplierWeb").Attribute("href").Value();
                item.Description = document.FindFirst(".productDescription").InnerHtml();

                resp = browser.Get("https://materials.ulprospector.com/pp.axd?CULTURE=zh&ID=TabProperties&A=LOAD&E=" + item.Id);
                dynamic json = new JavaScriptSerializer().DeserializeObject(resp);
                string html = json["Data"];
                document = parser.Parse(html);


                foreach (var v in document.Find("div.DSSEC"))
                {
                    if (v.Attribute("id").Value() == "DATAVIEW_DSSEC_GEN")
                    {
                        item.MainTables.GroupName = v.Attribute("title").Value();

                        Table<MainTableRow> table = null;
                        foreach (var vv in v.Find("table > tr"))
                        {
                            if (vv.Class().Any(a => a == "categoryheader"))
                            {
                                if (table != null)
                                    item.MainTables.Tables.Add(table);

                                table = new Table<MainTableRow>();
                                table.Caption = vv.FindFirstOrDefault(".catname").InnerText();
                            }
                            else
                            {
                                var tr = new MainTableRow();
                                tr.Key = vv.FindFirstOrDefault(".propname")?.InnerText();
                                foreach (var vvv in vv.Find("li"))
                                {
                                    var li = new NameUrl();
                                    li.Name = vvv.InnerText();
                                    li.Url = vvv.FindFirstOrDefault("a")?.Attribute("href").Value();
                                    tr.Values.Add(li);
                                }
                                table.Rows.Add(tr);
                            }
                        }
                        if (table != null)
                            item.MainTables.Tables.Add(table);
                    }
                    else
                    {
                        var group = new TableGroup<PropertyTableRow>();
                        group.GroupName = v.Attribute("title").Value();

                        Table<PropertyTableRow> table = null;
                        foreach (var vv in v.Find("table > tr"))
                        {
                            if (vv.Class().Any(a => a == "categoryheader"))
                            {
                                if (table != null)
                                    group.Tables.Add(table);

                                table = new Table<PropertyTableRow>();
                                table.Caption = vv.FindFirstOrDefault(".catname")?.InnerText();
                            }
                            else
                            {
                                var tr = new PropertyTableRow();
                                tr.Name = vv.FindFirstOrDefault(".propname")?.InnerText();
                                tr.Value = vv.FindFirstOrDefault(".dsvalue")?.InnerText();
                                tr.Unit = vv.FindFirstOrDefault(".dsunit")?.InnerText();
                                tr.TestMethod = vv.FindFirstOrDefault(".standard")?.InnerText();
                                table.Rows.Add(tr);
                            }
                        }
                        if (table != null)
                            group.Tables.Add(table);
                        item.PropertyTablesList.Add(group);
                    }
                }

                Console.CursorLeft = 0;
                Console.Write(charArr[i % charArr.Length]);
                Console.CursorLeft = y;
                Console.Write((int)(i * 100 / Items.Count) + "%");

                System.IO.File.WriteAllText(@"result\"+item.Id.ToString() + ".json", new JavaScriptSerializer().Serialize(item));
            }

            Console.WriteLine();
        }

        static void login()
        {
            browser.Post("https://www.ulprospector.com/session", new Dictionary<string, string> { { "email", "chenchao@shsunny.com" }, { "password", "524524Cc" }, { "rememberMe", "true" } });
            browser.Get("https://materials.ulprospector.com/zh/search/basic?SET=FASTLOOKUP&A=RESET");
        }
        static Program()
        {
            Items = new ConcurrentBag<Item>();

            browser = new Browser();
            browser.Timeout = 1000 * 60 * 60;
        }
    }
}
