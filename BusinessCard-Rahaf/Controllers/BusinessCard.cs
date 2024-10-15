using BusinessCard.Core.Repository;
using BusinessCard.Core.Servises;
using BusinessCard_Rahaf.Modals;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace BusinessCard_Rahaf.Controllers
{
    public class BusinessCard : Controller
    {
        private readonly IBusinessCardServise _businessCardService;
        private readonly DbCardContext _context;

        public BusinessCard(IBusinessCardServise businessCardService)
        {
            _businessCardService = businessCardService;
        }

        public BusinessCard(DbCardContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateBusinessCard([FromBody] BusinessCardInf businessCardInf)
        {
            if (ModelState.IsValid)
            {
                // Normally, save this business card to the database
                return Ok(new { message = "Business card created successfully!" });
            }
            return BadRequest(ModelState);
        }

        [HttpPost("create/csv")]
        public async Task<IActionResult> CreateBusinessCardFromCsv([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            var businessCards = await _businessCardService.ParseCsvAsync(file);
            // Save the parsed business cards to the database
            return Ok(businessCards);
        }

        [HttpPost("create/xml")]
        public async Task<IActionResult> CreateBusinessCardFromXml([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            var businessCards = await _businessCardService.ParseXmlAsync(file);
            // Save the parsed business cards to the database
            return Ok(businessCards);
        }

        [HttpPost("create/qrcode")]
        public IActionResult createbusinesscardfromqrcode([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("invalid file");

            var businesscard = _businessCardService.ParseQrCode(file);
            if (businesscard != null)
            {
                // save the business card to the database
                return Ok(businesscard);
            }
            return BadRequest("qr code could not be read");
        }

        //[HttpGet("list")]
        //public async Task<IActionResult> GetAllBusinessCards()
        //{
        //    var businessCards = await _businessCardService.GetAllBusinessCardsAsync();

        //    if (businessCards == null || businessCards.Count == 0)
        //    {
        //        return NotFound(new { message = "No business cards found." });
        //    }

        //    return Ok(businessCards);
        //}


        [HttpGet("list")]
        public async Task<IActionResult> GetAllBusinessCards(
          [FromQuery] string name,
          [FromQuery] string email,
          [FromQuery] string gender)
        {
            var query = _context.BusinessCards.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(b => b.Email.Contains(email));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(b => b.Gender.Equals(gender, StringComparison.OrdinalIgnoreCase));
            }

            var businessCards = await query.ToListAsync();

            if (businessCards == null || businessCards.Count == 0)
            {
                return NotFound(new { message = "No business cards found." });
            }

            return Ok(businessCards);
        }


        // DELETE: api/businesscard/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBusinessCard(int id)
        {
            var businessCard = await _businessCardService.GetBusinessCardByIdAsync(id);
            if (businessCard == null)
            {
                return NotFound(new { message = "Business card not found." });
            }

            var result = await _businessCardService.DeleteBusinessCardAsync(id);

            if (result)
            {
                return Ok(new { message = "Business card deleted successfully." });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to delete the business card." });
            }
        }

        public string GenerateCsv(List<BusinessCardInf> businessCards)
        {
            var csv = new StringBuilder();

            // Add header line
            csv.AppendLine("Id,Name,Gender,DateOfBirth,Email,Phone,Address");

            // Add data lines
            foreach (var card in businessCards)
            {
                csv.AppendLine($"{card.Id},{card.Name},{card.Gender},{card.DateOfBirth:yyyy-MM-dd},{card.Email},{card.Phone},{card.Address}");
            }

            return csv.ToString();
        }

        public string GenerateXml(List<BusinessCardInf> businessCards)
        {
            var serializer = new XmlSerializer(typeof(List<BusinessCard>));

            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, businessCards);
                return stringWriter.ToString();
            }
        }

        // GET: api/businesscard/export?format=csv OR api/businesscard/export?format=xml
        [HttpGet("export")]
        public async Task<IActionResult> ExportBusinessCards([FromQuery] string format)
        {
            var businessCards = await _businessCardService.GetAllBusinessCardsAsync();

            if (businessCards == null || !businessCards.Any())
            {
                return NotFound(new { message = "No business cards found to export." });
            }

            switch (format.ToLower())
            {
                case "csv":
                    var csvContent = GenerateCsv(businessCards);
                    var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                    return File(csvBytes, "text/csv", "BusinessCards.csv");

                case "xml":
                    var xmlContent = GenerateXml(businessCards);
                    var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
                    return File(xmlBytes, "application/xml", "BusinessCards.xml");

                default:
                    return BadRequest(new { message = "Invalid format. Use 'csv' or 'xml'." });
            }
        }

        // GET: api/businesscard/export?format=csv&name=John&gender=male&dob=1990-01-01
        [HttpGet("export")]
        public async Task<IActionResult> ExportBusinessCards(
            [FromQuery] string format,
            [FromQuery] string name,
            [FromQuery] string email,
            [FromQuery] string phone,
            [FromQuery] string gender,
            [FromQuery] DateTime? dob)
        {
            var businessCards = await _businessCardService.GetFilteredBusinessCardsAsync(name, email, phone, gender, dob);

            if (businessCards == null || !businessCards.Any())
            {
                return NotFound(new { message = "No business cards found for the given filters." });
            }

            switch (format.ToLower())
            {
                case "csv":
                    var csvContent = GenerateCsv(businessCards);
                    var csvBytes = Encoding.UTF8.GetBytes(csvContent);
                    return File(csvBytes, "text/csv", "FilteredBusinessCards.csv");

                case "xml":
                    var xmlContent = GenerateXml(businessCards);
                    var xmlBytes = Encoding.UTF8.GetBytes(xmlContent);
                    return File(xmlBytes, "application/xml", "FilteredBusinessCards.xml");

                default:
                    return BadRequest(new { message = "Invalid format. Use 'csv' or 'xml'." });
            }
        }



    }
}
