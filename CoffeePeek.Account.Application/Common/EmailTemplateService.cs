using System.Net;
using CoffeePeek.Account.Application.Common.Interfaces;

namespace CoffeePeek.Account.Application.Common;

public class EmailTemplateService : IEmailTemplateService
{
    public string GetConfirmationHtml(string username, string confirmationUrl)
    {
        var safeUsername = WebUtility.HtmlEncode(username);

        return $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin: 0; padding: 0; background-color: #1A1412; font-family: sans-serif; color: #F5F5F5;">
            <table align="center" border="0" cellpadding="0" cellspacing="0" width="100%" style="max-width: 600px; margin-top: 40px; background-color: #1A1412; border: 1px solid #3D2F28; border-radius: 16px; overflow: hidden;">
                <tr>
                    <td align="center" style="padding: 40px 0 20px 0; background-color: #2D241F;">
                        <div style="width: 60px; height: 60px; background-color: #1A1412; border: 1px solid #3D2F28; border-radius: 12px; display: inline-block; line-height: 60px; text-align: center;">
                            <span style="font-size: 30px;">☕</span>
                        </div>
                        <h1 style="margin: 20px 0 0 0; color: #FFFFFF; font-size: 24px; letter-spacing: 1px;">
                            Coffee<span style="color: #EAB308;">Peek</span>
                        </h1>
                    </td>
                </tr>
                <tr>
                    <td style="padding: 40px; text-align: center;">
                        <h2 style="color: #FFFFFF; font-size: 28px; margin-bottom: 20px;">Привет, {safeUsername}!</h2>
                        <p style="color: #A39E93; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                            Чтобы начать свое знакомство с лучшими в мире сортами кофе, пожалуйста, подтвердите свой адрес электронной почты.
                            Эта ссылка действительна в течении 10 минут.
                        </p>
                        <table align="center" border="0" cellpadding="0" cellspacing="0">
                            <tr>
                                <td align="center" bgcolor="#EAB308" style="border-radius: 8px;">
                                    <a href="{confirmationUrl}" target="_blank" style="display: inline-block; padding: 16px 36px; font-size: 16px; color: #1A1412; font-weight: bold; text-decoration: none; border-radius: 8px;">
                                        Подтвердить почту
                                    </a>
                                </td>
                            </tr>
                        </table>
                        <p style="color: #5C544F; font-size: 14px; margin-top: 40px;">
                            Если кнопка не работает, скопируйте и вставьте эту ссылку: <br>
                            <a href="{confirmationUrl}" style="color: #EAB308; text-decoration: none;">{confirmationUrl}</a>
                        </p>
                    </td>
                </tr>
                <tr>
                    <td style="padding: 30px; background-color: #1A1412; border-top: 1px solid #3D2F28; text-align: center;">
                        <p style="color: #5C544F; font-size: 12px; margin: 0;">© 2025 CoffeePeek. Все права защищены.</p>
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
        <html>
        <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
        </head>
        <body style="margin: 0; padding: 0; background-color: #1A1412; font-family: sans-serif; color: #F5F5F5;">
            <table align="center" border="0" cellpadding="0" cellspacing="0" width="100%" style="max-width: 600px; margin-top: 40px; background-color: #1A1412; border: 1px solid #3D2F28; border-radius: 16px; overflow: hidden;">
                <tr>
                    <td align="center" style="padding: 40px 0 20px 0; background-color: #2D241F;">
                        <div style="width: 60px; height: 60px; background-color: #1A1412; border: 1px solid #3D2F28; border-radius: 12px; display: inline-block; line-height: 60px; text-align: center;">
                            <span style="font-size: 30px;">☕</span>
                        </div>
                        <h1 style="margin: 20px 0 0 0; color: #FFFFFF; font-size: 24px; letter-spacing: 1px;">
                            Coffee<span style="color: #EAB308;">Peek</span>
                        </h1>
                    </td>
                </tr>
                <tr>
                    <td style="padding: 40px; text-align: center;">
                        <h2 style="color: #FFFFFF; font-size: 28px; margin-bottom: 20px;">С возвращением, {safeUsername}! ☕</h2>
                        <p style="color: #A39E93; font-size: 16px; line-height: 1.6; margin-bottom: 30px;">
                            Рады снова видеть вас в CoffeePeek. Ваши любимые кофейни и новые открытия ждут вас.
                        </p>
                        <p style="color: #5C544F; font-size: 14px; margin-top: 20px;">
                            Если вы не входили в аккаунт, немедленно смените пароль.
                        </p>
                    </td>
                </tr>
                <tr>
                    <td style="padding: 30px; background-color: #1A1412; border-top: 1px solid #3D2F28; text-align: center;">
                        <p style="color: #5C544F; font-size: 12px; margin: 0;">© 2025 CoffeePeek. Все права защищены.</p>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
    }
}