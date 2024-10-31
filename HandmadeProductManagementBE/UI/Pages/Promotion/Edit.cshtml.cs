using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

    namespace UI.Pages.Promotion
{
    public class EditModel : PageModel
        {
            private readonly ApiResponseHelper _apiHelper;

            public EditModel(ApiResponseHelper apiHelper)
            {
                _apiHelper = apiHelper;
            }
            public string? ErrorMessage { get; set; }
            public string? ErrorDetail { get; set; }

            [BindProperty]
            public PromotionForUpdateDto Promotion { get; set; }
            public string Id { get; set; }

            public async Task<IActionResult> OnGetAsync(string id)
            {
            try
            {
                if (string.IsNullOrEmpty(id))
                    {
                        return NotFound();
                    }
                    Id = id;
                    var response = await _apiHelper.GetAsync<PromotionDto>($"{Constants.ApiBaseUrl}/api/promotions/{id}");
                    if (response != null && response.Data != null)
                    {
                        Promotion = new PromotionForUpdateDto
                        {
                            Name = response.Data.Name,
                            Description = response.Data.Description,
                            DiscountRate = response.Data.DiscountRate,
                            StartDate = response.Data.StartDate,
                            EndDate = response.Data.EndDate
                        };
                        return Page();
                    }
                    else
                    {
                        return NotFound();
                    }


                }
                catch (BaseException.ErrorException ex)
                {
                    ErrorMessage = ex.ErrorDetail.ErrorCode;
                    ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
                }
                catch (Exception ex)
                {
                    ErrorMessage = "An unexpected error occurred.";
                }
                return Page();

            }

            public async Task<IActionResult> OnPostAsync(string id)
            {
            Id = id;
            try
            {


                if (string.IsNullOrEmpty(Id))
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    // Log model state errors for debugging
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                    return Page();
                }

                var updatePayload = new
                {
                    Promotion.Name,
                    Promotion.Description,
                    Promotion.DiscountRate,
                    Promotion.StartDate,
                    Promotion.EndDate
                };

                var response = await _apiHelper.PatchAsync<bool>($"{Constants.ApiBaseUrl}/api/promotions/{Id}", updatePayload);
                if (response != null && response.Data)
                {
                    TempData["SuccessMessage"] = "Promotion updated successfully.";
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while updating the promotion.");
                    return Page();
                }
            }
            catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }

            return Page();
            }
        }
    }

