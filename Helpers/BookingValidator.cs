using BloomyBE.Configuration;

namespace BloomyBE.Helpers
{
    public static class BookingValidator
    {
        public static bool IsValidServiceArea(string address, string district, BookingSettings settings, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(district))
            {
                errorMessage = "Vui lòng nhập đầy đủ địa chỉ và quận/huyện.";
                return false;
            }

            if (address.Trim().Length < 10)
            {
                errorMessage = "Địa chỉ phải có ít nhất 10 ký tự.";
                return false;
            }

            if (address.Trim().Length > 200)
            {
                errorMessage = "Địa chỉ không được vượt quá 200 ký tự.";
                return false;
            }

            // Only check if district is in the allowed list
            if (!settings.AllowedDistricts.Contains(district))
            {
                errorMessage = $"Bloomy chỉ phục vụ tại 3 khu vực: {string.Join(", ", settings.AllowedDistricts)}.";
                return false;
            }

            return true;
        }

        public static bool IsFutureOrToday(DateTime eventDate, out string errorMessage)
        {
            errorMessage = string.Empty;
            var today = DateTime.UtcNow.Date;
            if (eventDate.Date < today)
            {
                errorMessage = $"Ngày tổ chức phải là hôm nay ({today:dd/MM/yyyy}) hoặc trong tương lai.";
                return false;
            }

            // Max 365 days in the future
            var maxDate = today.AddYears(1);
            if (eventDate.Date > maxDate)
            {
                errorMessage = "Ngày tổ chức sự kiện không được vượt quá 1 năm kể từ hôm nay.";
                return false;
            }

            return true;
        }
    }
}
