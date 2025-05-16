namespace Models.http
{
    public class Response : HttpResponseMessage
    {
        public required string Message { get; set; }
    }

    public class RegisterResponse : Response
    {
    }

    public class LoginResponse : Response
    {
        public required string Token { get; set; }
        public required User User { get; set; }
    }
}
