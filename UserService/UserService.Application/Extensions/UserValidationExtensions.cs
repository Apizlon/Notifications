using System.Text.RegularExpressions;
using UserService.Application.Contracts.User;
using UserService.Application.Exceptions;

namespace UserService.Application.Extensions;

public static class UserValidationExtensions
    {
        private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        public static void ValidateRegister(this RegisterRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 6 || request.Username.Length > 50)
                errors.Add("Username must be 6-50 characters long");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6 || request.Password.Length > 50)
                errors.Add("Password must be 6-50 characters long");

            if (!string.IsNullOrEmpty(request.Email) && (!EmailRegex.IsMatch(request.Email) || request.Email.Length > 50))
                errors.Add("Invalid email format");

            if (request.DateOfBirth.HasValue)
            {
                if (request.DateOfBirth.Value.Year < 1900 || request.DateOfBirth.Value > DateTime.UtcNow)
                    errors.Add("Date of birth must be between 1900 and current date");
            }

            if (errors.Any())
                throw new BadRequestException(string.Join("; ", errors));
        }

        public static void ValidateLogin(this LoginRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Username))
                errors.Add("Username is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                errors.Add("Password is required");

            if (errors.Any())
                throw new BadRequestException(string.Join("; ", errors));
        }

        public static void ValidateUpdate(this UpdateUserRequest request)
        {
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(request.Email) && (!EmailRegex.IsMatch(request.Email) || request.Email.Length > 50))
                errors.Add("Invalid email format");

            if (request.DateOfBirth.HasValue)
            {
                if (request.DateOfBirth.Value.Year < 1900 || request.DateOfBirth.Value > DateTime.UtcNow)
                    errors.Add("Date of birth must be between 1900 and current date");
            }

            if (errors.Any())
                throw new BadRequestException(string.Join("; ", errors));
        }
    }