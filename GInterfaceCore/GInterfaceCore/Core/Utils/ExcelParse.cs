using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Office.Interop.Excel;
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
        //public static async Task<List<List<string>>> ProcessExcelFileAsync2(Stream excelFileStream, string nameFile, int headers, string startKeyword, string endKeyword, string headerBase)
        //{
        //    string customDirectory = @"C:\Apps\Genesis\Ginterface\Data";
        //    string tempFilePath = Path.Combine(customDirectory, nameFile);

        //    try
        //    {
        //        // Copiar el contenido del Stream al archivo temporal
        //        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        //        {
        //            await excelFileStream.CopyToAsync(fileStream);
        //            Log.Information("------------------------------------");
        //            Log.Information($"DATA File created->#:{fileStream.Name}");
        //            Log.Information("------------------------------------");
        //        }


        //    }
        //    catch (IOException ioEx)
        //    {
        //        Console.WriteLine($"Error al escribir el archivo: {ioEx.Message}");
        //        return null;
        //    }
        //    catch (UnauthorizedAccessException uaEx)
        //    {
        //        Console.WriteLine($"Acceso denegado al archivo: {uaEx.Message}");
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error inesperado: {ex.Message}");
        //        return null;
        //    }

        //    // Procesar el archivo Excel usando Interop (sincrónico)
        //    Excel.Application xlApp = null;
        //    Excel.Workbook xlWorkbook = null;
        //    Excel._Worksheet xlWorksheet = null;
        //    Excel.Range xlRange = null;

        //    List<List<string>> result = new List<List<string>>();
        //    try
        //    {
        //        xlApp = new Excel.Application();
        //        Log.Information("------------------------------------");
        //        Log.Information("DATA Excel instanciated");
        //        Log.Information("------------------------------------");
        //        //xlWorkbook = xlApp.Workbooks.Open(tempFilePath);
        //        xlWorkbook = xlApp.Workbooks.Open(tempFilePath,
        //            0, false, 5, "", "", false, Excel.XlPlatform.xlWindows, "",
        //            true, false, 0, true, false, false);
        //        Log.Information("------------------------------------");
        //        Log.Information("DATA Excel Open");
        //        Log.Information("------------------------------------");
        //        xlWorksheet = (Excel._Worksheet)xlWorkbook.Sheets[1];
        //        Log.Information("------------------------------------");
        //        Log.Information("DATA ClosedXML Worksheet");
        //        Log.Information("------------------------------------");

        //        List<string> AllValues = new List<string>();
        //        List<string> DataAfterEnd = new List<string>();
        //        List<List<string>> HeaderBaseData = new List<List<string>>(); // Lista para almacenar las filas de cada valor de headerBase

        //        // Dividir los valores de headerBase en caso de que vengan separados por comas
        //        string[] headerBaseKeywords = headerBase.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        //                                                .Select(hb => hb.Trim()) // Quitar espacios en blanco
        //        .ToArray();

        //        var rows = worksheet.RangeUsed().RowsUsed(); // Obtiene solo las filas usadas

        //        // Recorre todas las celdas y almacena los valores en la lista AllValues
        //        foreach (var row in rows)
        //        {
        //            foreach (var cell in row.CellsUsed())
        //            {
        //                string cellValue = cell.GetValue<string>();
        //                AllValues.Add(cellValue);

        //                // Buscar las filas que contengan algún valor de headerBase
        //                foreach (var headerKeyword in headerBaseKeywords)
        //                {
        //                    if (cellValue.Equals(headerKeyword, StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        // Recolectar toda la fila de datos para ese valor de headerBase
        //                        List<string> rowData = new List<string>();
        //                        foreach (var rowCell in row.CellsUsed())
        //                        {
        //                            string cellRowValue = rowCell.GetValue<string>();
        //                            rowData.Add(cellRowValue);
        //                        }
        //                        HeaderBaseData.Add(rowData); // Agregar la fila completa a la lista
        //                    }
        //                }
        //            }
        //        }

        //        // Usamos LINQ para capturar los datos después de la palabra clave de fin
        //        bool foundEnd = false;
        //        Log.Information("------------------------------------");
        //        Log.Information($"DATA AllValues->#:{AllValues.Count}");
        //        Log.Information("------------------------------------");

        //        DataAfterEnd = AllValues
        //            .SkipWhile(val =>
        //            {
        //                if (val.Equals(endKeyword, StringComparison.OrdinalIgnoreCase))
        //                {
        //                    foundEnd = true;
        //                }
        //                return !foundEnd;
        //            })
        //            .ToList();

        //        // Dividir los datos en sub-arrays con longitud igual al número de encabezados
        //        result = DataAfterEnd
        //            .Select((value, index) => new { value, index })
        //            .GroupBy(x => x.index / headers)
        //            .Select(g => g.Select(x => x.value).ToList()) // Convertir a List<string>
        //            .ToList(); // Cambiar a List<List<string>> para facilitar su manejo

        //        // Agregar los datos de headerBase al principio de la lista
        //        foreach (var headerRow in HeaderBaseData)
        //        {
        //            result.Insert(0, headerRow);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var error = $"Error inesperado durante el procesamiento de Excel: {ex.Message}";
        //        Console.WriteLine(error);
        //        Log.Fatal("------------------------------------");
        //        Log.Fatal(error);
        //        Log.Fatal("------------------------------------");
        //        return null;
        //    }
        //    finally
        //    {
        //        // Eliminar el archivo temporal si es necesario
        //        if (File.Exists(tempFilePath))
        //        {
        //            File.Delete(tempFilePath);
        //        }

        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }

        //    Log.Information("------------------------------------");
        //    Log.Information($"DATA -> result #:{result.Count}");
        //    Log.Information("------------------------------------");

        //    // Retornar la lista de listas de strings
        //    return result;
        //}


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
                    Log.Information($"DATA File created->#:{fileStream.Name}");
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

            try
            {
                using (var workbook = new XLWorkbook(tempFilePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var xlRange = worksheet.RangeUsed();

                    List<string> AllValues = new List<string>();
                    List<List<string>> DataAfterEnd = new List<List<string>>();
                    List<List<string>> HeaderBaseData = new List<List<string>>();

                    // Obtener filas y columnas usadas
                    var rows = xlRange.RowsUsed();
                    int colCount = worksheet.FirstRowUsed().CellCount();

                    // Dividir los valores de headerBase en caso de que vengan separados por comas
                    string[] headerBaseKeywords = headerBase.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                            .Select(hb => hb.Trim())
                                                            .ToArray();

                    bool foundStart = false;
                    bool foundEnd = false;

                    // Procesar las filas
                    foreach (var row in rows)
                    {
                        var rowData = row.Cells().Where(cell => !cell.IsEmpty()).Select(cell => cell.GetFormattedString()).ToList();


                        // Verificar si se encontró la palabra clave de inicio
                        if (!foundStart && rowData.Any(cell => cell.Equals(startKeyword, StringComparison.OrdinalIgnoreCase)))
                        {
                            foundStart = true;
                        }

                        // Si se encontró la palabra clave de inicio, procesar los valores
                        if (foundStart)
                        {
                            // Si encontramos la palabra clave de fin, comenzamos a guardar datos
                            if (rowData.Any(cell => cell.Equals(endKeyword, StringComparison.OrdinalIgnoreCase)))
                            {
                                foundEnd = true;
                            }

                            // Guardar datos después de encontrar la palabra clave de fin
                            if (foundEnd)
                            {
                                DataAfterEnd.Add(rowData);
                            }

                            // Buscar las filas que contengan algún valor de headerBase
                            foreach (var headerKeyword in headerBaseKeywords)
                            {
                                if (rowData.Contains(headerKeyword))
                                {
                                    HeaderBaseData.Add(rowData);
                                }
                            }
                        }
                    }

                    // Dividir los datos en sub-arrays con longitud igual al número de encabezados
                    result = DataAfterEnd
                        .SelectMany(row => row) // Aplanar las filas de datos
                        .Select((value, index) => new { value, index })
                        .GroupBy(x => x.index / headers)
                        .Select(g => g.Select(x => x.value).ToList())
                        .ToList();

                    // Agregar los datos de headerBase al principio de la lista
                    foreach (var headerRow in HeaderBaseData)
                    {
                        result.Insert(0, headerRow);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"Error inesperado durante el procesamiento de Excel: {ex.Message}");
                return null;
            }
            finally
            {
                // Eliminar el archivo temporal
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Log.Information($"DATA -> result #:{result.Count}");
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
