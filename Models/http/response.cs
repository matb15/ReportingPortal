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
        public string Token { get; set; }
        public User User { get; set; }
    }
}
