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



        public static async Task<List<List<string>>> ProcessExcelFileAsync(Stream excelFileStream, string nameFile, int headers, string startKeyword, string endKeyword, List<string> headerBase)
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
                    // Suponiendo que headerBase ya es una lista de strings
                    string resultado = String.Join(",", headerBase);

                    string[] headerBaseKeywords = resultado.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(hb => hb.Trim())
                                        .ToArray();              // Convierte la lista en un array


                    bool foundStart = false;
                    bool foundEnd = false;

                    // Procesar las filas
                    foreach (var row in rows)
                    {
                        var rowData = row.Cells().Where(cell => !cell.IsEmpty()).Select(cell => cell.GetFormattedString()).ToList();


                        // Verificar si se encontró la palabra clave de inicio
                        if (!foundStart && rowData.Any(cell => cell.Equals(startKeyword, StringComparison.Ordinal)))
                        {
                            foundStart = true;
                        }

                        // Si se encontró la palabra clave de inicio, procesar los valores
                        if (foundStart)
                        {
                            // Verificar si se encontró la palabra clave de fin
                            if (rowData.Any(cell => cell.Equals(endKeyword, StringComparison.Ordinal)))
                            {
                                foundEnd = true;
                            }

                            // Guardar datos después de encontrar la palabra clave de fin
                            if (foundEnd)
                            {
                                DataAfterEnd.Add(rowData);
                            }

                            // Buscar las filas que contengan exactamente los valores de headerBase
                            foreach (var headerKeyword in headerBaseKeywords)
                            {
                                if (rowData.Any(cell => cell.Equals(headerKeyword, StringComparison.Ordinal)))
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
        public async Task<List<List<string>>> SimulateFileUpload(Stream uploadedFileStream, string nameFile, int headers, string startKeyword, string endKeyword, List<string> headerBase)
        {
            // Procesar el archivo directamente desde el Stream sin ruta de archivo local
            return await ProcessExcelFileAsync(uploadedFileStream, nameFile, headers, startKeyword, endKeyword, headerBase);
        }
    }
}