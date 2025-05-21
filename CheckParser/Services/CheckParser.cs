
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CheckParser.Models;

namespace CheckParser.Services
{
    public class CheckParser
    {
        private static readonly Regex HeaderRx = new Regex(
            @"SERVICE_INFO_START.*?Receipt:\s*(?<rc>\d+).*?Time:\s*(?<tm>[\d\.]+\s[\d:]+).*?Document:\s*(?<dt>\w+).*?SERVICE_INFO_END",
            RegexOptions.Singleline);

        private static readonly Regex KvitasRx = new Regex(@"Kvitas:\s*(?<kv>\d+)", RegexOptions.Singleline);

        private static readonly Regex PaymentRx = new Regex(
            @"Tarpinė suma EUR\s*(?<sub>\d+\.\d{2}).*?Pateikta\s*(?<method>\w+(?: \w+)*?)\s*(?<paid>\d+\.\d{2})(?:.*?Grąža Grynieji\s*(?<chg>\d+\.\d{2}))?",
            RegexOptions.Singleline);

        private static readonly Regex VatRx = new Regex(
            @"PVM-(?<rate>\d+) %\s*(?<vat>\d+\.\d{2})\s*(?<base>\d+\.\d{2})\s*(?<tot>\d+\.\d{2})",
            RegexOptions.Multiline);

        private static readonly Regex FooterRx = new Regex(
            @"Dokumento numeris:\s*(?<dn>\d+).*?Saugos modulio numeris:\s*(?<mid>\S+).*?Kvito parašas:\s*(?<sig>\S+).*?Kvito kodas:\s*(?<code>\S+).*?Fiskalinis kvito#\s*(?<fr>\S+).*?(?<ftime>\d{4}-\d{2}-\d{2}\s[\d:]+)",
            RegexOptions.Singleline);

        // Map of Lithuanian diacritics to ASCII equivalents
        private static readonly Dictionary<char, char> DiacriticMap = new Dictionary<char, char>
        {
            ['Ą'] = 'A',
            ['ą'] = 'a',
            ['Č'] = 'C',
            ['č'] = 'c',
            ['Ę'] = 'E',
            ['ę'] = 'e',
            ['Ė'] = 'E',
            ['ė'] = 'e',
            ['Į'] = 'I',
            ['į'] = 'i',
            ['Š'] = 'S',
            ['š'] = 's',
            ['Ų'] = 'U',
            ['ų'] = 'u',
            ['Ū'] = 'U',
            ['ū'] = 'u',
            ['Ž'] = 'Z',
            ['ž'] = 'z'
        };

        private static string NormalizeLithuanian(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = input.Select(ch => DiacriticMap.TryGetValue(ch, out var mapped) ? mapped : ch);
            return new string(result.ToArray());
        }

        public IEnumerable<Check> ParseAll(string text)
        {
            // split on SERVICE_INFO_START to isolate receipts
            var blocks = Regex.Split(text, "(?=SERVICE_INFO_START)")
                              .Where(b => b.Contains("Kvitas:"));

            foreach (var blk in blocks)
            {
                var c = new Check();

                // header
                var h = HeaderRx.Match(blk);
                if (h.Success)
                {
                    c.ReceiptNumber = h.Groups["rc"].Value;
                    if (DateTime.TryParseExact(h.Groups["tm"].Value,
                                                "dd.MM.yyyy HH:mm:ss",
                                                null,
                                                System.Globalization.DateTimeStyles.None,
                                                out var dt))
                        c.Time = dt;
                    c.DocumentType = NormalizeLithuanian(h.Groups["dt"].Value);
                }

                // Kvitas number
                var kvMatch = KvitasRx.Match(blk);
                if (kvMatch.Success && int.TryParse(kvMatch.Groups["kv"].Value, out var kvnum))
                    c.KvitasNumber = kvnum;

                // items: split by separator lines and parse each section
                c.Items = new List<LineItem>();
                var sections = Regex.Split(blk, "-{2,}")
                                     .Select(s => s.Trim())
                                     .Where(s => Regex.IsMatch(s, @"\d+\.\d{2}\s+A"))
                                     .ToList();
                foreach (var sec in sections)
                {
                    var linesSec = sec.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    // first line: name + price
                    var headerLine = linesSec[0];
                    var mLine = Regex.Match(headerLine, @"(?<name>.+?)\s+(?<price>\d+\.\d{2})\s+A");
                    if (!mLine.Success) continue;
                    // normalize item name
                    var rawName = mLine.Groups["name"].Value.Trim();
                    var name = NormalizeLithuanian(rawName);
                    var price = decimal.Parse(mLine.Groups["price"].Value);

                    // find discount line (if any)
                    decimal? discount = null;
                    foreach (var ln in linesSec.Skip(1))
                    {
                        var mDisc = Regex.Match(ln, @"(?<disc>-?\d+\.\d{2})\s+A");
                        if (mDisc.Success)
                        {
                            discount = decimal.Parse(mDisc.Groups["disc"].Value);
                            break;
                        }
                        var mPct = Regex.Match(ln, @"(?<pct>\d+)%\s+(?<disc>-?\d+\.\d{2})");
                        if (mPct.Success)
                        {
                            discount = decimal.Parse(mPct.Groups["disc"].Value);
                            break;
                        }
                    }

                    // UPC
                    var mUpc = Regex.Match(sec, @"(?<upc>\d{10,13})");
                    var upc = mUpc.Success ? mUpc.Groups["upc"].Value : null;

                    c.Items.Add(new LineItem
                    {
                        Name = name,
                        UnitPrice = price,
                        Discount = discount,
                        Upc = upc
                    });
                }

                // payment
                var p = PaymentRx.Match(blk);
                if (p.Success)
                {
                    c.Payments.Add(new PaymentEntry
                    {
                        Method = NormalizeLithuanian(p.Groups["method"].Value),
                        Amount = decimal.Parse(p.Groups["paid"].Value)
                    });
                    if (p.Groups["chg"].Success && decimal.TryParse(p.Groups["chg"].Value, out var change))
                        c.Change = change;
                }

                // VAT
                c.VatLines = VatRx.Matches(blk)
                                  .Cast<Match>()
                                  .Select(m => new VatLine
                                  {
                                      Rate = int.TryParse(m.Groups["rate"].Value, out var r) ? r : 0,
                                      Amount = decimal.Parse(m.Groups["vat"].Value),
                                      Base = decimal.Parse(m.Groups["base"].Value),
                                      Total = decimal.Parse(m.Groups["tot"].Value)
                                  })
                                  .ToList();

                // footer
                var f = FooterRx.Match(blk);
                if (f.Success)
                {
                    c.DocumentCheckNum = f.Groups["dn"].Value;
                    c.ModuleId = f.Groups["mid"].Value;
                    c.Signature = f.Groups["sig"].Value;
                    c.Code = f.Groups["code"].Value;
                    c.FiscalReceipt = f.Groups["fr"].Value;
                    if (DateTime.TryParse(f.Groups["ftime"].Value, out var ft))
                        c.FiscalTime = ft;
                }

                yield return c;
            }
        }
    }
}
