/*
 * File: IAuthService.cs
 * Purpose: Interface definition for user authentication operations (Login, Profile Lookup).
 * 
 * Dependencies Used:
 * - AuthDtos.cs: LoginRequestDto, LoginResponseDto, UserDto data contracts.
 * 
 * Used By:
 * - AuthService.cs: Implementation class.
 * - AuthController.cs: Consumed in HTTP endpoint methods.
 */

using System.Threading.Tasks;
using AssignmentSubmission.Core.DTOs;

namespace AssignmentSubmission.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<UserDto?> GetUserProfileAsync(int userId);
    }
}
