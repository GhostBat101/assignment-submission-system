/*
 * File: IJwtTokenGenerator.cs
 * Purpose: Interface defining the service contract for generating JSON Web Tokens (JWT).
 * 
 * Dependencies Used:
 * - User.cs: User entity containing identity data (Id, Email, Role).
 * 
 * Used By:
 * - JwtTokenGenerator.cs: Implements the token generation algorithm.
 * - AuthService.cs: Consumes this interface to issue tokens on successful authentication.
 */

using AssignmentSubmission.Core.Entities;

namespace AssignmentSubmission.Services.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
