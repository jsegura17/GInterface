using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel; 

namespace GInterfaceCore.Core.Utils
{
    public class ExcelParse
    {
        public static void ProcessExcelFile(Stream excelFileStream, string nameFile)
        {
            // Guardar el archivo temporalmente en el disco
            string tempFilePath = Path.Combine(Path.GetTempPath(), nameFile);

            // Copiar el contenido del Stream al archivo temporal
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                excelFileStream.CopyTo(fileStream);
            }

            // Procesar el archivo Excel usando Interop
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(tempFilePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            List<string> AllValues = new List<string>();
            List<string> Dupes = new List<string>();

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            for (int i = 1; i <= rowCount; i++)
            {
                for (int j = 1; j <= colCount; j++)
                {
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                        AllValues.Add(xlRange.Cells[i, j].Value2.ToString());
                }
            }

            string targetCode = "PL2407034024";  // El código que estás buscando
            int count = 0;

            Dupes = AllValues
                .SkipWhile(val =>
                {
                    if (val == targetCode) count++;
                    return count < 2;
                })
                .ToList();

            foreach (string g in Dupes)
            {
                Console.WriteLine(g);
            }

            // Limpiar recursos
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            // Eliminar el archivo temporal
            File.Delete(tempFilePath);
        }

        // Simulación de la llamada desde otro contexto (API, servicio, etc.)
        public static async Task SimulateFileUpload(Stream uploadedFileStream, string nameFile)
        {
            // Procesar el archivo directamente desde el Stream sin ruta de archivo local
            ProcessExcelFile(uploadedFileStream, nameFile);
        }
    }
}
