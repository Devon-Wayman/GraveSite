using OfficeOpenXml;
using OfficeOpenXml.DataValidation.Contracts;

namespace FinderScraper.Reporting
{
    public class ReportsManager
    {
        static string MainReportsPath = string.Empty;

        public static void InitReports<T>(List<T> data, string cemeteryName)
        {
            try
            {
                MainReportsPath = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}\\Reports", $"{cemeteryName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}");
                Directory.CreateDirectory(MainReportsPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory: {ex.Message}");
            }

            SaveToExcel(data, cemeteryName);
        }

        static void SaveToExcel<T>(List<T> data, string sheetName)
        {
            string MainWorkbookPath = Path.Combine(MainReportsPath, $"{sheetName}.xlsx");
            Console.WriteLine($"Saving data to {MainWorkbookPath}");
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(new FileInfo(MainWorkbookPath)))
            {
                // Check if the sheet already exists
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName) ?? package.Workbook.Worksheets.Add(sheetName);

                // Clear existing data
                worksheet.Cells.Clear();

                // Write headers
                System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    var cell = worksheet.Cells[1, i + 1];
                    cell.Value = properties[i].Name;
                    cell.Style.Font.Bold = true; // Make the header bold
                }

                // Write data
                for (int row = 0; row < data.Count; row++)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        // if the data is an array, convert the cell to a dropdown
                        if (properties[col].PropertyType == typeof(string[]))
                        {
                            ExcelRange cell = worksheet.Cells[row + 2, col + 1];
                            if (worksheet.DataValidations[cell.Address] != null)
                            {
                                worksheet.DataValidations.Remove(worksheet.DataValidations[cell.Address]);
                            }
                            IExcelDataValidationList validation = worksheet.DataValidations.AddListValidation(cell.Address);

                            string[]? arrayData = properties[col].GetValue(data[row]) as string[];
                            if (arrayData != null && arrayData.Length > 0)
                            {
                                cell.Value = arrayData.FirstOrDefault(); // Set the first entry as the default value
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                                cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                                // Ensure the total length of the list does not exceed 255 characters
                                int totalLength = arrayData.Sum(item => item.Length) + arrayData.Length - 1;
                                if (totalLength <= 255)
                                {
                                    foreach (string item in arrayData)
                                    {
                                        validation.Formula.Values.Add(item);
                                    }
                                }
                                else
                                {
                                    validation.Formula.Values.Add("List too long");
                                }

                                validation.ShowErrorMessage = true;
                            }
                        }
                        // if the data is a float array with the name Location, convert it to a concatenated string
                        else if (properties[col].PropertyType == typeof(float[]) && properties[col].Name == "Location")
                        {
                            float[]? location = properties[col].GetValue(data[row]) as float[];
                            if (location != null && location.Length >= 2)
                            {
                                Array.Reverse(location);
                                worksheet.Cells[row + 2, col + 1].Value = string.Join(", ", location);
                                var hyperlink = worksheet.Cells[row + 2, col + 2].Hyperlink = new Uri($"https://www.google.com/maps?q={location[0]},{location[1]}");
                                worksheet.Cells[row + 2, col + 2].Value = "Google Maps Link";
                                worksheet.Cells[row + 2, col + 2].Style.Font.UnderLine = true;
                                worksheet.Cells[row + 2, col + 2].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                            }
                            else
                            {
                                worksheet.Cells[row + 2, col + 1].Value = "";
                            }
                        }
                        else
                        {
                            worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(data[row]);
                        }
                    }
                }

                Console.WriteLine("Data written to Excel file. Removing redundant entry data...");

                #region Remove empty location and google map cell data
                int columnToCheck = -1;
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;

                // Find the "GoogleMapsLink" column
                for (int col = 1; col <= colCount; col++)
                {
                    if (worksheet.Cells[1, col].Text == "GoogleMapsLink")
                    {
                        columnToCheck = col;
                        break;
                    }
                }

                // If the column is found, process its values
                if (columnToCheck != -1)
                {
                    for (int row = 2; row <= rowCount; row++) // Start from row 2 to skip headers
                    {
                        var cell = worksheet.Cells[row, columnToCheck];

                        if (cell.Text.StartsWith("https://www.google.com/maps/place/,"))
                        {
                            cell.Value = ""; // Clear the cell
                        }
                    }

                    // Save changes
                    package.Save();
                    Console.WriteLine("Cells updated successfully.");
                }
                #endregion

                // Apply standard solid faded border style to all cells
                using (var range = worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Hair;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Hair;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Hair;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Hair;
                }

                // autofit the column width
                worksheet.Cells.AutoFitColumns(0);

                package.Save();
            }
        }
    }
}
