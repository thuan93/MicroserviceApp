namespace Shared.Constants;

public static class ApiConstants
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;
    
    public static class StatusCodes
    {
        public const string Success = "SUCCESS";
        public const string Error = "ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string ValidationError = "VALIDATION_ERROR";
    }
    
    public static class Messages
    {
        public const string CreatedSuccessfully = "Created successfully";
        public const string UpdatedSuccessfully = "Updated successfully";
        public const string DeletedSuccessfully = "Deleted successfully";
        public const string NotFound = "Resource not found";
        public const string ValidationFailed = "Validation failed";
    }
}
