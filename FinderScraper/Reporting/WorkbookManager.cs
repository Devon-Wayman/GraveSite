using OfficeOpenXml;

namespace FinderScraper.Reporting
{
    public class WorkbookManager
    {
        // This class will be used to manage the creation of Excel workbooks

        // set the path for the main xlsx file to the working directory of the executable and name it AllMemorials.xlsx
        public static string MainWorkbookPath = $"{AppDomain.CurrentDomain.BaseDirectory}\\AllMemorials.xlsx";

        public static void SaveToExcel<T>(List<T> data, string sheetName)
        {
            Console.WriteLine($"Saving data to {MainWorkbookPath}");
            // This method will save the data to the workbook
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(new FileInfo(MainWorkbookPath)))
            {
                // Check if the sheet already exists
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);

                // If the sheet exists, clear the data
                if (worksheet != null)
                {
                    // Clear existing data
                    worksheet.Cells.Clear();
                }
                else
                {
                    // Create new worksheet
                    worksheet = package.Workbook.Worksheets.Add(sheetName);
                }

                // Write headers
                var properties = typeof(T).GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i].Name;
                }

                // Write data
                for (int row = 0; row < data.Count; row++)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        // if the data is an array, convert it to a concatenated string. examples include Spouse, Parents and Siblings
                        if (properties[col].PropertyType == typeof(string[]))
                        {
                            // make the cell containing the string array data into a dropdown
                            var cell = worksheet.Cells[row + 2, col + 1];
                            var arrayData = (string[])properties[col].GetValue(data[row]);
                            cell.Value = arrayData.FirstOrDefault(); // Set the first entry as the default value
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                            cell.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

                            var validation = worksheet.DataValidations.AddListValidation(cell.Address);
                            foreach (var item in arrayData)
                            {
                                validation.Formula.Values.Add(item);
                            }
                            validation.ShowErrorMessage = true;
                        }
                        // if the data is a float array with the name Location, convert it to a concatenated string
                        else if (properties[col].PropertyType == typeof(float[]) && properties[col].Name == "Location")
                        {
                            var location = (float[])properties[col].GetValue(data[row]);
                            if (location != null && location.Length >= 2)
                            {
                                Array.Reverse(location);
                                worksheet.Cells[row + 2, col + 1].Value = string.Join(", ", location);
                                // Add Google Maps link in the next column
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
