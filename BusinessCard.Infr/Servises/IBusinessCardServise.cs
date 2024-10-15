using BusinessCard_Rahaf.Modals;
using CsvHelper;
using Microsoft.AspNetCore.Http;


namespace BusinessCard.Core.Servises
{
    public interface IBusinessCardServise
    {
        Task<List<BusinessCardInf>> ParseCsvAsync(IFormFile file);
        Task<List<BusinessCardInf>> ParseXmlAsync(IFormFile file);
        BusinessCardInf ParseQrCode(IFormFile qrCodeFile);
        Task<List<BusinessCardInf>> GetAllBusinessCardsAsync();
        Task<BusinessCardInf> GetBusinessCardByIdAsync(int id);
        Task<bool> DeleteBusinessCardAsync(int id);
         Task<List<BusinessCardInf>> GetFilteredBusinessCardsAsync(string name, string email, string phone, string gender, DateTime? dob);
    }
}
