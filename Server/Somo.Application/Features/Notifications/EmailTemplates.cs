using System.Globalization;
using System.Net;

namespace Somo.Application.Features.Notifications;

/// <summary>
/// Aspectul comun al emailurilor trimise de Somo. Stilurile sunt inline pentru că
/// niciun client de email nu se poate baza pe o foaie de stil externă.
/// </summary>
internal static class EmailTemplates
{
    private static readonly CultureInfo Romanian = new("ro-RO");

    public static string Layout(string heading, string introduction, string bodyHtml, string? footerNote = null)
    {
        var footer = footerNote is null
            ? string.Empty
            : $"""<p style="margin:24px 0 0;font-size:13px;color:#7f8c8d;">{footerNote}</p>""";

        return $"""
            <div style="margin:0;padding:24px;background:#f0f4f8;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e2e8f0;">
                <div style="background:#2c3e50;padding:20px 28px;">
                  <span style="color:#ffffff;font-size:20px;font-weight:700;letter-spacing:0.5px;">Somo</span>
                </div>
                <div style="padding:28px;">
                  <h1 style="margin:0 0 12px;font-size:20px;color:#2c3e50;">{heading}</h1>
                  <p style="margin:0 0 20px;font-size:15px;line-height:1.6;color:#4a5568;">{introduction}</p>
                  {bodyHtml}
                  {footer}
                </div>
              </div>
              <p style="max-width:560px;margin:16px auto 0;font-size:12px;color:#8a96a3;text-align:center;">
                Ai primit acest mesaj pentru că folosești Somo pentru programările animalului tău.
              </p>
            </div>
            """;
    }

    /// <summary>
    /// Tabelul de detalii din corpul mesajului. Perechile cu valoare goală se omit.
    /// </summary>
    public static string DetailsTable(params (string Label, string? Value)[] rows)
    {
        var cells = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Value))
            .Select(r => $"""
                <tr>
                  <td style="padding:8px 0;font-size:14px;color:#7f8c8d;width:40%;">{Escape(r.Label)}</td>
                  <td style="padding:8px 0;font-size:14px;color:#2c3e50;font-weight:600;">{Escape(r.Value!)}</td>
                </tr>
                """);

        return $"""
            <table style="width:100%;border-collapse:collapse;background:#f8fafc;border-radius:8px;padding:8px 16px;">
              {string.Join("\n", cells)}
            </table>
            """;
    }

    public static string Callout(string text, string accent = "#f39c12") => $"""
        <p style="margin:20px 0 0;padding:14px 16px;border-left:4px solid {accent};background:#f8fafc;font-size:14px;line-height:1.6;color:#2c3e50;">
          {text}
        </p>
        """;

    public static string FormatDateTime(DateTime value)
        => value.ToString("dddd, d MMMM yyyy, HH:mm", Romanian);

    public static string FormatDate(DateTime value)
        => value.ToString("d MMMM yyyy", Romanian);

    public static string Escape(string value) => WebUtility.HtmlEncode(value);
}
