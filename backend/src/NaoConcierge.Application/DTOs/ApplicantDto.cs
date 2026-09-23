namespace NaoConcierge.Application.DTOs;

public record ApplicantDto(
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? Suffix,
    DateTime? DateOfBirth,
    string? CitizenshipStatus,
    string? Email,
    string? MobilePhone,
    string? EmploymentStatus,
    string? EmployerName);
