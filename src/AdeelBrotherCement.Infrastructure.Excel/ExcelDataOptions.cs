namespace AdeelBrotherCement.Infrastructure.Excel;

public class ExcelDataOptions
{
    public const string SectionName = "ExcelData";
    public string DataDirectory { get; set; } = "data";
    public string WorkbookFileName { get; set; } = "BusinessData.xlsx";
    public string WorkbookPath => Path.Combine(DataDirectory, WorkbookFileName);
}
