using System.Globalization;
using System.Text.RegularExpressions;
using DocumentAnalysis.Domain.DTOs;
using DocumentAnalysis.Domain.Entities;

namespace DocumentAnalysis.Infrastructure.Parsing;

internal static partial class DocumentHeuristicsParser
{
    public static ExtractedDocumentData Parse(string text, bool usedOcr)
    {
        var normalizedText = Normalize(text);
        var documentType = DetectDocumentType(normalizedText);
        var totalAmount = ExtractPreferredTotalAmount(normalizedText);

        return new ExtractedDocumentData
        {
            ExtractedText = normalizedText,
            DocumentType = documentType,
            MerchantName = ExtractMerchant(normalizedText),
            DocumentNumber = ExtractDocumentNumber(normalizedText),
            IssueDate = ExtractIssueDate(normalizedText),
            DueDate = ExtractLabeledDate(normalizedText, DueDateLabelRegex()),
            CustomerName = ExtractCustomerName(normalizedText),
            TotalAmount = totalAmount,
            Subtotal = ExtractAmountByLabel(normalizedText, SubtotalRegex()),
            TaxAmount = ExtractAmountByLabel(normalizedText, TaxRegex()),
            Currency = ExtractCurrency(normalizedText),
            WarrantyDurationMonths = ExtractWarrantyDurationMonths(normalizedText),
            LineItems = ExtractLineItems(normalizedText),
            UsedOcr = usedOcr
        };
    }

    private static string Normalize(string text)
    {
        return string.Join(
            Environment.NewLine,
            text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private static DocumentType DetectDocumentType(string text)
    {
        if (text.Contains("invoice", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("fiscal invoice", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("factura", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("factura fiscala", StringComparison.OrdinalIgnoreCase))
        {
            return DocumentType.Invoice;
        }

        if (text.Contains("receipt", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("bon fiscal", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("bon", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("total", StringComparison.OrdinalIgnoreCase))
        {
            return DocumentType.Receipt;
        }

        return DocumentType.Unknown;
    }

    private static string? ExtractMerchant(string text)
    {
        var knownMerchant = ExtractKnownMerchant(text);
        if (!string.IsNullOrWhiteSpace(knownMerchant))
            return knownMerchant;

        var supplierMatch = SupplierLabelRegex().Match(text);
        if (supplierMatch.Success)
        {
            var supplier = CleanMergedSectionValue(supplierMatch.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(supplier))
                return supplier;
        }

        var firstLine = text.Split(Environment.NewLine).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(firstLine))
            return null;

        if (firstLine.Equals("Factura", StringComparison.OrdinalIgnoreCase) ||
            firstLine.Equals("Invoice", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return firstLine;
    }

    private static string? ExtractDocumentNumber(string text)
    {
        var seriesAndNumberMatch = SeriesAndNumberRegex().Match(text);
        if (seriesAndNumberMatch.Success)
        {
            var seriesAndNumber = seriesAndNumberMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(seriesAndNumber))
                return seriesAndNumber;
        }

        var invoiceNumberMatch = InvoiceNumberPriorityRegex().Match(text);
        if (invoiceNumberMatch.Success)
        {
            var preferredValue = invoiceNumberMatch.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(preferredValue))
                return preferredValue;
        }

        foreach (Match match in DocumentNumberRegex().Matches(text))
        {
            var value = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(value) &&
                !value.Equals("invoice", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("receipt", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("factura", StringComparison.OrdinalIgnoreCase) &&
                !value.Equals("furnizor", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

    private static DateTime? ExtractIssueDate(string text)
    {
        var labeledIssueDate = ExtractLabeledDate(text, IssueDateLabelRegex());
        if (labeledIssueDate.HasValue)
            return labeledIssueDate;

        var topBlockDate = ExtractDateNearHeader(text);
        if (topBlockDate.HasValue)
            return topBlockDate;

        foreach (Match match in DateRegex().Matches(text))
        {
            var parsed = TryParseDateValue(match.Value);
            if (parsed.HasValue)
                return parsed;
        }

        foreach (Match match in LongDateRegex().Matches(text))
        {
            var parsed = TryParseDateValue(match.Value);
            if (parsed.HasValue)
                return parsed;
        }

        return null;
    }

    private static DateTime? ExtractLabeledDate(string text, Regex regex)
    {
        var match = regex.Match(text);
        if (!match.Success)
            return null;

        return TryParseDateValue(match.Groups[1].Value.Trim());
    }

    private static DateTime? TryParseDateValue(string value)
    {
        if (DateTime.TryParseExact(
                value,
                ["dd.MM.yyyy", "dd/MM/yyyy", "yyyy-MM-dd"],
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var exactDate))
        {
            return exactDate;
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var parsedDate))
        {
            return parsedDate;
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.GetCultureInfo("ro-RO"),
                DateTimeStyles.AllowWhiteSpaces,
                out var parsedRomanianDate))
        {
            return parsedRomanianDate;
        }

        return null;
    }

    private static string? ExtractCustomerName(string text)
    {
        var customerFromClientBlock = ExtractCustomerFromClientBlock(text);
        if (!string.IsNullOrWhiteSpace(customerFromClientBlock))
            return customerFromClientBlock;

        var inlineMatch = InlineCustomerRegex().Match(text);
        if (inlineMatch.Success)
        {
            var inlineCustomer = CleanCustomerValue(inlineMatch.Groups[1].Value);
            if (!string.IsNullOrWhiteSpace(inlineCustomer))
                return inlineCustomer;
        }

        var lines = text.Split(Environment.NewLine);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!CustomerLabelRegex().IsMatch(lines[index]))
                continue;

            for (var nextIndex = index + 1; nextIndex < lines.Length; nextIndex++)
            {
                var candidate = lines[nextIndex].Trim();
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;
                if (IsSectionMarker(candidate))
                    return null;

                return CleanCustomerValue(candidate);
            }
        }

        return null;
    }

    private static decimal? ExtractAmountByLabel(string text, Regex regex)
    {
        decimal? bestAmount = null;

        foreach (Match match in regex.Matches(text))
        {
            var normalized = NormalizeAmount(match.Groups[1].Value);
            if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                continue;

            if (!bestAmount.HasValue || amount > bestAmount.Value)
                bestAmount = amount;
        }

        return bestAmount;
    }

    private static decimal? ExtractPreferredTotalAmount(string text)
    {
        var explicitTotal = ExtractAmountByLabel(text, TotalOfPaymentRegex());
        if (explicitTotal.HasValue)
            return explicitTotal;

        var splitTotal = ExtractSplitTotalOfPayment(text);
        if (splitTotal.HasValue)
            return splitTotal;

        var footerTotal = ExtractFooterTotal(text);
        if (footerTotal.HasValue)
            return footerTotal;

        var altexDerivedTotal = ExtractAltexDerivedTotal(text);
        if (altexDerivedTotal.HasValue)
            return altexDerivedTotal;

        return ExtractAmountByLabel(text, TotalRegex());
    }

    private static decimal? ExtractSplitTotalOfPayment(string text)
    {
        var lines = text.Split(Environment.NewLine);
        for (var index = 0; index < lines.Length; index++)
        {
            if (!lines[index].Contains("Total de plata", StringComparison.OrdinalIgnoreCase))
                continue;

            for (var nextIndex = index + 1; nextIndex < Math.Min(index + 4, lines.Length); nextIndex++)
            {
                var amountMatch = MoneyRegex().Match(lines[nextIndex]);
                if (!amountMatch.Success)
                    continue;

                var normalized = NormalizeAmount(amountMatch.Value);
                if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                    return amount;
            }
        }

        return null;
    }

    private static string? ExtractCurrency(string text)
    {
        if (text.Contains('$'))
            return "USD";
        if (text.Contains("EUR", StringComparison.OrdinalIgnoreCase))
            return "EUR";
        if (text.Contains("RON", StringComparison.OrdinalIgnoreCase) || text.Contains("LEI", StringComparison.OrdinalIgnoreCase))
            return "RON";

        var match = CurrencyRegex().Match(text);
        if (!match.Success)
            return null;

        var value = match.Value.ToUpperInvariant();
        return value switch
        {
            "LEI" => "RON",
            _ => value
        };
    }

    private static int? ExtractWarrantyDurationMonths(string text)
    {
        var monthMatch = WarrantyMonthsRegex().Match(text);
        if (monthMatch.Success && int.TryParse(monthMatch.Groups[1].Value, out var months))
            return months;

        var yearMatch = WarrantyYearsRegex().Match(text);
        if (yearMatch.Success && int.TryParse(yearMatch.Groups[1].Value, out var years))
            return years * 12;

        return null;
    }

    private static List<ExtractedLineItemDto> ExtractLineItems(string text)
    {
        var altexItems = ExtractAltexLineItems(text);
        if (altexItems.Count > 0)
            return altexItems;

        var lines = text.Split(Environment.NewLine);
        var results = new List<ExtractedLineItemDto>();
        var inItemsSection = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!inItemsSection)
            {
                if (LineItemsHeaderRegex().IsMatch(line) ||
                    RomanianItemsHeaderRegex().IsMatch(line) ||
                    RomanianPriceHeaderRegex().IsMatch(line))
                {
                    inItemsSection = true;
                }

                continue;
            }

            if (IsItemsStopMarker(line))
                break;

            if (PriceTableHeaderRegex().IsMatch(line) || RomanianPriceHeaderRegex().IsMatch(line))
                continue;

            var parsedLine = ParseLineItem(line);
            if (parsedLine != null)
            {
                results.Add(parsedLine);
                continue;
            }

            var invoiceLine = ParseRomanianInvoiceLineItem(line);
            if (invoiceLine != null)
                results.Add(invoiceLine);
        }

        return results;
    }

    private static string? ExtractKnownMerchant(string text)
    {
        foreach (var merchant in KnownMerchants())
        {
            if (text.Contains(merchant, StringComparison.OrdinalIgnoreCase))
                return merchant;
        }

        return null;
    }

    private static string? ExtractCustomerFromClientBlock(string text)
    {
        var lines = text.Split(Environment.NewLine);
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith("Client:", StringComparison.OrdinalIgnoreCase))
                continue;

            for (var j = i + 1; j < Math.Min(lines.Length, i + 3); j++)
            {
                var candidate = lines[j].Trim().Trim('|').Trim();
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;
                if (candidate.StartsWith("Tip de plata", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (ClientCodeRegex().IsMatch(candidate))
                    continue;

                return CleanCustomerValue(candidate);
            }
        }

        return null;
    }

    private static List<ExtractedLineItemDto> ExtractAltexLineItems(string text)
    {
        var items = new List<ExtractedLineItemDto>();
        var lines = text.Split(Environment.NewLine);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (!AltexItemStartRegex().IsMatch(line))
                continue;

            var descriptionParts = new List<string> { line.Trim() };
            var numericLineIndex = -1;

            for (var j = i + 1; j < Math.Min(lines.Length, i + 4); j++)
            {
                var candidate = lines[j].Trim();
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                if (AltexNumericLineRegex().IsMatch(candidate))
                {
                    numericLineIndex = j;
                    break;
                }

                if (!StopMarkerRegex().IsMatch(candidate))
                    descriptionParts.Add(candidate);
            }

            if (numericLineIndex < 0)
                continue;

            var description = CleanupDescription(string.Join(" ", descriptionParts));
            if (string.IsNullOrWhiteSpace(description))
                continue;

            var numericLine = lines[numericLineIndex].Trim();
            var numericMatch = AltexNumericLineRegex().Match(numericLine);
            if (!numericMatch.Success)
                continue;

            items.Add(new ExtractedLineItemDto
            {
                Description = CleanupDescription(description),
                Quantity = ParseDecimalAsInt(numericMatch.Groups["qty"].Value),
                UnitPrice = ParseDecimal(numericMatch.Groups["unit"].Value),
                Amount = ParseDecimal(numericMatch.Groups["amount"].Value)
            });

            i = numericLineIndex;
        }

        return items;
    }

    private static decimal? ExtractFooterTotal(string text)
    {
        var lines = text.Split(Environment.NewLine);
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains("Total de plata", StringComparison.OrdinalIgnoreCase))
                continue;

            for (var j = i; j < Math.Min(lines.Length, i + 3); j++)
            {
                var matches = MoneyRegex().Matches(lines[j]);
                if (matches.Count == 0)
                    continue;

                var lastValue = matches[^1].Value;
                var parsed = ParseDecimal(lastValue);
                if (parsed.HasValue)
                    return parsed;
            }
        }

        return null;
    }

    private static decimal? ExtractAltexDerivedTotal(string text)
    {
        var lines = text.Split(Environment.NewLine);
        for (var i = 0; i < lines.Length; i++)
        {
            if (!AltexItemStartRegex().IsMatch(lines[i].Trim()))
                continue;

            for (var j = i + 1; j < Math.Min(lines.Length, i + 4); j++)
            {
                var numericMatch = AltexNumericLineRegex().Match(lines[j].Trim());
                if (!numericMatch.Success)
                    continue;

                return ParseDecimal(numericMatch.Groups["total"].Value);
            }
        }

        return null;
    }

    private static string CleanupDescription(string description)
    {
        return MultiSpaceRegex()
            .Replace(description, " ")
            .Trim(' ', '-', ':', '|');
    }

    private static ExtractedLineItemDto? ParseLineItem(string line)
    {
        if (!ItemPrefixRegex().IsMatch(line))
            return null;

        var itemIndex = ExtractLeadingNumber(line);
        var withoutPrefix = ItemPrefixRegex().Replace(line, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(withoutPrefix))
            return null;

        if (TableLegendRegex().IsMatch(withoutPrefix) || ColumnFormulaRegex().IsMatch(withoutPrefix))
            return null;

        var amounts = MoneyRegex().Matches(withoutPrefix)
            .Select(match => NormalizeAmount(match.Value))
            .Select(value => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : (decimal?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        if (amounts.Count >= 4)
        {
            return ParseInvoiceStyleLineItem(withoutPrefix, amounts);
        }

        var description = MoneyRegex().Replace(withoutPrefix, string.Empty);
        description = MultiSpaceRegex().Replace(description, " ").Trim(' ', '-', ':');

        if (string.IsNullOrWhiteSpace(description))
            return null;

        return new ExtractedLineItemDto
        {
            Description = description,
            Quantity = amounts.Count > 0 ? itemIndex : null,
            UnitPrice = amounts.Count >= 2 ? amounts[^2] : (amounts.Count == 1 ? amounts[0] : null),
            Amount = amounts.Count >= 1 ? amounts[^1] : null
        };
    }

    private static ExtractedLineItemDto ParseInvoiceStyleLineItem(string line, List<decimal> amounts)
    {
        var quantity = (int?)null;
        if (amounts[0] > 0 && amounts[0] <= 100 && decimal.Truncate(amounts[0]) == amounts[0])
            quantity = (int)amounts[0];

        var description = line;
        foreach (Match match in MoneyRegex().Matches(line).Cast<Match>().Reverse())
        {
            description = description.Remove(match.Index, match.Length);
        }

        description = MultiSpaceRegex().Replace(description, " ").Trim(' ', '-', ':');
        description = RomanianLineNoiseRegex().Replace(description, string.Empty).Trim();

        return new ExtractedLineItemDto
        {
            Description = description,
            Quantity = quantity,
            UnitPrice = amounts.Count >= 3 ? amounts[1] : null,
            Amount = amounts.Count >= 3 ? amounts[2] : (amounts.Count >= 1 ? amounts[^1] : null)
        };
    }

    private static int? ExtractLeadingNumber(string line)
    {
        var match = QuantityRegex().Match(line);
        return match.Success && int.TryParse(match.Groups[1].Value, out var quantity) ? quantity : null;
    }

    private static bool IsSectionMarker(string value)
    {
        return value.EndsWith(":", StringComparison.Ordinal) ||
               PriceTableHeaderRegex().IsMatch(value) ||
               IsItemsStopMarker(value);
    }

    private static bool IsItemsStopMarker(string line)
    {
        return StopMarkerRegex().IsMatch(line);
    }

    private static string NormalizeAmount(string rawValue)
    {
        var value = rawValue.Trim();
        value = value.Replace("$", string.Empty, StringComparison.Ordinal)
            .Replace("EUR", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("RON", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("LEI", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (value.Contains(',') && value.Contains('.'))
            return value.Replace(",", string.Empty, StringComparison.Ordinal);

        if (value.Count(c => c == ',') == 1 && value.Count(c => c == '.') == 0)
            return value.Replace(",", ".", StringComparison.Ordinal);

        return value;
    }

    private static DateTime? ExtractDateNearHeader(string text)
    {
        var lines = text.Split(Environment.NewLine);
        for (var i = 0; i < Math.Min(lines.Length, 8); i++)
        {
            var line = lines[i];
            if (!line.Contains("Data emiterii", StringComparison.OrdinalIgnoreCase) &&
                !line.Contains("Data facturii", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            for (var j = i + 1; j < Math.Min(lines.Length, i + 4); j++)
            {
                var maybeDate = lines[j].Trim();
                var parsed = TryParseDateValue(maybeDate);
                if (parsed.HasValue)
                    return parsed;
            }
        }

        return null;
    }

    private static string CleanCustomerValue(string value)
    {
        var cleaned = CleanMergedSectionValue(value);
        cleaned = CustomerNoiseRegex().Replace(cleaned, string.Empty).Trim();

        var words = cleaned
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 1 && char.IsLetter(word[0]))
            .ToList();

        if (words.Count >= 2)
            return string.Join(' ', words.TakeLast(2));

        return cleaned;
    }

    private static ExtractedLineItemDto? ParseRomanianInvoiceLineItem(string line)
    {
        var match = RomanianInvoiceLineRegex().Match(line);
        if (!match.Success)
            return null;

        var description = match.Groups["desc"].Value.Trim();
        description = RomanianLineNoiseRegex().Replace(description, string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var quantity = ParseDecimalAsInt(match.Groups["qty"].Value);
        var unitPrice = ParseDecimal(match.Groups["unit"].Value);
        var amount = ParseDecimal(match.Groups["amount"].Value);

        return new ExtractedLineItemDto
        {
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = amount
        };
    }

    private static int? ParseDecimalAsInt(string value)
    {
        var parsed = ParseDecimal(value);
        if (!parsed.HasValue)
            return null;

        return (int)parsed.Value;
    }

    private static decimal? ParseDecimal(string value)
    {
        var normalized = NormalizeAmount(value);
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : null;
    }

    private static string CleanMergedSectionValue(string value)
    {
        var cleaned = value;
        foreach (var marker in SectionSplitMarkers())
        {
            var markerIndex = cleaned.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                cleaned = cleaned[..markerIndex];
            }
        }

        return cleaned.Trim(' ', ':', '|', '-');
    }

    [GeneratedRegex(@"(?:invoice\s*(?:no|number)?|receipt\s*(?:no|number)?|factura\s*(?:nr|numar)?|bon(?:\s+fiscal)?\s*(?:nr|numar)?)[^\r\n:]*[:#\s]+([A-Z0-9\-\/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DocumentNumberRegex();

    [GeneratedRegex(@"(?:nr\.?\s*factura|invoice\s*no|invoice\s*number)[^\r\n:]*[:#\s]+([A-Z0-9\-\/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex InvoiceNumberPriorityRegex();

    [GeneratedRegex(@"serie\s+si\s+nr\.?\s*[:;]?\s*([A-Z0-9\-\/]+)", RegexOptions.IgnoreCase)]
    private static partial Regex SeriesAndNumberRegex();

    [GeneratedRegex(@"(?:furnizor|supplier)[^\r\n:]*[:#\s]+([^\r\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex SupplierLabelRegex();

    [GeneratedRegex(@"(?:cumparator|client|beneficiar)[^\r\n:]*[:#\s]+([^\r\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex InlineCustomerRegex();

    [GeneratedRegex(@"(?:invoice\s*date|issue\s*date|data\s*(?:facturii|emiterii|documentului))[^\r\n:]*[:#\s]+([^\r\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex IssueDateLabelRegex();

    [GeneratedRegex(@"(?:due\s*date|data\s*scadentei|scadenta)[^\r\n:]*[:#\s]+([^\r\n]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DueDateLabelRegex();

    [GeneratedRegex(@"\b\d{2}[./]\d{2}[./]\d{4}\b|\b\d{4}-\d{2}-\d{2}\b", RegexOptions.IgnoreCase)]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"\b(?:January|February|March|April|May|June|July|August|September|October|November|December|ianuarie|februarie|martie|aprilie|mai|iunie|iulie|august|septembrie|octombrie|noiembrie|decembrie)\s+\d{1,2}(?:,)?\s+\d{4}\b|\b\d{1,2}\s+(?:ianuarie|februarie|martie|aprilie|mai|iunie|iulie|august|septembrie|octombrie|noiembrie|decembrie)\s+\d{4}\b", RegexOptions.IgnoreCase)]
    private static partial Regex LongDateRegex();

    [GeneratedRegex(@"\b(?:total amount|grand total|amount due|de plata|total de plata|total)\b[^\r\n$]*(?:[$])?(\d{1,3}(?:,\d{3})*(?:\.\d{2})|\d+(?:[.,]\d{2}))", RegexOptions.IgnoreCase)]
    private static partial Regex TotalRegex();

    [GeneratedRegex(@"total\s+de\s+plata(?:[^\r\n]*)?[\r\n\s:|]*(\d{1,3}(?:\.\d{3})*(?:,\d{2})|\d{1,3}(?:,\d{3})*(?:\.\d{2})|\d+(?:[.,]\d{2}))", RegexOptions.IgnoreCase)]
    private static partial Regex TotalOfPaymentRegex();

    [GeneratedRegex(@"(?:subtotal|sub-total)[^\d$]*(?:[$])?(\d{1,3}(?:,\d{3})*(?:\.\d{2})|\d+(?:[.,]\d{2}))", RegexOptions.IgnoreCase)]
    private static partial Regex SubtotalRegex();

    [GeneratedRegex(@"(?:tax|tva)[^\r\n$]*(?:[$])?(\d{1,3}(?:,\d{3})*(?:\.\d{2})|\d+(?:[.,]\d{2}))", RegexOptions.IgnoreCase)]
    private static partial Regex TaxRegex();

    [GeneratedRegex(@"\b(RON|EUR|USD|LEI)\b", RegexOptions.IgnoreCase)]
    private static partial Regex CurrencyRegex();

    [GeneratedRegex(@"(?:^|\b)(\d+)\s*(?:month|months|luna|luni)\b", RegexOptions.IgnoreCase)]
    private static partial Regex WarrantyMonthsRegex();

    [GeneratedRegex(@"(?:^|\b)(\d+)\s*(?:year|years|an|ani)\b", RegexOptions.IgnoreCase)]
    private static partial Regex WarrantyYearsRegex();

    [GeneratedRegex(@"^(?:no\.?\s+)?(?:description|descriere|produs(?:e)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex LineItemsHeaderRegex();

    [GeneratedRegex(@"^(?:nr\.?\s+)?denumirea\s+produselor", RegexOptions.IgnoreCase)]
    private static partial Regex RomanianItemsHeaderRegex();

    [GeneratedRegex(@"^(?:qty|cant(?:itate)?)\s+(?:unit\s+price|pret(?:\s+unitar)?)\s+(?:amount|valoare|total)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PriceTableHeaderRegex();

    [GeneratedRegex(@"^(?:crt\.|u\.m\.|cantitatea|pret\s+unitar|cota|valoarea)\b", RegexOptions.IgnoreCase)]
    private static partial Regex RomanianPriceHeaderRegex();

    [GeneratedRegex(@"^(?:payment method|modalitate de plata|subtotal|sub-total|tax|tva|total(?: amount)?|total de plata|grand total|notes?|mentiuni|observatii|bill to|ship to|client|cumparator)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StopMarkerRegex();

    [GeneratedRegex(@"^(?:bill to|client|cumparator|beneficiar)\b", RegexOptions.IgnoreCase)]
    private static partial Regex CustomerLabelRegex();

    [GeneratedRegex(@"^\d+\s+")]
    private static partial Regex ItemPrefixRegex();

    [GeneratedRegex(@"^(?<idx>\d+)\s+(?<desc>.+?)\s+buc\s+(?<qty>\d+)\s+(?<unit>\d+(?:[.,]\d{2}))\s+\d+\s+(?<amount>\d+(?:[.,]\d{2}))\s+(?<vat>\d+(?:[.,]\d{2}))$", RegexOptions.IgnoreCase)]
    private static partial Regex RomanianInvoiceLineRegex();

    [GeneratedRegex(@"^[A-Z0-9]{4,}\s+\d{8,}\s+.+$", RegexOptions.IgnoreCase)]
    private static partial Regex AltexItemStartRegex();

    [GeneratedRegex(@"^(?<green>\d+(?:[.,]\d+)?)\s+(?<qty>\d+)\s+(?<unit>\d+(?:[.,]\d{2}))\s+\|?\s*\d+%?\s+(?<amount>\d+(?:[.,]\d{2}))\s+(?<vat>\d+(?:[.,]\d{2}))\s+(?<total>\d+(?:[.,]\d{2}))$", RegexOptions.IgnoreCase)]
    private static partial Regex AltexNumericLineRegex();

    [GeneratedRegex(@"^(?:\(?\d+\)?\s*)+$")]
    private static partial Regex TableLegendRegex();

    [GeneratedRegex(@"^\d+\(\d+x\d+\)(?:\s+\d+\(\d+x\d+\))*$", RegexOptions.IgnoreCase)]
    private static partial Regex ColumnFormulaRegex();

    [GeneratedRegex(@"\d{1,3}(?:,\d{3})*(?:\.\d{2})|\d+(?:[.,]\d{2})", RegexOptions.IgnoreCase)]
    private static partial Regex MoneyRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultiSpaceRegex();

    [GeneratedRegex(@"^(\d+)\b")]
    private static partial Regex QuantityRegex();

    [GeneratedRegex(@"^(?:Nr\.?\s*ord\.?registru\s*com\.?/an:?\s*[A-Z0-9]+)\s*", RegexOptions.IgnoreCase)]
    private static partial Regex CustomerNoiseRegex();

    [GeneratedRegex(@"^CL\d+$", RegexOptions.IgnoreCase)]
    private static partial Regex ClientCodeRegex();

    [GeneratedRegex(@"\b(?:buc|kg|set|bax|bottle|pcs)\b(?:\s+\d+)?(?:\s+\d+)?$|\b\d+\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex RomanianLineNoiseRegex();

    private static string[] SectionSplitMarkers()
    {
        return
        [
            "Cumparator",
            "Client",
            "Beneficiar",
            "Sediul",
            "Judetul",
            "Cod client",
            "Contul",
            "Banca"
        ];
    }

    private static string[] KnownMerchants()
    {
        return
        [
            "ALTEX ROMANIA SRL",
            "Dante International S.A"
        ];
    }
}
