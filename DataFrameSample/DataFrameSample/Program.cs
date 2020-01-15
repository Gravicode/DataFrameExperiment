using Microsoft.Data.Analysis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XPlot.Plotly;

namespace DataFrameSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            PrimitiveDataFrameColumn<DateTime> createdDate = new PrimitiveDataFrameColumn<DateTime>("CreatedDate");
            PrimitiveDataFrameColumn<float> temp = new PrimitiveDataFrameColumn<float>("Temp");
            PrimitiveDataFrameColumn<bool> status = new PrimitiveDataFrameColumn<bool>("Status",10);
            StringDataFrameColumn deviceName = new StringDataFrameColumn("DeviceName",10);
            StringDataFrameColumn actions = new StringDataFrameColumn("Actions", 10);
            StringDataFrameColumn factory = new StringDataFrameColumn("Factory", 10);

            

            Random rnd = new Random(Environment.TickCount);
            Enumerable.Range(1, 10).ToList().ForEach(x => { createdDate.Append(DateTime.Now.AddDays(x)); temp.Append(rnd.Next(25, 50)); deviceName[x-1] = $"device-{x}";factory[x - 1] = $"factory-{rnd.Next(1, 3)}"; });

            var df = new DataFrame(createdDate, deviceName, factory, temp, status, actions);
            df.Info();

            for(int row = 0; row < temp.Length; row++)
            {
                status[row] = temp[row] <= 30;
            }
            for (int row = 0; row < status.Length; row++)
            {
                if (!status[row].Value)
                    df[row, 5] = "device perlu di reset"; 
                        
            }
            df["Actions"].FillNulls("-", inPlace: true);

            DataTable dt = new DataTable("data sensor");
           
            foreach (var dc in df.Columns)
            {
                dt.Columns.Add(dc.Name.Replace(" ", "").Trim());
            }
            dt.AcceptChanges();

            for (long i = 0; i < df.Rows.Count; i++)
            {
                DataFrameRow row = df.Rows[i];
                var newRow = dt.NewRow();
                var cols = 0;
                foreach (var cell in row)
                {
                    newRow[cols] = cell.ToString();
                    cols++;
                }
                dt.Rows.Add(newRow);
            }
            dt.AcceptChanges();
            
            /*
            Formatter<DataTable>.Register((df, writer) =>
            {
                var headers = new List<IHtmlContent>();
                headers.Add(th(i("index")));
                foreach (DataColumn dc in df.Columns)
                {
                    headers.Add((IHtmlContent)th(dc.ColumnName));
                }
               
                var rows = new List<List<IHtmlContent>>();
                var take = 20;
                for (var i = 0; i < Math.Min(take, df.Rows.Count); i++)
                {
                    var cells = new List<IHtmlContent>();
                    cells.Add(td(i));
                    DataRow obj = df.Rows[i];
                    
                    for (int x = 0; x < df.Columns.Count;x++)
                    {
                        cells.Add(td(obj[x].ToString()));
                    }
                    
                   
                    rows.Add(cells);
                }

                var t = table(
                    thead(
                        headers),
                    tbody(
                        rows.Select(
                            r => tr(r))));

                writer.Write(t);
            }, "text/html");
            */
            PrimitiveDataFrameColumn<bool> boolFilter = df["Actions"].ElementwiseNotEquals("-");
            DataFrame filtered = df.Filter(boolFilter);
            
            GroupBy groupBy = df.GroupBy("Factory");
            
            DataFrame groupCounts = groupBy.Count();
            DataFrame tempGroupAvg = groupBy.Mean("Temp");

            var lineChart = Chart.Line(df.Rows.Select(g => new Tuple<DateTime, float>(Convert.ToDateTime(g[0]), Convert.ToSingle(g[3]))));
            lineChart.WithTitle("Temperature per Date");
            //display(lineChart);
        }
    }
}
