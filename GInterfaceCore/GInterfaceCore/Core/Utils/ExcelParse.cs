using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace GInterfaceCore.Core.Utils
{
    public class ExcelParse
    {
        public static async Task<List<List<string>>> ProcessExcelFileAsync(Stream excelFileStream, string nameFile, int headers, string startKeyword, string endKeyword, string headerBase)
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
                Console.WriteLine($"Error al escribir el archivo: {ioEx.Message}");
                return null;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Acceso denegado al archivo: {uaEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                return null;
            }

            // Procesar el archivo Excel usando Interop (sincrónico)
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(tempFilePath);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            List<string> AllValues = new List<string>();
            List<string> DataAfterEnd = new List<string>();
            List<List<string>> HeaderBaseData = new List<List<string>>(); // Lista para almacenar las filas de cada valor de headerBase

            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            // Dividir los valores de headerBase en caso de que vengan separados por comas
            string[] headerBaseKeywords = headerBase.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(hb => hb.Trim()) // Quitar espacios en blanco
                                                    .ToArray();

            // Recorre todas las celdas y almacena los valores en la lista AllValues
            for (int i = 1; i <= rowCount; i++)
            {
                for (int j = 1; j <= colCount; j++)
                {
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        string cellValue = xlRange.Cells[i, j].Value2.ToString();
                        AllValues.Add(cellValue);

                        // Buscar las filas que contengan algún valor de headerBase
                        foreach (var headerKeyword in headerBaseKeywords)
                        {
                            if (cellValue.Equals(headerKeyword, StringComparison.OrdinalIgnoreCase))
                            {
                                // Recolectar toda la fila de datos para ese valor de headerBase
                                List<string> rowData = new List<string>();
                                for (int k = 1; k <= colCount; k++)
                                {
                                    if (xlRange.Cells[i, k] != null && xlRange.Cells[i, k].Value2 != null)
                                    {
                                        rowData.Add(xlRange.Cells[i, k].Value2.ToString());
                                    }
                                }
                                HeaderBaseData.Add(rowData); // Agregar la fila completa a la lista
                            }
                        }
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
                .Select(g => g.Select(x => x.value).ToList()) // Convertir a List<string>
                .ToList(); // Cambiar a List<List<string>> para facilitar su manejo

            // Agregar los datos de headerBase al principio de la lista
            foreach (var headerRow in HeaderBaseData)
            {
                result.Insert(0, headerRow);
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

            // Retornar la lista de listas de strings
            return result;
        }

        // Simulación de la llamada desde otro contexto (API, servicio, etc.)
        public async Task<List<List<string>>> SimulateFileUpload(Stream uploadedFileStream, string nameFile, int headers, string startKeyword, string endKeyword, string headerBase)
        {
            // Procesar el archivo directamente desde el Stream sin ruta de archivo local
            return await ProcessExcelFileAsync(uploadedFileStream, nameFile, headers, startKeyword, endKeyword, headerBase);
        }
    }
}
