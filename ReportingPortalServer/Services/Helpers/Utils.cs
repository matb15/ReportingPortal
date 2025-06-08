﻿using Models;

namespace ReportingPortalServer.Services.Helpers
{
    public static class Utils
    {
        public static string? GetJwt(HttpContext httpcontext)
        {
            string? authHeader = httpcontext.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader["Bearer ".Length..].Trim();
        }

        public static void GenerateNewVerificationToken(User user, ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            EmailVerificationToken emailVerificationToken = new()
            {
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
            };

            context.EmailVerificationTokens.Add(emailVerificationToken);

            context.SaveChanges();

            string link = configuration.GetSection("FrontAddress").Value + $"/verify-email?token={emailVerificationToken.Token}";

            string logoUrl = configuration["EmailSettings:LogoUrl"] ?? configuration.GetSection("FrontAddress").Value + "/logo.png";
            string supportEmail = configuration["EmailSettings:FromEmail"] ?? "-";

            string emailBody = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f9f9f9;
                            margin: 0; padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background-color: #ffffff;
                            border-radius: 8px;
                            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                            padding: 30px;
                            color: #333333;
                        }}
                        .header {{
                            text-align: center;
                            margin-bottom: 30px;
                        }}
                        .header img {{
                            max-width: 150px;
                        }}
                        h1 {{
                            color: #2c3e50;
                        }}
                        p {{
                            font-size: 16px;
                            line-height: 1.5;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 15px 25px;
                            margin-top: 25px;
                            font-size: 16px;
                            color: white;
                            background-color: #007bff;
                            text-decoration: none;
                            border-radius: 5px;
                        }}
                        .footer {{
                            margin-top: 40px;
                            font-size: 12px;
                            color: #999999;
                            text-align: center;
                        }}
                        a.button:hover {{
                            background-color: #0056b3;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='{logoUrl}' alt='Company Logo' />
                        </div>
                        <h1>Confirm Your Email Address</h1>
                        <p>Hi {user.Name},</p>
                        <p>Thanks for registering! Please confirm your email address by clicking the button below:</p>
                        <a href='{link}' class='button'>Confirm Email</a>
                        <p>If the button doesn't work, copy and paste the following link into your browser:</p>
                        <p><a href='{link}'>{link}</a></p>
                        <p>If you did not sign up for this account, please ignore this email or contact our support.</p>
                        <div class='footer'>
                            <p>Need help? Contact us at <a href='mailto:{supportEmail}'>{supportEmail}</a>.</p>
                            <p>&copy; {DateTime.UtcNow.Year} Your Company. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
                ";

            emailService.SendEmail(user.Email, "Confirm your email", emailBody);
        }

        public static void GenerateNewResetPasswordToken(User user, ApplicationDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            ResetPasswordToken resetPasswordToken = new()
            {
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
            };

            context.PasswordResetTokens.Add(resetPasswordToken);

            context.SaveChanges();

            string link = configuration.GetSection("FrontAddress").Value + $"/reset-password?token={resetPasswordToken.Token}";

            string logoUrl = configuration["EmailSettings:LogoUrl"] ?? configuration.GetSection("FrontAddress").Value + "/logo.png";
            string supportEmail = configuration["EmailSettings:FromEmail"] ?? "-";

            string emailBody = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f9f9f9;
                            margin: 0; padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background-color: #ffffff;
                            border-radius: 8px;
                            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                            padding: 30px;
                            color: #333333;
                        }}
                        .header {{
                            text-align: center;
                            margin-bottom: 30px;
                        }}
                        .header img {{
                            max-width: 150px;
                        }}
                        h1 {{
                            color: #2c3e50;
                        }}
                        p {{
                            font-size: 16px;
                            line-height: 1.5;
                        }}
                        .button {{
                            display: inline-block;
                            padding: 15px 25px;
                            margin-top: 25px;
                            font-size: 16px;
                            color: white;
                            background-color: #007bff;
                            text-decoration: none;
                            border-radius: 5px;
                        }}
                        .footer {{
                            margin-top: 40px;
                            font-size: 12px;
                            color: #999999;
                            text-align: center;
                        }}
                        a.button:hover {{
                            background-color: #0056b3;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <img src='{logoUrl}' alt='Company Logo' />
                        </div>
                        <h1>Reset Your Password</h1>
                        <p>Hi {user.Name},</p>
                        <p>We received a request to reset your password. Please click the button below to reset it:</p>
                        <a href='{link}' class='button'>Confirm Email</a>
                        <p>If the button doesn't work, copy and paste the following link into your browser:</p>
                        <p><a href='{link}'>{link}</a></p>
                        <p>If you did request a new password for this account, please ignore this email or contact our support.</p>
                        <div class='footer'>
                            <p>Need help? Contact us at <a href='mailto:{supportEmail}'>{supportEmail}</a>.</p>
                            <p>&copy; {DateTime.UtcNow.Year} Your Company. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
                ";

            emailService.SendEmail(user.Email, "Reset Password", emailBody);
        }

    }
}
