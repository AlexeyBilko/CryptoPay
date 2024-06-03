using DinkToPdf;
using PdfSharp;
using PdfSharp.Pdf;
using ServiceLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace ServiceLayer.Service.Realization
{
    public static class PDFHelper
    {
        public static byte[] CreateEarningsReportPdf(List<PaymentPageDTO> paymentPages, List<PaymentPageTransactionDTO> paymentPageTransactions, List<WithdrawalDTO> withdrawals, EarningsDTO earnings, DateTime startDate, DateTime endDate)
        {
            var converter = new BasicConverter(new PdfTools());

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 10, Bottom = 10 }
        },
                Objects = {
            new ObjectSettings() {
                PagesCount = true,
                HtmlContent = GenerateEarningsReportHtml(paymentPages, paymentPageTransactions, withdrawals, earnings, startDate, endDate),
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Сторінка [page] із [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "вебзастосунок CryptoPay" }
            }
        }
            };

            var pdf = converter.Convert(doc);

            using (FileStream stream = new FileStream(DateTime.UtcNow.Ticks.ToString() + ".pdf", FileMode.Create))
            {
                stream.Write(pdf, 0, pdf.Length);
            }

            return pdf;
        }

        public static string GenerateEarningsReportHtml(List<PaymentPageDTO> paymentPages, List<PaymentPageTransactionDTO> paymentPageTransactions, List<WithdrawalDTO> withdrawals, EarningsDTO earnings, DateTime startDate, DateTime endDate)
        {
            StringBuilder html = new StringBuilder();
            html.Append(@"<html><head><style>
        body { font-family: Arial, sans-serif; background-color: #f0f0f0; color: #333; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ccc; padding: 8px; text-align: left; }
        th { background-color: #007bff; color: #fff; }
        </style></head><body>");
            html.Append("<h2>Звіт про доходи</h2>");
            html.Append($"<h4>Звітний період: з {startDate.ToShortDateString()} дo {endDate.ToShortDateString()}</h4>");

            // Payment Pages Table
            html.Append("<h3>Платіжні Сторінки</h3>");
            html.Append("<table>");
            html.Append("<tr><th>ID Сторінки</th><th>Назва</th><th>Опис</th></tr>");
            foreach (var page in paymentPages)
            {
                html.Append($"<tr><td>{page.Id}</td><td>{page.Title}</td><td>{page.Description}</td></tr>");
            }
            html.Append("</table>");

            // Transactions Table
            html.Append("<h3>Транзакції</h3>");
            html.Append("<table>");
            html.Append("<tr><th>ID Транзакції</th><th>ID Платіжної Сторінки</th><th>К-ть (Криптовалюти)</th><th>Сума (USD)</th><th>Статус</th><th>Дата</th></tr>");
            foreach (var trx in paymentPageTransactions)
            {
                html.Append($"<tr><td>{trx.Id}</td><td>{trx.PaymentPageId}</td><td>{trx.ActualAmountCrypto} {trx.PaymentPage.AmountDetails.Currency.CurrencyCode}</td><td>${trx.PaymentPage.AmountDetails.AmountUSD}</td><td>{trx.Status}</td><td>{trx.TransactionHash}</td></tr>");
            }
            html.Append("</table>");

            // Withdrawals Table
            html.Append("<h3>Виведення</h3>");
            html.Append("<table>");
            html.Append("<tr><th>ID Виведення</th><th>К-ть</th><th>Криптовалюта</th><th>Статус</th><th>Дата виведення</th></tr>");
            foreach (var wd in withdrawals)
            {
                html.Append($"<tr><td>{wd.Id}</td><td>{wd.AmountDetails.AmountCrypto} {wd.AmountDetails.Currency.CurrencyCode}</td><td>{wd.Status}</td><td>{wd.RequestedDate.ToShortDateString()}</td></tr>");
            }
            html.Append("</table>");

            // Earnings Summary
            html.Append("<h3>Підсумки про заробіток</h3>");
            html.Append("<table>");
            html.Append("<tr><th>Загальна кількість зароблених BTC</th><th>Загальна кількість зароблених ETH</th></tr>");
            html.Append($"<tr><td>{earnings.TotalEarnedBTC} BTC</td><td>{earnings.TotalEarnedETH} ETH</td></tr>");
            html.Append("</table>");
            html.Append("<table>");
            html.Append("<tr><th>Поточний баланс BTC</th><th>Поточний баланс ETH</th></tr>");
            html.Append($"<tr><td>{earnings.CurrentBalanceBTC} BTC</td><td>{earnings.CurrentBalanceETH} ETH</td></tr>");
            html.Append("</table>");

            html.Append("</body></html>");

            return html.ToString();
        }
    }
}
