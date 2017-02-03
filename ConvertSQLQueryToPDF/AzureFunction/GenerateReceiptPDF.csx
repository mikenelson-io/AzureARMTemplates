#load "OrderViewModel.csx"
#r "PdfRpt.dll"
#r "itextsharp.dll"
#r "itextsharp.xmlworker.dll"
#r "itextsharp.pdfa.dll"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using System.Reflection;
using System.IO;
using iTextSharp;
using iTextSharp.text;

public static byte[] CreatePdfReport(OrderViewModel order)
{
    var pdfReportData = new PdfReport().DocumentPreferences(doc =>
    {
        doc.RunDirection(PdfRunDirection.LeftToRight);
        doc.Orientation(PageOrientation.Portrait);
        doc.PageSize(PdfPageSize.A4);
        doc.DocumentMetadata(new DocumentMetadata { Author = "Contoso Sports League", Application = "Contoso.Apps.SportsLeague", Subject = "Contoso Sports League Store Receipt", Title = "Receipt" });
        doc.Compression(new CompressionSettings
        {
            EnableCompression = true,
            EnableFullCompression = true
        });
    })
    .DefaultFonts(fonts =>
    {
        fonts.Path(Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\arial.ttf",
                            Environment.GetEnvironmentVariable("SystemRoot") + "\\fonts\\verdana.ttf");
    })
    .PagesFooter(footer =>
    {
        footer.DefaultFooter(DateTime.Now.ToString("MM/dd/yyyy"));
    })
    .PagesHeader(header =>
    {
        header.CacheHeader(cache: true); // It's a default setting to improve the performance.
        header.DefaultHeader(defaultHeader =>
        {
            defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
            defaultHeader.Message("Contoso Sports League - Purchase Receipt");
        });
    })
    .MainTableTemplate(template =>
    {
        template.BasicTemplate(BasicTemplate.ClassicTemplate);
    })
    .MainTablePreferences(table =>
    {
        table.ColumnsWidthsType(TableColumnWidthType.Relative);
        table.NumberOfDataRowsPerPage(5);
    })
    .MainTableDataSource(dataSource =>
    {
        var listOfRows = new List<OrderViewModel>();
        listOfRows.Add(order);
        dataSource.StronglyTypedList(listOfRows);
    })
    .MainTableSummarySettings(summarySettings =>
    {
        summarySettings.OverallSummarySettings("Summary");
        summarySettings.PreviousPageSummarySettings("Previous Page Summary");
        summarySettings.PageSummarySettings("Page Summary");
    })
    .MainTableColumns(columns =>
    {
        columns.AddColumn(column =>
        {
            column.PropertyName("rowNo");
            column.IsRowNumber(true);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(0);
            column.Width(1);
            column.HeaderCell("#");
        });

        columns.AddColumn(column =>
        {
            column.PropertyName<OrderViewModel>(x => x.OrderId);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(1);
            column.Width(2);
            column.HeaderCell("OrderId");
        });

        columns.AddColumn(column =>
        {
            column.PropertyName<OrderViewModel>(x => x.FirstName);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(2);
            column.Width(3);
            column.HeaderCell("First Name");
        });

        columns.AddColumn(column =>
        {
            column.PropertyName<OrderViewModel>(x => x.LastName);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(3);
            column.Width(3);
            column.HeaderCell("Last Name");
        });
    
        columns.AddColumn(column =>
        {
            column.PropertyName<OrderViewModel>(x => x.Total);
            column.CellsHorizontalAlignment(HorizontalAlignment.Center);
            column.IsVisible(true);
            column.Order(4);
            column.Width(4);
            column.HeaderCell("Total");
        });
    })
    .MainTableEvents(events =>
    {
        events.DataSourceIsEmpty(message: "There is no data available to display.");
    })
    .Generate(data => data.AsPdfStream(new MemoryStream()));

    return ((MemoryStream)pdfReportData.PdfStreamOutput).ToArray();

}
