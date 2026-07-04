namespace LeaveManagement.Application.Interfaces
{
    /// <summary>
    /// Provides access to the security context of the currently authenticated user.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the unique integer ID of the currently authenticated user.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the user ID claim is missing or invalid.</exception>
        public int UserId { get; }

        /// <summary>
        /// Determines whether the current user belongs to the specified security role.
        /// </summary>
        /// <param name="role">The name of the role to check.</param>
        /// <returns>True if the user is in the role; otherwise, false.</returns>
        bool IsInRole(string role);
    }
}
