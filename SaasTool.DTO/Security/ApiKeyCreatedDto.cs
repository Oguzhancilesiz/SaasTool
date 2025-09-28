using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DTO.Security
{
    /// <summary>
    /// API anahtarı oluşturulduğunda yalnızca bir kez dönen yanıt.
    /// DİKKAT: Key plaintext sadece burada görünecek, DB’de hash saklanır.
    /// </summary>
    public sealed class ApiKeyCreatedDto
    {
        public Guid Id { get; set; }
        /// <summary>Plaintext anahtar (sadece ilk oluşturma cevabında gönderilir)</summary>
        public string Key { get; set; } = null!;
        /// <summary>Son 4 karakter (listelemelerde gösterilebilir)</summary>
        public string KeyLast4 { get; set; } = "****";
    }
}
