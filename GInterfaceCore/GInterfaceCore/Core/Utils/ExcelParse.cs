using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace GInterfaceCore.Core.Utils
{
    public class ExcelParse
    {
        public static async Task ProcessExcelFileAsync(Stream excelFileStream, string nameFile, int headers, string startKeyword, string endKeyword)
        {
            string customDirectory = @"C:\Apps\Genesis\Ginterface\Data";
            string tempFilePath = Path.Combine(customDirectory, nameFile);

            try
            {
                // Copiar el contenido del Stream al archivo temporal
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await excelFileStream.CopyToAsync(fileStream);
                }
            }
            catch (IOException ioEx)
            {
                // Manejar excepciones relacionadas con E/S (entrada/salida)
                Console.WriteLine($"Error al escribir el archivo: {ioEx.Message}");
                return;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                // Manejar excepciones relacionadas con permisos de acceso
                Console.WriteLine($"Acceso denegado al archivo: {uaEx.Message}");
                return;
            }
            catch (Exception ex)
            {
                // Manejar cualquier otra excepción
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return;
            }

            // Procesar el archivo Excel usando Interop (sincrónico)
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(tempFilePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            List<string> AllValues = new List<string>();
            List<string> DataAfterEnd = new List<string>();

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            // Recorre todas las celdas y almacena los valores en la lista AllValues
            for (int i = 1; i <= rowCount; i++)
            {
                for (int j = 1; j <= colCount; j++)
                {
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        string cellValue = xlRange.Cells[i, j].Value2.ToString();
                        AllValues.Add(cellValue);
                    }
                }
            }

            // Usamos LINQ para capturar los datos después de la palabra clave de fin
            bool foundEnd = false;

            DataAfterEnd = AllValues
                .SkipWhile(val =>
                {
                    if (val.Equals(endKeyword, StringComparison.OrdinalIgnoreCase))
                    {
                        foundEnd = true;
                    }
                    return !foundEnd;
                })
                .ToList();

            // Dividir los datos en sub-arrays con longitud igual al número de encabezados
            var result = DataAfterEnd
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / headers)
                .Select(g => g.Select(x => x.value).ToArray())
                .ToArray();

            // Imprimir los sub-arrays
            foreach (var array in result)
            {
                Console.WriteLine($"Array de tamaño {headers}:");
                foreach (var item in array)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine();
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
        public async Task SimulateFileUpload(Stream uploadedFileStream, string nameFile, int headers, string startKeyword, string endKeyword)
        {
            // Procesar el archivo directamente desde el Stream sin ruta de archivo local
           await ProcessExcelFileAsync(uploadedFileStream, nameFile, headers, startKeyword, endKeyword);
        }
    }
}
