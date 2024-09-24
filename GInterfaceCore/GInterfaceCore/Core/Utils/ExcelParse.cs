using ClosedXML.Excel;
using Microsoft.SqlServer.Server;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Excel = Microsoft.Office.Interop.Excel;

namespace GInterfaceCore.Core.Utils
{
    public class ExcelParse
    {
        public static async Task<List<List<string>>> ProcessExcelFileAsync2(Stream excelFileStream, string nameFile, int headers, string startKeyword, string endKeyword, string headerBase)
        {
            string customDirectory = @"C:\Apps\Genesis\Ginterface\Data";
            string tempFilePath = Path.Combine(customDirectory, nameFile);

            try
            {
                // Copiar el contenido del Stream al archivo temporal
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await excelFileStream.CopyToAsync(fileStream);
                    Log.Information("------------------------------------");
                    Log.Information($"DATA File created->#:{fileStream.Name}");
                    Log.Information("------------------------------------");
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
            Excel.Application xlApp = null;
            Excel.Workbook xlWorkbook = null;
            Excel._Worksheet xlWorksheet = null;
            Excel.Range xlRange = null;

            List<List<string>> result = new List<List<string>>();
            try
            {
                xlApp = new Excel.Application();
                Log.Information("------------------------------------");
                Log.Information("DATA Excel instanciated");
                Log.Information("------------------------------------");
                //xlWorkbook = xlApp.Workbooks.Open(tempFilePath);
                xlWorkbook = xlApp.Workbooks.Open(tempFilePath,
                    0, false, 5, "", "", false, Excel.XlPlatform.xlWindows, "",
                    true, false, 0, true, false, false);
                Log.Information("------------------------------------");
                Log.Information("DATA Excel Open");
                Log.Information("------------------------------------");
                xlWorksheet = (Excel._Worksheet)xlWorkbook.Sheets[1];
                Log.Information("------------------------------------");
                Log.Information("DATA Excel _Worksheet");
                Log.Information("------------------------------------");
                xlRange = xlWorksheet.UsedRange;

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
                        var cell = (Excel.Range)xlRange.Cells[i, j];
                        if (cell != null && cell.Value2 != null)
                        {
                            string cellValue = cell.Value2.ToString();
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
                                        var row = (Excel.Range)xlRange.Cells[i, k];
                                        
                                        if (row != null && row.Value2 != null)
                                        {
                                            string cellRowValue = row.Value2.ToString();
                                            rowData.Add(cellRowValue);
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
                Log.Information("------------------------------------");
                Log.Information($"DATA AllValues->#:{AllValues.Count}");
                Log.Information("------------------------------------");

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
                result = DataAfterEnd
                    .Select((value, index) => new { value, index })
                    .GroupBy(x => x.index / headers)
                    .Select(g => g.Select(x => x.value).ToList()) // Convertir a List<string>
                    .ToList(); // Cambiar a List<List<string>> para facilitar su manejo

                // Agregar los datos de headerBase al principio de la lista
                foreach (var headerRow in HeaderBaseData)
                {
                    result.Insert(0, headerRow);
                }
            }
            catch (COMException comEx)
            {
                var error = $"Error de Excel Interop: {comEx.Message}";
                Console.WriteLine(error);
                Log.Fatal("------------------------------------");
                Log.Fatal(error);
                Log.Fatal("------------------------------------");
                return null;
            }
            catch (Exception ex)
            {
                var error = $"Error inesperado durante el procesamiento de Excel: {ex.Message}";
                Console.WriteLine(error);
                Log.Fatal("------------------------------------");
                Log.Fatal(error);
                Log.Fatal("------------------------------------");
                return null;
            }
            finally
            {
                // Limpiar recursos
                if (xlRange != null) Marshal.ReleaseComObject(xlRange);
                if (xlWorksheet != null) Marshal.ReleaseComObject(xlWorksheet);
                if (xlWorkbook != null)
                {
                    xlWorkbook.Close(false);
                    Marshal.ReleaseComObject(xlWorkbook);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                    Marshal.ReleaseComObject(xlApp);
                }

                // Eliminar el archivo temporal
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Log.Information("------------------------------------");
            Log.Information($"DATA -> result #:{result.Count}");
            Log.Information("------------------------------------");

            // Retornar la lista de listas de strings
            return result;
        }
    

        public static async Task<List<List<string>>> ProcessExcelFileAsync(Stream excelFileStream, string nameFile, int headers, string startKeyword, string endKeyword, string headerBase)
        {
            List<List<string>> result = new List<List<string>>();

            string customDirectory = @"C:\Apps\Genesis\Ginterface\Data";
            string tempFilePath = Path.Combine(customDirectory, nameFile);

            try
            {
                // Copiar el contenido del Stream al archivo temporal
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                {
                    await excelFileStream.CopyToAsync(fileStream);
                    Log.Information("------------------------------------");
                    Log.Information($"DATA File created->#:{fileStream.Name}");
                    Log.Information("------------------------------------");
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

            // Abrir el archivo de Excel
            using (var workbook = new XLWorkbook(tempFilePath))
            {
                // Obtener la primera hoja del archivo
                var worksheet = workbook.Worksheet(1);

                // Leer el contenido de una celda específica
                string value = worksheet.Cell("A1").Value.ToString();
                Console.WriteLine($"El valor de la celda A1 es: {value}");

                // Leer un rango de celdas
                var range = worksheet.Range("A1:B2");

                foreach (var cell in range.Cells())
                {
                    Console.WriteLine($"El valor de la celda {cell.Address} es: {cell.Value}");
                }

                // Leer todas las celdas usadas en una hoja
                foreach (var row in worksheet.RowsUsed())
                {
                    foreach (var cell in row.CellsUsed())
                    {
                        Console.WriteLine($"El valor de la celda {cell.Address} es: {cell.Value}");
                    }
                }
            }

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
