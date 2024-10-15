using BusinessCard.Core.Servises;
using Microsoft.AspNetCore.Http;
using System.Xml.Serialization;
using BusinessCard_Rahaf.Modals;
using System.Drawing;
using Microsoft.EntityFrameworkCore;
using ZXing;


namespace BusinessCard.Core.Repository
{
    public class BusinessCardRepository: IBusinessCardServise
    {
        private readonly DbCardContext _context;

        public BusinessCardRepository(DbCardContext context)
        {
            _context = context;
        }
        public async Task<List<BusinessCardInf>> ParseCsvAsync(IFormFile file)
        {
            var businessCards = new List<BusinessCardInf>();

            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                var csv = await stream.ReadToEndAsync();
                var lines = csv.Split('\n');

                foreach (var line in lines.Skip(1))  // Skip header row
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');
                    var card = new BusinessCardInf
                    {
                        Name = values[0],
                        Gender = values[1],
                        DateOfBirth = DateTime.Parse(values[2]),
                        Email = values[3],
                        Phone = values[4],
                        Address = values[5]
                    };

                    businessCards.Add(card);
                }
            }

            return businessCards;
        }

        public async Task<List<BusinessCardInf>> ParseXmlAsync(IFormFile file)
        {
            var businessCards = new List<BusinessCardInf>();

            using (var stream = file.OpenReadStream())
            {
                var xmlSerializer = new XmlSerializer(typeof(List<BusinessCardInf>));
                var cards = (List<BusinessCardInf>)xmlSerializer.Deserialize(stream);
                businessCards.AddRange(cards);
            }

            return businessCards;
        }

        public BusinessCardInf ParseQrCode(IFormFile qrCodeFile)
        {
            using (var stream = qrCodeFile.OpenReadStream())
            {
                var barcodeReader = new ZXing.Windows.Compatibility.BarcodeReader();
                var bitmap = (Bitmap)System.Drawing.Image.FromStream(stream);
                var result = barcodeReader.Decode(bitmap);

                if (result != null)
                {
                    var cardData = result.Text.Split(';');  // Assuming QR code data is formatted in a simple delimiter format
                    return new BusinessCardInf
                    {
                        Name = cardData[0],
                        Gender = cardData[1],
                        DateOfBirth = DateTime.Parse(cardData[2]),
                        Email = cardData[3],
                        Phone = cardData[4],
                        Address = cardData[5]
                    };
                }
            }

            return null;
        }

        public async Task<List<BusinessCardInf>> GetAllBusinessCardsAsync()
        {
            return await _context.BusinessCards.ToListAsync();
        }

        // Get a business card by ID
        public async Task<BusinessCardInf> GetBusinessCardByIdAsync(int id)
        {
            return await _context.BusinessCards.FindAsync(id);
        }

        // Delete a business card by ID
        public async Task<bool> DeleteBusinessCardAsync(int id)
        {
            var businessCard = await _context.BusinessCards.FindAsync(id);
            if (businessCard == null)
            {
                return false;
            }

            _context.BusinessCards.Remove(businessCard);
            await _context.SaveChangesAsync();
            return true;
        }

        // Method to get filtered business cards
        public async Task<List<BusinessCardInf>> GetFilteredBusinessCardsAsync(string name, string email, string phone, string gender, DateTime? dob)
        {
            var query = _context.BusinessCards.AsQueryable();

            // Apply filters if they are provided
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(b => b.Email.Contains(email));
            }

            if (!string.IsNullOrEmpty(phone))
            {
                query = query.Where(b => b.Phone.Contains(phone));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(b => b.Gender.Equals(gender, StringComparison.OrdinalIgnoreCase));
            }

            if (dob.HasValue)
            {
                query = query.Where(b => b.DateOfBirth.Date == dob.Value.Date);
            }

            return await query.ToListAsync();
        }
    }
}
