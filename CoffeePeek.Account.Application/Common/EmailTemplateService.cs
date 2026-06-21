using System.Net;
using CoffeePeek.Account.Application.Common.Interfaces;

namespace CoffeePeek.Account.Application.Common;

public class EmailTemplateService : IEmailTemplateService
{
    private const string BrandGold = "#EAB308";
    private const string BrandBg = "#1A1412";
    private const string BrandSurface = "#2D241F";
    private const string BrandBorder = "#3D2F28";
    private const string TextPrimary = "#FFFFFF";
    private const string TextMuted = "#A39E93";
    private const string TextDim = "#5C544F";

    private const string LogoUrl = "https://coffeepeek.by/logo-mark.svg";

    private const string ConfirmationEmailCss = """
        body, table, td, a { -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }
        table, td { mso-table-lspace: 0pt; mso-table-rspace: 0pt; }
        img { border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; }
        body { margin: 0 !important; padding: 0 !important; width: 100% !important; }
        a[x-apple-data-detectors] { color: inherit !important; text-decoration: none !important; }
        @media only screen and (max-width: 620px) {
            .email-wrapper { padding: 16px !important; }
            .email-card { width: 100% !important; }
            .email-content { padding: 28px 20px !important; }
            .email-heading { font-size: 24px !important; line-height: 32px !important; }
            .email-cta { display: block !important; width: 100% !important; box-sizing: border-box !important; }
            .email-steps { display: none !important; }
        }
        """;

    public string GetConfirmationHtml(string username, string confirmationUrl)
    {
        var safeUsername = WebUtility.HtmlEncode(username);
        var safeUrlDisplay = WebUtility.HtmlEncode(confirmationUrl);
        var year = DateTime.UtcNow.Year;

        return $"""
            <!DOCTYPE html>
            <html lang="ru" xmlns="http://www.w3.org/1999/xhtml" xmlns:v="urn:schemas-microsoft-com:vml" xmlns:o="urn:schemas-microsoft-com:office:office">
            <head>
                <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <meta name="color-scheme" content="dark" />
                <meta name="supported-color-schemes" content="dark" />
                <title>Подтверждение email — CoffeePeek</title>
                <!--[if mso]>
                <noscript>
                    <xml>
                        <o:OfficeDocumentSettings>
                            <o:PixelsPerInch>96</o:PixelsPerInch>
                        </o:OfficeDocumentSettings>
                    </xml>
                </noscript>
                <![endif]-->
                <style type="text/css">
                    {ConfirmationEmailCss}
                </style>
            </head>
            <body style="margin:0;padding:0;background-color:{BrandBg};font-family:'Noto Sans','Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;">
                <div style="display:none;max-height:0;overflow:hidden;mso-hide:all;font-size:1px;line-height:1px;color:{BrandBg};">
                    Подтвердите email, чтобы начать пользоваться CoffeePeek. Ссылка действует 10 минут.
                </div>

                <table role="presentation" width="100%" border="0" cellspacing="0" cellpadding="0" style="background-color:{BrandBg};">
                    <tr>
                        <td align="center" class="email-wrapper" style="padding:40px 16px;">
                            <table role="presentation" class="email-card" width="480" border="0" cellspacing="0" cellpadding="0" style="max-width:480px;width:100%;border:1px solid {BrandBorder};border-radius:24px;overflow:hidden;background-color:{BrandSurface};box-shadow:0 24px 48px -12px rgba(0,0,0,0.5);">
                                <tr>
                                    <td height="4" style="background:linear-gradient(90deg,#B8860B 0%,{BrandGold} 50%,#B8860B 100%);background-color:{BrandGold};font-size:0;line-height:0;">&nbsp;</td>
                                </tr>
                                <tr>
                                    <td class="email-content" style="padding:40px 36px 32px;text-align:center;">
                                        <table role="presentation" align="center" border="0" cellspacing="0" cellpadding="0" style="margin:0 auto 28px;">
                                            <tr>
                                                <td align="center" style="padding-right:10px;vertical-align:middle;">
                                                    <table role="presentation" border="0" cellspacing="0" cellpadding="0">
                                                        <tr>
                                                            <td align="center" style="width:36px;height:36px;background-color:{BrandBg};border:1px solid {BrandBorder};border-radius:11px;">
                                                                <img src="{LogoUrl}" width="18" height="18" alt="" style="display:block;margin:9px auto;" />
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                                <td align="left" style="vertical-align:middle;font-family:'Segoe UI',system-ui,sans-serif;font-size:18px;font-weight:700;letter-spacing:-0.025em;color:{TextPrimary};">
                                                    Coffee<span style="color:{BrandGold};">Peek</span>
                                                </td>
                                            </tr>
                                        </table>

                                        <table role="presentation" align="center" border="0" cellspacing="0" cellpadding="0" class="email-steps" style="margin:0 auto 28px;">
                                            <tr>
                                                <td style="padding:4px 10px;border-radius:99px;background-color:rgba(234,179,8,0.12);font-family:'Noto Sans','Segoe UI',sans-serif;font-size:11px;font-weight:600;color:{BrandGold};">
                                                    <span style="display:inline-block;width:16px;height:16px;line-height:16px;border-radius:99px;background-color:{BrandGold};color:{BrandBg};font-size:9px;font-weight:700;text-align:center;margin-right:4px;">&#10003;</span>
                                                    Email
                                                </td>
                                                <td width="12" style="font-size:0;line-height:0;">&nbsp;</td>
                                                <td style="padding:4px 10px;border-radius:99px;background-color:rgba(234,179,8,0.12);font-family:'Noto Sans','Segoe UI',sans-serif;font-size:11px;font-weight:600;color:{BrandGold};">
                                                    <span style="display:inline-block;width:16px;height:16px;line-height:16px;border-radius:99px;background-color:{BrandGold};color:{BrandBg};font-size:9px;font-weight:700;text-align:center;margin-right:4px;">2</span>
                                                    Подтверждение
                                                </td>
                                                <td width="12" style="font-size:0;line-height:0;">&nbsp;</td>
                                                <td style="padding:4px 10px;border-radius:99px;background-color:rgba(255,255,255,0.04);border:1px solid {BrandBorder};font-family:'Noto Sans','Segoe UI',sans-serif;font-size:11px;font-weight:600;color:{TextMuted};">
                                                    <span style="display:inline-block;width:16px;height:16px;line-height:16px;border-radius:99px;border:1px solid {BrandBorder};color:{TextMuted};font-size:9px;font-weight:700;text-align:center;margin-right:4px;">3</span>
                                                    Готово
                                                </td>
                                            </tr>
                                        </table>

                                        <table role="presentation" align="center" border="0" cellspacing="0" cellpadding="0" style="margin:0 auto 20px;">
                                            <tr>
                                                <td align="center" style="width:72px;height:72px;border-radius:99px;background-color:rgba(234,179,8,0.10);font-size:32px;line-height:72px;color:{BrandGold};">
                                                    &#9993;
                                                </td>
                                            </tr>
                                        </table>

                                        <h1 class="email-heading" style="margin:0 0 10px;font-family:'Segoe UI',system-ui,sans-serif;font-size:28px;line-height:36px;font-weight:700;color:{TextPrimary};">
                                            Привет, {safeUsername}!
                                        </h1>
                                        <p style="margin:0 0 8px;font-size:14px;line-height:22px;color:{TextMuted};">
                                            Спасибо за регистрацию в CoffeePeek — сервисе для поиска и оценки кофеен.
                                        </p>
                                        <p style="margin:0 0 28px;font-size:14px;line-height:22px;color:{TextMuted};">
                                            Нажмите кнопку ниже, чтобы подтвердить email и активировать аккаунт.
                                            Ссылка действительна <strong style="color:{TextPrimary};font-weight:600;">10 минут</strong>.
                                        </p>

                                        <table role="presentation" align="center" border="0" cellspacing="0" cellpadding="0" style="margin:0 auto 28px;width:100%;max-width:320px;">
                                            <tr>
                                                <td align="center" bgcolor="{BrandGold}" style="border-radius:12px;box-shadow:0 4px 6px -4px rgba(180,140,75,0.2),0 10px 15px -3px rgba(180,140,75,0.2);">
                                                    <a href="{confirmationUrl}" target="_blank" class="email-cta" style="display:block;padding:14px 24px;font-family:'Segoe UI',system-ui,sans-serif;font-size:15px;font-weight:600;color:{BrandBg};text-decoration:none;border-radius:12px;background-color:{BrandGold};">
                                                        Подтвердить email
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <table role="presentation" align="center" border="0" cellspacing="0" cellpadding="0" style="margin:0 auto;width:100%;">
                                            <tr>
                                                <td style="padding:14px 16px;border-radius:12px;background-color:rgba(26,20,18,0.6);border:1px solid {BrandBorder};text-align:left;">
                                                    <p style="margin:0 0 6px;font-size:12px;line-height:18px;color:{TextDim};">
                                                        Если кнопка не работает, скопируйте ссылку:
                                                    </p>
                                                    <p style="margin:0;font-size:11px;line-height:18px;word-break:break-all;">
                                                        <a href="{confirmationUrl}" target="_blank" style="color:{BrandGold};text-decoration:underline;font-family:monospace;">{safeUrlDisplay}</a>
                                                    </p>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:20px 36px 28px;border-top:1px solid {BrandBorder};text-align:center;background-color:{BrandBg};">
                                        <p style="margin:0 0 6px;font-size:12px;line-height:18px;color:{TextDim};">
                                            Если вы не регистрировались в CoffeePeek, проигнорируйте это письмо.
                                        </p>
                                        <p style="margin:0;font-size:12px;line-height:18px;color:{TextDim};">
                                            &copy; {year} CoffeePeek &middot; coffeepeek.by
                                        </p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    public string GetWelcomeBackHtml(string username)
    {
        var safeUsername = WebUtility.HtmlEncode(username);

        return $"""
            <!DOCTYPE html>
            <html lang="ru">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
            </head>
            <body style="margin: 0; padding: 0; background-color: {BrandBg}; font-family: sans-serif; color: {TextPrimary};">
                <table align="center" border="0" cellpadding="0" cellspacing="0" width="100%" style="max-width: 600px; margin-top: 40px; background-color: {BrandBg}; border: 1px solid {BrandBorder}; border-radius: 16px; overflow: hidden;">
                    <tr>
                        <td align="center" style="padding: 40px 0 20px 0; background-color: {BrandSurface};">
                            <div style="width: 60px; height: 60px; background-color: {BrandBg}; border: 1px solid {BrandBorder}; border-radius: 12px; display: inline-block; line-height: 60px; text-align: center;">
                                <span style="font-size: 30px;">&#9749;</span>
                            </div>
                            <h1 style="margin: 20px 0 0 0; color: {TextPrimary}; font-size: 24px; letter-spacing: 1px;">
                                Coffee<span style="color: {BrandGold};">Peek</span>
                            </h1>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding: 40px; text-align: center;">
                            <h2 style="color: {TextPrimary}; font-size: 28px; margin-bottom: 20px;">С возвращением, {safeUsername}!</h2>
                            <p style="color: {TextMuted}; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                                Рады снова видеть вас в CoffeePeek. Ваши любимые кофейни и новые открытия ждут вас.
                            </p>
                            <p style="color: {TextDim}; font-size: 14px; margin-top: 20px;">
                                Если вы не входили в аккаунт, немедленно смените пароль.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style="padding: 30px; background-color: {BrandBg}; border-top: 1px solid {BrandBorder}; text-align: center;">
                            <p style="color: {TextDim}; font-size: 12px; margin: 0;">&copy; {DateTime.UtcNow.Year} CoffeePeek. Все права защищены.</p>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}
