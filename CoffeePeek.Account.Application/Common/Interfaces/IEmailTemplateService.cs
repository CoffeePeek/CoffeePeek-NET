namespace CoffeePeek.Account.Application.Common.Interfaces;

public interface IEmailTemplateService
{
    string GetConfirmationHtml(string username, string confirmationUrl);
    string GetWelcomeBackHtml(string username);
}