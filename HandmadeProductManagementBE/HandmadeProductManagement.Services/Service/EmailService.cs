using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HandmadeProductManagement.Services.Service;

public class EmailService(
    IConfiguration config,
    UserManager<ApplicationUser> userManager,
    IUserAgentService userAgentService
)
    : IEmailService
{
    private readonly string _companyName = "Handmade Product Shop";
    private readonly string _hostMail = config.GetSection("EmailCredential").GetSection("mail").Value!;
    private readonly string _password = config.GetSection("EmailCredential").GetSection("password").Value!;

    public async Task SendEmailConfirmationAsync(string destMail, string clientUri)
    {
        var user = await userManager.FindByEmailAsync(destMail);
        var subject = $"HANDMADE PRODUCT SHOP CONFIRMATION";
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
        var encodedToken = HttpUtility.UrlEncode(token);
        var emailConfirmationLink = $"{clientUri}?email={user!.Email}&token={encodedToken}";
        var body = GenerateEmailConfirmationHtml(user, emailConfirmationLink);

        await SendEmailAsync(destMail, subject, body, emailConfirmationLink);
    }


    public async Task SendPasswordRecoveryEmailAsync(string destMail, string passwordResetLink)
    {
        var user = await userManager.Users
            .Include(u => u.UserInfo)
            .SingleOrDefaultAsync(u => u.Email == destMail && !u.DeletedTime.HasValue && u.DeletedBy == null);

        // var fromAddress = new MailAddress(_hostMail, _companyName);
        // var toAddress = new MailAddress(destMail, "Customer");
        var subject = $"HANDMADE PRODUCT SHOP PASSWORD RECOVERY FOR ACCOUNT {destMail}";
        var emailContent = GeneratePasswordRecoveryHtml(user!, passwordResetLink);
        await SendEmailAsync(destMail, subject, emailContent, passwordResetLink);
    }

    private async Task SendEmailAsync(string destMail, string subject, string emailContent, string featureUrl)
    {
        var fromAddress = new MailAddress(_hostMail, _companyName);
        var toAddress = new MailAddress(destMail, "Customer");
        var smtp = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, _password)
        };

        using (var message = new MailMessage(fromAddress, toAddress))
        {
            message.Subject = subject;
            message.Body = emailContent;
            message.IsBodyHtml = true;
            await smtp.SendMailAsync(message);
        }
    }

    private string GenerateEmailConfirmationHtml(ApplicationUser user, string emailConfirmationLink)
    {
        var email = user.Email;
        var result = new StringBuilder();

        var header = $@"<!DOCTYPE html>
<html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"">

<head>
	<title></title>
	<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""><!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]--><!--[if !mso]><!-->
	<link href=""https://fonts.googleapis.com/css2?family=Montserrat:wght@100;200;300;400;500;600;700;800;900"" rel=""stylesheet"" type=""text/css"">
	<link href=""https://fonts.googleapis.com/css2?family=Playfair+Display:wght@100;200;300;400;500;600;700;800;900"" rel=""stylesheet"" type=""text/css""><!--<![endif]-->
	<style>
		* {{
			box-sizing: border-box;
		}}

		body {{
			margin: 0;
			padding: 0;
		}}

		a[x-apple-data-detectors] {{
			color: inherit !important;
			text-decoration: inherit !important;
		}}

		#MessageViewBody a {{
			color: inherit;
			text-decoration: none;
		}}

		p {{
			line-height: inherit
		}}

		.desktop_hide,
		.desktop_hide table {{
			mso-hide: all;
			display: none;
			max-height: 0px;
			overflow: hidden;
		}}

		.image_block img+div {{
			display: none;
		}}

		sup,
		sub {{
			line-height: 0;
			font-size: 75%;
		}}

		.menu_block.desktop_hide .menu-links span {{
			mso-hide: all;
		}}

		@media (max-width:640px) {{
			.desktop_hide table.icons-outer {{
				display: inline-table !important;
			}}

			.desktop_hide table.icons-inner,
			.social_block.desktop_hide .social-table {{
				display: inline-block !important;
			}}

			.icons-inner {{
				text-align: center;
			}}

			.icons-inner td {{
				margin: 0 auto;
			}}

			.image_block div.fullWidth {{
				max-width: 100% !important;
			}}

			.mobile_hide {{
				display: none;
			}}

			.row-content {{
				width: 100% !important;
			}}

			.stack .column {{
				width: 100%;
				display: block;
			}}

			.mobile_hide {{
				min-height: 0;
				max-height: 0;
				max-width: 0;
				overflow: hidden;
				font-size: 0px;
			}}

			.desktop_hide,
			.desktop_hide table {{
				display: table !important;
				max-height: none !important;
			}}

			.reverse {{
				display: table;
				width: 100%;
			}}

			.reverse .column.last {{
				display: table-header-group !important;
			}}

			.row-6 td.column.last .border {{
				padding: 25px 20px 20px;
				border-top: 0;
				border-right: 0px;
				border-bottom: 0;
				border-left: 0;
			}}

			.row-5 .column-1 .block-1.spacer_block {{
				height: 40px !important;
			}}

			.row-1 .column-1 {{
				padding: 20px 20px 5px !important;
			}}

			.row-1 .column-3 {{
				padding: 5px 20px 25px !important;
			}}
		}}
	</style><!--[if mso ]><style>sup, sub {{ font-size: 100% !important; }} sup {{ mso-text-raise:10% }} sub {{ mso-text-raise:-10% }}</style> <![endif]-->
</head>";


        var body =
            $@"<body class=""body"" style=""background-color: #e6e6e6; margin: 0; padding: 0; -webkit-text-size-adjust: none; text-size-adjust: none;"">
	<table class=""nl-container"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #e6e6e6;"">
		<tbody>
			<tr>
				<td>
					<table class=""row row-1"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff; border-radius: 0; color: #000000; width: 620px; margin: 0 auto;"" width=""620"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""25%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: middle; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""icons_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: center; line-height: 0;"">
														<tr>
															<td class=""pad"" style=""vertical-align: middle; color: #000000; font-family: inherit; font-size: 14px; font-weight: 400; text-align: center;"">
																<table class=""icons-outer"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-table;"">
																	<tr>
																	</tr>
																</table>
															</td>
														</tr>
													</table>
												</td>
												<td class=""column column-2"" width=""50%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: middle; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""menu_block block-1"" width=""100%"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"">
																<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																	<tr>
																		<td class=""alignment"" style=""text-align:center;font-size:0px;"">
																			<div class=""menu-links""><!--[if mso]><table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" align=""center"" style=""""><tr style=""text-align:center;""><![endif]--><!--[if mso]><td style=""padding-top:5px;padding-right:5px;padding-bottom:5px;padding-left:5px""><![endif]--><a href=""https://example.com"" target=""_self"" style=""mso-hide:false;padding-top:5px;padding-bottom:5px;padding-left:5px;padding-right:5px;display:inline-block;color:#101112;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:16px;text-decoration:none;letter-spacing:normal;"">About</a><!--[if mso]></td><![endif]--><!--[if mso]><td style=""padding-top:5px;padding-right:5px;padding-bottom:5px;padding-left:5px""><![endif]--><a href=""https://example.com"" target=""_self"" style=""mso-hide:false;padding-top:5px;padding-bottom:5px;padding-left:5px;padding-right:5px;display:inline-block;color:#101112;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:16px;text-decoration:none;letter-spacing:normal;"">News</a><!--[if mso]></td><![endif]--><!--[if mso]><td style=""padding-top:5px;padding-right:5px;padding-bottom:5px;padding-left:5px""><![endif]--><a href=""https://example.com"" target=""_self"" style=""mso-hide:false;padding-top:5px;padding-bottom:5px;padding-left:5px;padding-right:5px;display:inline-block;color:#101112;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:16px;text-decoration:none;letter-spacing:normal;"">Shop</a><!--[if mso]></td><![endif]--><!--[if mso]></tr></table><![endif]--></div>
																		</td>
																	</tr>
																</table>
															</td>
														</tr>
													</table>
												</td>
												<td class=""column column-3"" width=""25%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: middle; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""social_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-left:5px;padding-right:5px;text-align:center;"">
																<div class=""alignment"" align=""center"">
																	<table class=""social-table"" width=""72px"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-block;"">
																		<tr>
																			<td style=""padding:0 2px 0 2px;""><a href=""https://www.facebook.com/"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-only-logo-dark-gray/facebook@2x.png"" width=""32"" height=""auto"" alt=""facebook"" title=""facebook"" style=""display: block; height: auto; border: 0;""></a></td>
																			<td style=""padding:0 2px 0 2px;""><a href=""https://www.instagram.com/"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-only-logo-dark-gray/instagram@2x.png"" width=""32"" height=""auto"" alt=""instagram"" title=""instagram"" style=""display: block; height: auto; border: 0;""></a></td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-2"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff; color: #000000; width: 620px; margin: 0 auto;"" width=""620"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 40px; padding-left: 20px; padding-right: 20px; padding-top: 40px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""heading_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-bottom:10px;padding-top:10px;text-align:center;width:100%;"">
																<h1 style=""margin: 0; color: #393d47; direction: ltr; font-family: 'Playfair Display', Georgia, serif; font-size: 38px; font-weight: 400; letter-spacing: normal; line-height: 120%; text-align: center; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 45.6px;""><span class=""tinyMce-placeholder"" style=""word-break: break-word;"">Confirm Your Email!&nbsp;</span></h1>
															</td>
														</tr>
													</table>
													<table class=""image_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""width:100%;padding-right:0px;padding-left:0px;"">
																<div class=""alignment"" align=""center"" style=""line-height:10px"">
																	<div class=""fullWidth"" style=""max-width: 232px;""><img src=""https://d1oco4z2z1fhwp.cloudfront.net/templates/default/8146/confirm.png"" style=""display: block; height: auto; border: 0; width: 100%;"" width=""232"" alt=""Confirm subscription"" title=""Confirm subscription"" height=""auto""></div>
																</div>
															</td>
														</tr>
													</table>
													<table class=""paragraph_block block-3"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
														<tr>
															<td class=""pad"" style=""padding-bottom:10px;padding-top:20px;"">
																<div style=""color:#393d47;direction:ltr;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:16px;font-weight:400;letter-spacing:0px;line-height:150%;text-align:center;mso-line-height-alt:24px;"">
																	<p style=""margin: 0;"">We're excited to have you on board! Before we get started, we need you to confirm your email. This ensures that you'll receive all our exclusive offers and updates.</p>
																</div>
															</td>
														</tr>
													</table>
													<table class=""button_block block-4"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-bottom:10px;padding-top:10px;text-align:center;"">
																<div class=""alignment"" align=""center""><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""https://example.com"" style=""height:43px;width:106px;v-text-anchor:middle;"" arcsize=""0%"" strokeweight=""0.75pt"" strokecolor=""#4D4997"" fillcolor=""#4d4997"">
<w:anchorlock/>
<v:textbox inset=""0px,0px,0px,0px"">
<center dir=""false"" style=""color:#ffffff;font-family:Tahoma, sans-serif;font-size:16px"">
<![endif]--><a href={emailConfirmationLink} target=""_blank"" style=""background-color:#4d4997;border-bottom:1px solid #4D4997;border-left:1px solid #4D4997;border-radius:0px;border-right:1px solid #4D4997;border-top:1px solid #4D4997;color:#ffffff;display:inline-block;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:16px;font-weight:400;mso-border-alt:none;padding-bottom:5px;padding-top:5px;text-align:center;text-decoration:none;width:auto;word-break:keep-all;""><span style=""word-break: break-word; padding-left: 20px; padding-right: 20px; font-size: 16px; display: inline-block; letter-spacing: normal;""><span style=""word-break: break-word; line-height: 32px;"">Confirm</span></span></a><!--[if mso]></center></v:textbox></v:roundrect><![endif]--></div>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-3"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #eeeeee; border-left: 20px solid #FFFFFF; border-radius: 0; border-right: 20px solid #FFFFFF; color: #000000; width: 620px; margin: 0 auto;"" width=""620"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 20px; padding-left: 20px; padding-right: 20px; padding-top: 30px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""heading_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-bottom:10px;padding-top:10px;text-align:center;width:100%;"">
																<h2 style=""margin: 0; color: #393d47; direction: ltr; font-family: 'Playfair Display', Georgia, serif; font-size: 30px; font-weight: 400; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 36px;""><span class=""tinyMce-placeholder"" style=""word-break: break-word;"">Download Our App</span></h2>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-4"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #eeeeee; border-left: 20px solid #FFFFFF; border-radius: 0; border-right: 20px solid #FFFFFF; color: #000000; width: 620px; margin: 0 auto;"" width=""620"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""41.666666666666664%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""image_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""width:100%;"">
																<div class=""alignment"" align=""center"" style=""line-height:10px"">
																	<div style=""max-width: 201.667px;""><img src=""https://d1oco4z2z1fhwp.cloudfront.net/templates/default/8146/download_app.png"" style=""display: block; height: auto; border: 0; width: 100%;"" width=""201.667"" alt=""Download our app"" title=""Download our app"" height=""auto""></div>
																</div>
															</td>
														</tr>
													</table>
												</td>
												<td class=""column column-2"" width=""58.333333333333336%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""button_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-top:20px;text-align:center;"">
																<div class=""alignment"" align=""center""><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""https://example.com"" style=""height:59px;width:298px;v-text-anchor:middle;"" arcsize=""0%"" strokeweight=""0.75pt"" strokecolor=""#4D4997"" fill=""false"">
<w:anchorlock/>
<v:textbox inset=""0px,0px,0px,0px"">
<center dir=""false"" style=""color:#4d4997;font-family:Tahoma, sans-serif;font-size:16px"">
<![endif]--><a href=""https://example.com"" target=""_blank"" style=""background-color:transparent;border-bottom:1px solid #4D4997;border-left:1px solid #4D4997;border-radius:0px;border-right:1px solid #4D4997;border-top:1px solid #4D4997;color:#4d4997;display:block;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:16px;font-weight:400;mso-border-alt:none;padding-bottom:10px;padding-top:10px;text-align:center;text-decoration:none;width:100%;word-break:keep-all;""><span style=""word-break: break-word; padding-left: 20px; padding-right: 20px; font-size: 16px; display: inline-block; letter-spacing: normal;""><span style=""word-break: break-word;"">Download <span style=""word-break: break-word; line-height: 19.2px;"" data-mce-style>on </span></span><span style=""word-break: break-word;""><br><span style=""word-break: break-word; line-height: 19.2px;"" data-mce-style>AppStore</span></span></span></a><!--[if mso]></center></v:textbox></v:roundrect><![endif]--></div>
															</td>
														</tr>
													</table>
													<table class=""button_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-top:20px;text-align:center;"">
																<div class=""alignment"" align=""center""><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""https://example.com"" style=""height:59px;width:298px;v-text-anchor:middle;"" arcsize=""0%"" strokeweight=""0.75pt"" strokecolor=""#4D4997"" fill=""false"">
<w:anchorlock/>
<v:textbox inset=""0px,0px,0px,0px"">
<center dir=""false"" style=""color:#4d4997;font-family:Tahoma, sans-serif;font-size:16px"">
<![endif]--><a href=""https://example.com"" target=""_blank"" style=""background-color:transparent;border-bottom:1px solid #4D4997;border-left:1px solid #4D4997;border-radius:0px;border-right:1px solid #4D4997;border-top:1px solid #4D4997;color:#4d4997;display:block;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:16px;font-weight:400;mso-border-alt:none;padding-bottom:10px;padding-top:10px;text-align:center;text-decoration:none;width:100%;word-break:keep-all;""><span style=""word-break: break-word; padding-left: 20px; padding-right: 20px; font-size: 16px; display: inline-block; letter-spacing: normal;""><span style=""word-break: break-word; line-height: 19.2px;"">Download on</span><span style=""word-break: break-word; line-height: 19.2px;""><br>GooglePlay</span></span></a><!--[if mso]></center></v:textbox></v:roundrect><![endif]--></div>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-5"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #eeeeee; border-left: 20px solid #FFFFFF; border-radius: 0; border-right: 20px solid #FFFFFF; color: #000000; width: 620px; margin: 0 auto;"" width=""620"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<div class=""spacer_block block-1"" style=""height:60px;line-height:60px;font-size:1px;"">&#8202;</div>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-6"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff; color: #000000; width: 620px; margin: 0 auto;"" width=""620"">
										<tbody>
											<tr class=""reverse"">
												<td class=""column column-1 last"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 20px; padding-left: 20px; padding-right: 20px; padding-top: 25px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<div class=""border"">
														<table class=""icons_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: center; line-height: 0;"">
															<tr>
																<td class=""pad"" style=""vertical-align: middle; color: #000000; font-family: inherit; font-size: 14px; font-weight: 400; text-align: center;"">
																	<table class=""icons-outer"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-table;"">
																		<tr>
																			<td style=""vertical-align: middle; text-align: center; padding-top: 0px; padding-bottom: 0px; padding-left: 0px; padding-right: 0px;""><a href=""https://example.com"" target=""_self"" style=""text-decoration: none;""><img class=""icon"" src=""https://d1oco4z2z1fhwp.cloudfront.net/templates/default/8146/yourlogo__10_.png"" alt=""Logo"" height=""auto"" width=""119"" align=""center"" style=""display: block; height: auto; margin: 0 auto; border: 0;""></a></td>
																		</tr>
																	</table>
																</td>
															</tr>
														</table>
														<table class=""text_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
															<tr>
																<td class=""pad"" style=""padding-top:15px;"">
																	<div style=""font-family: sans-serif"">
																		<div class style=""font-size: 12px; font-family: Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif; mso-line-height-alt: 14.399999999999999px; color: #555555; line-height: 1.2;"">
																			<p style=""margin: 0; font-size: 16px; text-align: center; mso-line-height-alt: 19.2px;""><span style=""word-break: break-word; font-size: 14px;"">2023 © All rights reserved</span></p>
																		</div>
																	</div>
																</td>
															</tr>
														</table>
													</div>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-7"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff; color: #000000; width: 620px; margin: 0 auto;"" width=""620"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""icons_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: center; line-height: 0;"">
														<tr>
															<td class=""pad"" style=""vertical-align: middle; color: #1e0e4b; font-family: 'Inter', sans-serif; font-size: 15px; padding-bottom: 5px; padding-top: 5px; text-align: center;""><!--[if vml]><table align=""center"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""display:inline-block;padding-left:0px;padding-right:0px;mso-table-lspace: 0pt;mso-table-rspace: 0pt;""><![endif]-->
																<!--[if !vml]><!-->
																<table class=""icons-inner"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-block; padding-left: 0px; padding-right: 0px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation""><!--<![endif]-->
																	<tr>
																		<td style=""vertical-align: middle; text-align: center; padding-top: 5px; padding-bottom: 5px; padding-left: 5px; padding-right: 6px;""><a href=""http://designedwithbeefree.com/"" target=""_blank"" style=""text-decoration: none;""><img class=""icon"" alt=""Beefree Logo"" src=""https://d1oco4z2z1fhwp.cloudfront.net/assets/Beefree-logo.png"" height=""auto"" width=""34"" align=""center"" style=""display: block; height: auto; margin: 0 auto; border: 0;""></a></td>
																		<td style=""font-family: 'Inter', sans-serif; font-size: 15px; font-weight: undefined; color: #1e0e4b; vertical-align: middle; letter-spacing: undefined; text-align: center; line-height: normal;""><a href=""http://designedwithbeefree.com/"" target=""_blank"" style=""color: #1e0e4b; text-decoration: none;"">Designed with Beefree</a></td>
																	</tr>
																</table>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
				</td>
			</tr>
		</tbody>
	</table><!-- End -->
</body>

</html>";
        result.Append(header);
        result.Append(body);

        return result.ToString();
    }

    private string GeneratePasswordRecoveryHtml(ApplicationUser user, string passwordResetLink)
    {
        var clientInfo = userAgentService.GetClientInfo();
        var result = new StringBuilder();

        var header =
            $@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
  <head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta name=""x-apple-disable-message-reformatting"" />
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
    <meta name=""color-scheme"" content=""light dark"" />
    <meta name=""supported-color-schemes"" content=""light dark"" />
    <title></title>
    <style type=""text/css"" rel=""stylesheet"" media=""all"">
    /* Base ------------------------------ */
    
    @import url(""https://fonts.googleapis.com/css?family=Nunito+Sans:400,700&display=swap"");
    body {{
      width: 100% !important;
      height: 100%;
      margin: 0;
      -webkit-text-size-adjust: none;
    }}
    
    a {{
      color: #3869D4;
    }}
    
    a img {{
      border: none;
    }}
    
    td {{
      word-break: break-word;
    }}
    
    .preheader {{
      display: none !important;
      visibility: hidden;
      mso-hide: all;
      font-size: 1px;
      line-height: 1px;
      max-height: 0;
      max-width: 0;
      opacity: 0;
      overflow: hidden;
    }}
    /* Type ------------------------------ */
    
    body,
    td,
    th {{
      font-family: ""Nunito Sans"", Helvetica, Arial, sans-serif;
    }}
    
    h1 {{
      margin-top: 0;
      color: #333333;
      font-size: 22px;
      font-weight: bold;
      text-align: left;
    }}
    
    h2 {{
      margin-top: 0;
      color: #333333;
      font-size: 16px;
      font-weight: bold;
      text-align: left;
    }}
    
    h3 {{
      margin-top: 0;
      color: #333333;
      font-size: 14px;
      font-weight: bold;
      text-align: left;
    }}
    
    td,
    th {{
      font-size: 16px;
    }}
    
    p,
    ul,
    ol,
    blockquote {{
      margin: .4em 0 1.1875em;
      font-size: 16px;
      line-height: 1.625;
    }}
    
    p.sub {{
      font-size: 13px;
    }}
    /* Utilities ------------------------------ */
    
    .align-right {{
      text-align: right;
    }}
    
    .align-left {{
      text-align: left;
    }}
    
    .align-center {{
      text-align: center;
    }}
    
    .u-margin-bottom-none {{
      margin-bottom: 0;
    }}
    /* Buttons ------------------------------ */
    
    .button {{
      background-color: #3869D4;
      border-top: 10px solid #3869D4;
      border-right: 18px solid #3869D4;
      border-bottom: 10px solid #3869D4;
      border-left: 18px solid #3869D4;
      display: inline-block;
      color: #FFF;
      text-decoration: none;
      border-radius: 3px;
      box-shadow: 0 2px 3px rgba(0, 0, 0, 0.16);
      -webkit-text-size-adjust: none;
      box-sizing: border-box;
    }}
    
    .button--green {{
      background-color: #22BC66;
      border-top: 10px solid #22BC66;
      border-right: 18px solid #22BC66;
      border-bottom: 10px solid #22BC66;
      border-left: 18px solid #22BC66;
    }}
    
    .button--red {{
      background-color: #FF6136;
      border-top: 10px solid #FF6136;
      border-right: 18px solid #FF6136;
      border-bottom: 10px solid #FF6136;
      border-left: 18px solid #FF6136;
    }}
    
    @media only screen and (max-width: 500px) {{
      .button {{
        width: 100% !important;
        text-align: center !important;
      }}
    }}
    /* Attribute list ------------------------------ */
    
    .attributes {{
      margin: 0 0 21px;
    }}
    
    .attributes_content {{
      background-color: #F4F4F7;
      padding: 16px;
    }}
    
    .attributes_item {{
      padding: 0;
    }}
    /* Related Items ------------------------------ */
    
    .related {{
      width: 100%;
      margin: 0;
      padding: 25px 0 0 0;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
    }}
    
    .related_item {{
      padding: 10px 0;
      color: #CBCCCF;
      font-size: 15px;
      line-height: 18px;
    }}
    
    .related_item-title {{
      display: block;
      margin: .5em 0 0;
    }}
    
    .related_item-thumb {{
      display: block;
      padding-bottom: 10px;
    }}
    
    .related_heading {{
      border-top: 1px solid #CBCCCF;
      text-align: center;
      padding: 25px 0 10px;
    }}
    /* Discount Code ------------------------------ */
    
    .discount {{
      width: 100%;
      margin: 0;
      padding: 24px;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
      background-color: #F4F4F7;
      border: 2px dashed #CBCCCF;
    }}
    
    .discount_heading {{
      text-align: center;
    }}
    
    .discount_body {{
      text-align: center;
      font-size: 15px;
    }}
    /* Social Icons ------------------------------ */
    
    .social {{
      width: auto;
    }}
    
    .social td {{
      padding: 0;
      width: auto;
    }}
    
    .social_icon {{
      height: 20px;
      margin: 0 8px 10px 8px;
      padding: 0;
    }}
    /* Data table ------------------------------ */
    
    .purchase {{
      width: 100%;
      margin: 0;
      padding: 35px 0;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
    }}
    
    .purchase_content {{
      width: 100%;
      margin: 0;
      padding: 25px 0 0 0;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
    }}
    
    .purchase_item {{
      padding: 10px 0;
      color: #51545E;
      font-size: 15px;
      line-height: 18px;
    }}
    
    .purchase_heading {{
      padding-bottom: 8px;
      border-bottom: 1px solid #EAEAEC;
    }}
    
    .purchase_heading p {{
      margin: 0;
      color: #85878E;
      font-size: 12px;
    }}
    
    .purchase_footer {{
      padding-top: 15px;
      border-top: 1px solid #EAEAEC;
    }}
    
    .purchase_total {{
      margin: 0;
      text-align: right;
      font-weight: bold;
      color: #333333;
    }}
    
    .purchase_total--label {{
      padding: 0 15px 0 0;
    }}
    
    body {{
      background-color: #F2F4F6;
      color: #51545E;
    }}
    
    p {{
      color: #51545E;
    }}
    
    .email-wrapper {{
      width: 100%;
      margin: 0;
      padding: 0;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
      background-color: #F2F4F6;
    }}
    
    .email-content {{
      width: 100%;
      margin: 0;
      padding: 0;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
    }}
    /* Masthead ----------------------- */
    
    .email-masthead {{
      padding: 25px 0;
      text-align: center;
    }}
    
    .email-masthead_logo {{
      width: 94px;
    }}
    
    .email-masthead_name {{
      font-size: 16px;
      font-weight: bold;
      color: #A8AAAF;
      text-decoration: none;
      text-shadow: 0 1px 0 white;
    }}
    /* Body ------------------------------ */
    
    .email-body {{
      width: 100%;
      margin: 0;
      padding: 0;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
    }}
    
    .email-body_inner {{
      width: 570px;
      margin: 0 auto;
      padding: 0;
      -premailer-width: 570px;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
      background-color: #FFFFFF;
    }}
    
    .email-footer {{
      width: 570px;
      margin: 0 auto;
      padding: 0;
      -premailer-width: 570px;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
      text-align: center;
    }}
    
    .email-footer p {{
      color: #A8AAAF;
    }}
    
    .body-action {{
      width: 100%;
      margin: 30px auto;
      padding: 0;
      -premailer-width: 100%;
      -premailer-cellpadding: 0;
      -premailer-cellspacing: 0;
      text-align: center;
    }}
    
    .body-sub {{
      margin-top: 25px;
      padding-top: 25px;
      border-top: 1px solid #EAEAEC;
    }}
    
    .content-cell {{
      padding: 45px;
    }}
    /*Media Queries ------------------------------ */
    
    @media only screen and (max-width: 600px) {{
      .email-body_inner,
      .email-footer {{
        width: 100% !important;
      }}
    }}
    
    @media (prefers-color-scheme: dark) {{
      body,
      .email-body,
      .email-body_inner,
      .email-content,
      .email-wrapper,
      .email-masthead,
      .email-footer {{
        background-color: #333333 !important;
        color: #FFF !important;
      }}
      p,
      ul,
      ol,
      blockquote,
      h1,
      h2,
      h3,
      span,
      .purchase_item {{
        color: #FFF !important;
      }}
      .attributes_content,
      .discount {{
        background-color: #222 !important;
      }}
      .email-masthead_name {{
        text-shadow: none !important;
      }}
    }}
    
    :root {{
      color-scheme: light dark;
      supported-color-schemes: light dark;
    }}
    </style>
    <!--[if mso]>
    <style type=""text/css"">
      .f-fallback  {{
        font-family: Arial, sans-serif;
      }}
    </style>
  <![endif]-->
  </head>";

        var body = $@" <body>
    <span class=""preheader"">Use this link to reset your password. The link is only valid for 24 hours.</span>
    <table class=""email-wrapper"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
      <tr>
        <td align=""center"">
          <table class=""email-content"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
            <tr>
              <td class=""email-masthead"">
                <a href=""https://example.com"" class=""f-fallback email-masthead_name"">
                {_companyName}
              </a>
              </td>
            </tr>
            <!-- Email Body -->
            <tr>
              <td class=""email-body"" width=""570"" cellpadding=""0"" cellspacing=""0"">
                <table class=""email-body_inner"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <!-- Body content -->
                  <tr>
                    <td class=""content-cell"">
                      <div class=""f-fallback"">
                        <h1>Hi {user.UserInfo.FullName},</h1>
                        <p>You recently requested to reset your password for your {_companyName} account. Use the button below to reset it. <strong>This password reset is only valid for the next 24 hours.</strong></p>
                        <!-- Action -->
                        <table class=""body-action"" align=""center"" width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                          <tr>
                            <td align=""center"">
                              <!-- Border based button
           https://litmus.com/blog/a-guide-to-bulletproof-buttons-in-email-design -->
                              <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" role=""presentation"">
                                <tr>
                                  <td align=""center"">
                                    <a style=""color: black; font-weight: bold"" href={passwordResetLink} class=""f-fallback button button--green"" target=""_blank"">Reset your password</a>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                        <p>For security, this request was received from a <strong>{clientInfo.operatingSystem}</strong> device using <strong>{clientInfo.browser}</strong>. If you did not request a password reset, please ignore this email or <a href=""{{{{support_url}}}}"">contact support</a> if you have questions.</p>
                        <p>Thanks,
                          <br>The <strong>{_companyName}</strong> team</p>
                        <!-- Sub copy -->
                        <table class=""body-sub"" role=""presentation"">
                          <tr>
                            <td>
                              <p class=""f-fallback sub"">If you’re having trouble with the button above, copy and paste the URL below into your web browser.</p>
                              <p class=""f-fallback sub"">{{{{action_url}}}}</p>
                            </td>
                          </tr>
                        </table>
                      </div>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
            <tr>
              <td>
                <table class=""email-footer"" align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                  <tr>
                    <td class=""content-cell"" align=""center"">
                      <p class=""f-fallback sub align-center"">
                        [Company Name, LLC]
                        <br>1234 Street Rd.
                        <br>Suite 1234
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";

        result.Append(header);
        result.Append(body);

        return result.ToString();
    }
}