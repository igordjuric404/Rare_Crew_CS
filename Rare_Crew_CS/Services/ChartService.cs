using Rare_Crew_CS.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

public class ChartService
{
    public void GeneratePieChart(List<Employee> employees, string outputFilePath)
    {
        string directory = Path.GetDirectoryName(outputFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var colors = new SKColor[]
        {
            new SKColor(0xFF, 0xDA, 0x99),
            new SKColor(0xFF, 0xD4, 0x87),
            new SKColor(0xFF, 0xCD, 0x75),
            new SKColor(0xFF, 0xC7, 0x63),
            new SKColor(0xFF, 0xC1, 0x52),
            new SKColor(0xFF, 0xBA, 0x40),
            new SKColor(0xFF, 0xB4, 0x2E),
            new SKColor(0xFF, 0xAD, 0x1C),
            new SKColor(0xFF, 0xA7, 0x0A),
            new SKColor(0xFF, 0xA3, 0x00),
            new SKColor(0xFF, 0xA5, 0x00)
        };

        var info = new SKImageInfo(750, 600);
        using (var surface = SKSurface.Create(info))
        {
            var canvas = surface.Canvas;

            canvas.Clear(new SKColor(0x21, 0x25, 0x29));

            canvas.Save();


            var center = new SKPoint(info.Width * 0.671f, info.Height / 2);
            var radius = Math.Min(info.Width * 0.752f, info.Height) * 0.4f;
            canvas.RotateDegrees(-90, center.X, center.Y);

            var totalTimeWorked = employees.Sum(e => e.TotalTimeWorked);

            float startAngle = 0;
            int colorIndex = 0;

            foreach (var employee in employees)
            {
                float sweepAngle = 360f * ((float)employee.TotalTimeWorked / (float)totalTimeWorked);

                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    Color = colors[colorIndex % colors.Length]
                };

                var path = new SKPath();
                path.MoveTo(center);
                path.ArcTo(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), startAngle, sweepAngle, false);
                path.Close();
                canvas.DrawPath(path, paint);

                var borderPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true,
                    Color = SKColors.White,
                    StrokeWidth = 2
                };
                canvas.DrawPath(path, borderPaint);

                startAngle += sweepAngle;
                colorIndex++;
            }

            canvas.Restore();

            float legendX = info.Width * 0.044f;
            float legendY = info.Height * 0.216f;
            float legendSpacing = 32f;

            colorIndex = 0;
            foreach (var employee in employees)
            {
                var legendPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    Color = colors[colorIndex % colors.Length]
                };

                canvas.DrawRect(new SKRect(legendX, legendY, legendX + 20, legendY + 20), legendPaint);

                var textPaint = new SKPaint
                {
                    TextSize = 17,
                    IsAntialias = true,
                    Color = SKColors.White,
                    TextAlign = SKTextAlign.Left
                };

                canvas.DrawText($"{employee.Name} ({(employee.TotalTimeWorked / totalTimeWorked) * 100:F1}%)", legendX + 27, legendY + 15, textPaint);

                legendY += legendSpacing;
                colorIndex++;
            }

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(outputFilePath))
            {
                data.SaveTo(stream);
            }
        }
    }
}
